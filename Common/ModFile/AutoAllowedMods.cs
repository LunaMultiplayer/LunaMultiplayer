using System.Collections.Generic;

namespace LunaCommon.ModFile
{
    public static class AutoAllowedMods
    {
        public static List<string> AllowedMods = new List<string>
        {
            "lunamultiplayer/plugins/fastmember.dll",
            "lunamultiplayer/plugins/lidgren.network.dll",
            "lunamultiplayer/plugins/lunaclient.dll",
            "lunamultiplayer/plugins/lunacommon.dll",
            "lunamultiplayer/plugins/mono.data.tds.dll",
            "lunamultiplayer/plugins/system.data.dll",
            "lunamultiplayer/plugins/system.threading.dll",
            "lunamultiplayer/plugins/system.transactions.dll"
        };
    }
}
