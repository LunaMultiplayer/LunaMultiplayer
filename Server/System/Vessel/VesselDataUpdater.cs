using LunaCommon.Xml;
using Server.Log;
using Server.Settings.Structures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Server.System.Vessel
{
    /// <summary>
    /// We try to avoid working with protovessels as much as possible as they can be huge files.
    /// This class patches the vessel file with the information messages we receive about a position and other vessel properties.
    /// This way we send the whole vessel definition only when there are parts that have changed 
    /// </summary>
    public partial class VesselDataUpdater
    {
        #region Semaphore

        /// <summary>
        /// To not overwrite our own data we use a lock
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, object> Semaphore = new ConcurrentDictionary<Guid, object>();

        #endregion
        
        /// <summary>
        /// Raw updates a vessel in the dictionary and takes care of the locking in case we received another vessel message type
        /// </summary>
        public static void RawConfigNodeInsertOrUpdate(Guid vesselId, string vesselDataInConfigNodeFormat)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd(vesselId, new object()))
                {
                    var vesselAsXml = ConfigNodeXmlParser.ConvertToXml(vesselDataInConfigNodeFormat);

                    if (GeneralSettings.SettingsStore.ModControl)
                    {
                        var vesselParts = GetPartNames(vesselAsXml);
                        if (vesselParts != null)
                        {
                            var bannedParts = vesselParts.Except(ModFileSystem.ModControl.AllowedParts);
                            if (bannedParts.Any())
                            {
                                LunaLog.Warning($"Received a vessel with BANNED parts! {vesselId}");
                                return;
                            }
                        }
                    }

                    VesselStoreSystem.CurrentVesselsInXmlFormat.AddOrUpdate(vesselId, vesselAsXml, (key, existingVal) => vesselAsXml);
                }
            });
        }

        private static IEnumerable<string> GetPartNames(string vesselData)
        {
            var document = new XmlDocument();
            document.LoadXml(vesselData);

            var xpath = $@"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='PART']/{ConfigNodeXmlParser.ValueNode}[@name='name']";
            var parts = document.SelectNodes(xpath);

            return parts?.Cast<XmlNode>().Select(n => n.InnerText).Distinct();
        }
    }
}
