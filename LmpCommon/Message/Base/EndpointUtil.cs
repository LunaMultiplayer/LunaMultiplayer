using System.Net;

namespace LmpCommon.Message.Base
{
    public static class IpEndpointUtil
    {
        public static int GetByteCount(this IPEndPoint endPointToCheck)
        {
            //Lidgren uses 7 bytes to write an endpoint
            return 7;
        }
    }
}
