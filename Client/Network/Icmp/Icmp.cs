using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LunaClient.Network.Icmp
{
    public class Icmp
    {
        #region Fields & properties

        private IPAddress _mHost;
        protected IPAddress Host
        {
            get => _mHost;
            set => _mHost = value ?? throw new ArgumentNullException();
        }
        protected Socket ClientSocket { get; set; }
        protected DateTime StartTime { get; set; }
        protected Timer PingTimeOut { get; set; }
        protected bool HasTimedOut { get; set; }

        #endregion

        #region Constructor & Destructor

        public Icmp(IPAddress host)
        {
            Host = host;
        }
        
        ~Icmp()
        {
            ClientSocket?.Close();
        }

        #endregion

        #region Public

        public TimeSpan Ping()
        {
            return Ping(1000);
        }

        public TimeSpan Ping(int timeout)
        {
            EndPoint remoteEp = new IPEndPoint(Host, 0);
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            // Get the ICMP message
            var buffer = GetEchoMessageBuffer();
            // Set the timeout timer
            HasTimedOut = false;
            PingTimeOut = new Timer(PingTimedOut, null, timeout, Timeout.Infinite);
            // Get the current time
            StartTime = DateTime.Now;
            // Send the ICMP message and receive the reply
            try
            {
                if (ClientSocket.SendTo(buffer, remoteEp) <= 0)
                    throw new SocketException();
                buffer = new byte[buffer.Length + 20];
                if (ClientSocket.ReceiveFrom(buffer, ref remoteEp) <= 0)
                    throw new SocketException();
            }
            catch (SocketException)
            {
                if (HasTimedOut)
                    return TimeSpan.MaxValue;
                else
                    throw;
            }
            finally
            {
                // Close the socket
                ClientSocket.Close();
                ClientSocket = null;
                // destroy the timer
                PingTimeOut.Change(Timeout.Infinite, Timeout.Infinite);
                PingTimeOut.Dispose();
                PingTimeOut = null;
            }
            // Get the time
            var ret = DateTime.Now.Subtract(StartTime);
            return ret;
        }

        #endregion

        #region Protected

        protected byte[] GetEchoMessageBuffer()
        {
            var message = new EchoMessage
            {
                Type = 8,
                Data = new byte[32]
            };

            // ICMP echo
            for (var i = 0; i < 32; i++)
            {
                message.Data[i] = 32;   // Send spaces
            }

            message.CheckSum = message.GetChecksum();
            return message.GetObjectBytes();
        }
        
        protected void PingTimedOut(object state)
        {
            HasTimedOut = true;
            ClientSocket?.Close();
        }

        #endregion
    }
}
