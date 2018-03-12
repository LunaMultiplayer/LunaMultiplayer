using LunaCommon.Xml;
using System.Collections.Generic;

namespace LunaCommon.ModFile.Structure
{
    public class MandatoryDllFile
    {
        public string FilePath { get; set; }
        public string Sha { get; set; }
    }
    
    public class ModControlStructure
    {
        [XmlComment(Value = "Allow plugins that are NOT listed in the MandatoryPlugins")]
        public bool AllowNonListedPlugins { get; set; } = true;

        [XmlComment(Value = @"Mandatory plugins required to connect to the server. Example:     
        <MandatoryDllFile>
            <FilePath>gcmonitor/KSPUIHelper.dll</FilePath>
            <Sha>18-26-C5-88-E7-EB-E8-DC-A2-1C-F2-BD-B7-73-24-9B-CF-D9-9F-E1-03-91-D2-04-B6-EC-9E-44-BD-9B-46-99</Sha>
        </MandatoryDllFile>
        <MandatoryDllFile>
          <FilePath>gcmonitor/GCMonitor.dll</FilePath>
        </MandatoryDllFile>")]
        public List<MandatoryDllFile> MandatoryPlugins { get; set; } = new List<MandatoryDllFile>();

        [XmlComment(Value = "Forbidden pluggins. Example: <string>forbiddenpluginpath2/forbiddenFile2.dll</string>")]
        public List<string> ForbiddenPlugins { get; set; } = new List<string>();

        [XmlComment(Value = "Allowed parts")]
        public List<string> AllowedParts { get; set; } = new List<string>();
    }
}
