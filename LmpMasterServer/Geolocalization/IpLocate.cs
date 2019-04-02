using LmpMasterServer.Geolocalization.Base;
using System;
using System.Net;
using System.Net.Http;

namespace LmpMasterServer.Geolocalization
{
    internal class IpLocate : BaseGeolocalization
    {
        public static string GetCountry(IPEndPoint externalEndpoint)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    dynamic output = JsonReader.Read(client.GetStringAsync($"https://www.iplocate.io/api/lookup/{externalEndpoint.Address}").Result);
                    return output["country_code"];
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
