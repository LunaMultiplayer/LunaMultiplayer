using System;
using System.Net;
using System.Threading.Tasks;

namespace LmpMasterServer.Geolocalization
{
    internal interface IGeolocalization
    {
        static Task<string> GetCountryAsync(IPEndPoint endpoint) => throw new NotImplementedException();
    }
}
