using LunaCommon.Xml;
using Server.Context;
using Server.Settings;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Server.System
{
    /// <summary>
    /// Here we keep a copy of all the player vessels in XML format and we also save them to files at a specified rate
    /// </summary>
    public class VesselStoreSystem
    {
        public static string VesselsFolder = Path.Combine(ServerContext.UniverseDirectory, "Vessels");

        public static ConcurrentDictionary<Guid, string> CurrentVesselsInXmlFormat = new ConcurrentDictionary<Guid, string>();

        private static bool _currentlyWriting;

        public static void AddUpdateVesselInConfigNodeFormat(Guid vesselId, string configNodeVesselData)
        {
            var vesselAsXml = ConfigNodeXmlParser.ConvertToXml(configNodeVesselData);

            CurrentVesselsInXmlFormat.AddOrUpdate(vesselId, vesselAsXml, (key, existingVal) => vesselAsXml);
        }

        public static bool VesselExists(Guid vesselId) => CurrentVesselsInXmlFormat.ContainsKey(vesselId);

        public static void RemoveVessel(Guid vesselId)
        {
            CurrentVesselsInXmlFormat.TryRemove(vesselId, out _);

            Task.Run(() =>
            {
                while (_currentlyWriting)
                {
                    Thread.Sleep(100);
                }

                FileHandler.FileDelete(Path.Combine(VesselsFolder, $"{vesselId}.xml"));
            });
        }
        
        public static string GetVesselInConfigNodeFormat(Guid vesselId)
        {
            return CurrentVesselsInXmlFormat.TryGetValue(vesselId, out var vesselInXmlFormat) ? 
                ConfigNodeXmlParser.ConvertToConfigNode(vesselInXmlFormat) : null;
        }

        /// <summary>
        /// Load the stored vessels into the dictionary
        /// </summary>
        public static void LoadExistingVessels()
        {
            foreach (var file in Directory.GetFiles(VesselsFolder).Where(f=> Path.GetExtension(f) == ".xml"))
            {
                if (Guid.TryParse(Path.GetFileNameWithoutExtension(file), out var vesselId))
                {
                    CurrentVesselsInXmlFormat.TryAdd(vesselId, FileHandler.ReadFileText(file));
                }
            }
        }

        /// <summary>
        /// This multithreaded function backups the vessels from the internal dictionary to a file at a specified interval
        /// </summary>
        public static void BackupVesselsThread()
        {
            while (ServerContext.ServerRunning)
            {
                _currentlyWriting = true;

                var vesselsInXml = CurrentVesselsInXmlFormat.ToArray();
                foreach (var vessel in vesselsInXml)
                {
                    FileHandler.WriteToFile(Path.Combine(VesselsFolder, $"{vessel.Key}.xml"), vessel.Value);
                }
                _currentlyWriting = false;

                Thread.Sleep(GeneralSettings.SettingsStore.VesselsBackupIntervalMs);
            }
        }
    }
}
