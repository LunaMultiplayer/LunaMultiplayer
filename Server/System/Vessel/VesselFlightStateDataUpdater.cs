using LunaCommon.Message.Data.Vessel;
using LunaCommon.Xml;
using Server.Utilities;
using System;
using System.Collections.Concurrent;
using System.Globalization;
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
        /// <summary>
        /// Update the vessel files with flightstate data max at a 2,5 seconds interval
        /// </summary>
        private const int FileFlightStateUpdateIntervalMs = 2500;

        /// <summary>
        /// Avoid updating the vessel files so often as otherwise the server will lag a lot!
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, DateTime> LastFlightStateUpdateDictionary = new ConcurrentDictionary<Guid, DateTime>();

        /// <summary>
        /// We received a flight state information from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later receive an updated vesselproto
        /// </summary>
        public static void WriteFlightstateDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselFlightStateMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            if (!LastFlightStateUpdateDictionary.TryGetValue(msgData.VesselId, out var lastUpdated) || (DateTime.Now - lastUpdated).TotalMilliseconds > FileFlightStateUpdateIntervalMs)
            {
                LastFlightStateUpdateDictionary.AddOrUpdate(msgData.VesselId, DateTime.Now, (key, existingVal) => DateTime.Now);

                Task.Run(() =>
                {
                    lock (Semaphore.GetOrAdd(msgData.VesselId, new object()))
                    {
                        if (!VesselStoreSystem.CurrentVesselsInXmlFormat.TryGetValue(msgData.VesselId, out var xmlData)) return;

                        var updatedText = UpdateProtoVesselWithNewFlightStateData(xmlData, msgData);
                        VesselStoreSystem.CurrentVesselsInXmlFormat.TryUpdate(msgData.VesselId, updatedText, xmlData);
                    }
                });
            }
        }

        /// <summary>
        /// Updates the proto vessel with the values we received about a flight state of a vessel
        /// </summary>
        private static string UpdateProtoVesselWithNewFlightStateData(string vesselData, VesselFlightStateMsgData msgData)
        {
            var document = new XmlDocument();
            document.LoadXml(vesselData);

            var node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='CTRLSTATE']/{ConfigNodeXmlParser.ValueNode}[@name='pitch']");
            if (node != null) node.InnerText = msgData.Pitch.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='CTRLSTATE']/{ConfigNodeXmlParser.ValueNode}[@name='yaw']");
            if (node != null) node.InnerText = msgData.Yaw.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='CTRLSTATE']/{ConfigNodeXmlParser.ValueNode}[@name='roll']");
            if (node != null) node.InnerText = msgData.Roll.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='CTRLSTATE']/{ConfigNodeXmlParser.ValueNode}[@name='trimPitch']");
            if (node != null) node.InnerText = msgData.PitchTrim.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='CTRLSTATE']/{ConfigNodeXmlParser.ValueNode}[@name='trimYaw']");
            if (node != null) node.InnerText = msgData.YawTrim.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='CTRLSTATE']/{ConfigNodeXmlParser.ValueNode}[@name='trimRoll']");
            if (node != null) node.InnerText = msgData.RollTrim.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='CTRLSTATE']/{ConfigNodeXmlParser.ValueNode}[@name='mainThrottle']");
            if (node != null) node.InnerText = msgData.MainThrottle.ToString(CultureInfo.InvariantCulture);

            return document.ToIndentedString();
        }
    }
}
