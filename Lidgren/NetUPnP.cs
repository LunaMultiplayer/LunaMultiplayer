using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Net.Sockets;
#if !NET472
using System.Net.Http;
#endif
using System.Threading;

#if !__NOIPENDPOINT__
using NetEndPoint = System.Net.IPEndPoint;
#endif

namespace Lidgren.Network
{
	/// <summary>
	/// Status of the UPnP capabilities
	/// </summary>
	public enum UPnPStatus
	{
		/// <summary>
		/// Still discovering UPnP capabilities
		/// </summary>
		Discovering,

		/// <summary>
		/// UPnP is not available
		/// </summary>
		NotAvailable,

		/// <summary>
		/// UPnP is available and ready to use
		/// </summary>
		Available
	}

	/// <summary>
	/// UPnP support class
	/// </summary>
	public class NetUPnP
	{
#if !NET472
		private static readonly HttpClient s_httpClient = new HttpClient();
#endif
		private const int c_discoveryTimeOutMillis = 1000;

		private string m_serviceUrl;
		private string m_serviceName = "";
		private NetPeer m_peer;
		private ManualResetEvent m_discoveryComplete = new ManualResetEvent(false);

		internal double m_discoveryResponseDeadline;

		private UPnPStatus m_status;

		/// <summary>
		/// Status of the UPnP capabilities of this NetPeer
		/// </summary>
		public UPnPStatus Status { get { return m_status; } }

		/// <summary>
		/// NetUPnP constructor
		/// </summary>
		public NetUPnP(NetPeer peer)
		{
			m_peer = peer;
			m_discoveryResponseDeadline = double.MinValue;
		}

		internal void Discover(NetPeer peer)
		{
			string str =
"M-SEARCH * HTTP/1.1\r\n" +
"HOST: 239.255.255.250:1900\r\n" +
"ST:upnp:rootdevice\r\n" +
"MAN:\"ssdp:discover\"\r\n" +
"MX:3\r\n\r\n";

			m_discoveryResponseDeadline = NetTime.Now + 6.0; // arbitrarily chosen number, router gets 6 seconds to respond
			m_status = UPnPStatus.Discovering;

			byte[] arr = System.Text.Encoding.UTF8.GetBytes(str);

			m_peer.LogDebug("Attempting UPnP discovery");
			peer.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
			peer.RawSend(arr, 0, arr.Length, new NetEndPoint(NetUtility.GetBroadcastAddress(), 1900));
			peer.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);
		}

	    internal void CheckForDiscoveryTimeout()
	    {
	        if ((m_status != UPnPStatus.Discovering) || (NetTime.Now < m_discoveryResponseDeadline))
                return;
	        m_peer.LogDebug("UPnP discovery timed out");
	        m_status = UPnPStatus.NotAvailable;
	    }

		internal void ExtractServiceUrl(string resp)
		{
#if !DEBUG
			try
			{
#endif
			XmlDocument desc = new XmlDocument();
#if !NET472
            using (var responseStream = s_httpClient.GetStreamAsync(resp).GetAwaiter().GetResult())
                desc.Load(responseStream);
#else
			var request = WebRequest.Create(resp);
			request.Method = "GET";
			using (var response = request.GetResponse())
			using (var responseStream = response.GetResponseStream())
				desc.Load(responseStream);
#endif

			XmlNamespaceManager nsMgr = new XmlNamespaceManager(desc.NameTable);
			nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
			XmlNode typen = desc.SelectSingleNode("//tns:device/tns:deviceType/text()", nsMgr);
			if (!typen.Value.Contains("InternetGatewayDevice"))
				return;

			m_serviceName = "WANIPConnection";
			XmlNode node = desc.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:" + m_serviceName + ":1\"]/tns:controlURL/text()", nsMgr);
			if (node == null)
			{
				//try another service name
				m_serviceName = "WANPPPConnection";
				node = desc.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:" + m_serviceName + ":1\"]/tns:controlURL/text()", nsMgr);
				if (node == null)
					return;
			}

			m_serviceUrl = CombineUrls(resp, node.Value);
			m_peer.LogDebug("UPnP service ready");
			m_status = UPnPStatus.Available;
			m_discoveryComplete.Set();
#if !DEBUG
			}
			catch
			{
				m_peer.LogVerbose("Exception ignored trying to parse UPnP XML response");
				return;
			}
#endif
		}

		private static string CombineUrls(string gatewayURL, string subURL)
		{
			// Is Control URL an absolute URL?
			if ((subURL.Contains("http:")) || (subURL.Contains(".")))
				return subURL;

			gatewayURL = gatewayURL.Replace("http://", "");  // strip any protocol
			int n = gatewayURL.IndexOf("/");
			if (n != -1)
				gatewayURL = gatewayURL.Substring(0, n);  // Use first portion of URL
			return "http://" + gatewayURL + subURL;
		}

		private bool CheckAvailability()
		{
			switch (m_status)
			{
				case UPnPStatus.NotAvailable:
					return false;
				case UPnPStatus.Available:
					return true;
				case UPnPStatus.Discovering:
					if (m_discoveryComplete.WaitOne(c_discoveryTimeOutMillis))
						return true;
					if (NetTime.Now > m_discoveryResponseDeadline)
						m_status = UPnPStatus.NotAvailable;
					return false;
			}
			return false;
        }

        /// <summary>
        /// Add a forwarding rule to the router using UPnP
        /// </summary>
        /// <param name="externalPort">The external, WAN facing, port</param>
        /// <param name="description">A description for the port forwarding rule</param>
        /// <param name="internalPort">The port on the client machine to send traffic to</param>
        public bool ForwardPort(int externalPort, string description, int internalPort = 0)
        {
            if (!CheckAvailability())
                return false;

            IPAddress mask;
            var client = NetUtility.GetMyAddress(out mask);
            if (client == null)
                return false;

            if (internalPort == 0)
                internalPort = externalPort;

            try
            {
                SOAPRequest(m_serviceUrl,
                    "<u:AddPortMapping xmlns:u=\"urn:schemas-upnp-org:service:" + m_serviceName + ":1\">" +
                    "<NewRemoteHost></NewRemoteHost>" +
                    "<NewExternalPort>" + externalPort.ToString() + "</NewExternalPort>" +
                    "<NewProtocol>" + ProtocolType.Udp.ToString().ToUpper(System.Globalization.CultureInfo.InvariantCulture) + "</NewProtocol>" +
                    "<NewInternalPort>" + internalPort.ToString() + "</NewInternalPort>" +
                    "<NewInternalClient>" + client.ToString() + "</NewInternalClient>" +
                    "<NewEnabled>1</NewEnabled>" +
                    "<NewPortMappingDescription>" + description + "</NewPortMappingDescription>" +
                    "<NewLeaseDuration>0</NewLeaseDuration>" +
                    "</u:AddPortMapping>",
                    "AddPortMapping");

                m_peer.LogDebug("Sent UPnP port forward request");
                NetUtility.Sleep(50);
            }
            catch (Exception ex)
            {
                m_peer.LogWarning("UPnP port forward failed: " + ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Delete a forwarding rule from the router using UPnP
        /// </summary>
        /// <param name="externalPort">The external, 'internet facing', port</param>
        public bool DeleteForwardingRule(int externalPort)
        {
            if (!CheckAvailability())
                return false;

            try
            {
                SOAPRequest(m_serviceUrl,
                "<u:DeletePortMapping xmlns:u=\"urn:schemas-upnp-org:service:" + m_serviceName + ":1\">" +
                "<NewRemoteHost>" +
                "</NewRemoteHost>" +
                "<NewExternalPort>" + externalPort + "</NewExternalPort>" +
                "<NewProtocol>" + ProtocolType.Udp.ToString().ToUpper(System.Globalization.CultureInfo.InvariantCulture) + "</NewProtocol>" +
                "</u:DeletePortMapping>", "DeletePortMapping");
                return true;
            }
            catch (Exception ex)
            {
                m_peer.LogWarning("UPnP delete forwarding rule failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Retrieve the extern ip using UPnP
        /// </summary>
        public IPAddress GetExternalIP()
		{
			if (!CheckAvailability())
				return null;
			try
			{
				XmlDocument xdoc = SOAPRequest(m_serviceUrl, "<u:GetExternalIPAddress xmlns:u=\"urn:schemas-upnp-org:service:" + m_serviceName + ":1\">" +
				"</u:GetExternalIPAddress>", "GetExternalIPAddress");
				XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
				nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
				string IP = xdoc.SelectSingleNode("//NewExternalIPAddress/text()", nsMgr).Value;
				return IPAddress.Parse(IP);
			}
			catch (Exception ex)
			{
				m_peer.LogWarning("Failed to get external IP: " + ex.Message);
				return null;
			}
		}

		private XmlDocument SOAPRequest(string url, string soap, string function)
		{
			string req = "<?xml version=\"1.0\"?>" +
			"<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
			"<s:Body>" +
			soap +
			"</s:Body>" +
			"</s:Envelope>";

#if !NET472
			using (var request = new HttpRequestMessage(HttpMethod.Post, url))
			{
				request.Headers.Add("SOAPACTION", "\"urn:schemas-upnp-org:service:" + m_serviceName + ":1#" + function + "\"");
				request.Content = new StringContent(req, System.Text.Encoding.UTF8, "text/xml");
				request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml");
				request.Content.Headers.ContentType.CharSet = "utf-8";

				using (var response = s_httpClient.SendAsync(request).GetAwaiter().GetResult())
				using (var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
				{
					XmlDocument respObj = new XmlDocument();
					respObj.Load(stream);
					return respObj;
				}
			}
#else
			var request = WebRequest.Create(url);
			request.Method = "POST";
			byte[] body = System.Text.Encoding.UTF8.GetBytes(req);
			request.Headers.Add("SOAPACTION", "\"urn:schemas-upnp-org:service:" + m_serviceName + ":1#" + function + "\"");
			request.ContentType = "text/xml; charset=\"utf-8\"";
			request.ContentLength = body.Length;

			using (var requestStream = request.GetRequestStream())
				requestStream.Write(body, 0, body.Length);

			using (var response = request.GetResponse())
			using (var responseStream = response.GetResponseStream())
			{
				XmlDocument respObj = new XmlDocument();
				respObj.Load(responseStream);
				return respObj;
			}
#endif
		}
	}
}