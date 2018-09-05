using LunaCommon.Xml;
using Server.Context;
using Server.Events;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Server.System
{
    /// <summary>
    /// Here we keep a copy of all the player vessels in XML format and we also save them to files at a specified rate
    /// </summary>
    public static class VesselStoreSystem
    {
        public static string VesselsPath = Path.Combine(ServerContext.UniverseDirectory, "Vessels");

        public static ConcurrentDictionary<Guid, string> CurrentVesselsInXmlFormat = new ConcurrentDictionary<Guid, string>();

        private static readonly object BackupLock = new object();

        static VesselStoreSystem() => ExitEvent.ServerClosing += BackupVessels;

        public static bool VesselExists(Guid vesselId) => CurrentVesselsInXmlFormat.ContainsKey(vesselId);

        /// <summary>
        /// Removes a vessel from the store
        /// </summary>
        public static void RemoveVessel(Guid vesselId)
        {
            CurrentVesselsInXmlFormat.TryRemove(vesselId, out _);

            Task.Run(() =>
            {
                lock (BackupLock)
                {
                    FileHandler.FileDelete(Path.Combine(VesselsPath, $"{vesselId}.xml"));
                }
            });
        }

        /// <summary>
        /// Returns a XML vessel in the standard KSP format
        /// </summary>
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
            lock (BackupLock)
            {
                foreach (var file in Directory.GetFiles(VesselsPath).Where(f => Path.GetExtension(f) == ".xml"))
                {
                    if (Guid.TryParse(Path.GetFileNameWithoutExtension(file), out var vesselId))
                    {
                        CurrentVesselsInXmlFormat.TryAdd(vesselId, FileHandler.ReadFileText(file));
                    }
                }
            }
        }

        /// <summary>
        /// Actually performs the backup of the vessels to file
        /// </summary>
        public static void BackupVessels()
        {
            lock (BackupLock)
            {
                var vesselsInXml = CurrentVesselsInXmlFormat.ToArray();
                foreach (var vessel in vesselsInXml)
                {
                    FileHandler.WriteToFile(Path.Combine(VesselsPath, $"{vessel.Key}.xml"), vessel.Value);
                }
            }
        }
    }
}
