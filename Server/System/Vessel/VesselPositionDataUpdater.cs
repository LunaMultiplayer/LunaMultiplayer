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
        /// Update the vessel files with position data max at a 2,5 seconds interval
        /// </summary>
        private const int FilePositionUpdateIntervalMs = 2500;

        /// <summary>
        /// Avoid updating the vessel files so often as otherwise the server will lag a lot!
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, DateTime> LastPositionUpdateDictionary = new ConcurrentDictionary<Guid, DateTime>();

        /// <summary>
        /// We received a position information from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later receive an updated vesselproto
        /// </summary>
        public static void WritePositionDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselPositionMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            if (!LastPositionUpdateDictionary.TryGetValue(msgData.VesselId, out var lastUpdated) || (DateTime.Now - lastUpdated).TotalMilliseconds > FilePositionUpdateIntervalMs)
            {
                LastPositionUpdateDictionary.AddOrUpdate(msgData.VesselId, DateTime.Now, (key, existingVal) => DateTime.Now);

                Task.Run(() =>
                {
                    lock (Semaphore.GetOrAdd(msgData.VesselId, new object()))
                    {
                        if (!VesselStoreSystem.CurrentVesselsInXmlFormat.TryGetValue(msgData.VesselId, out var xmlData)) return;

                        var updatedText = UpdateProtoVesselWithNewPositionData(xmlData, msgData);
                        VesselStoreSystem.CurrentVesselsInXmlFormat.TryUpdate(msgData.VesselId, updatedText, xmlData);
                    }
                });
            }
        }

        /// <summary>
        /// Updates the proto vessel with the values we received about a position of a vessel
        /// </summary>
        private static string UpdateProtoVesselWithNewPositionData(string vesselData, VesselPositionMsgData msgData)
        {
            var document = new XmlDocument();
            document.LoadXml(vesselData);

            var node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='lat']");
            if (node != null) node.InnerText = msgData.LatLonAlt[0].ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='lon']");
            if (node != null) node.InnerText = msgData.LatLonAlt[1].ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='alt']");
            if (node != null) node.InnerText = msgData.LatLonAlt[2].ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='hgt']");
            if (node != null) node.InnerText = msgData.HeightFromTerrain.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='nrm']");
            if (node != null)
                node.InnerText = $"{msgData.NormalVector[0].ToString(CultureInfo.InvariantCulture)}," +
                             $"{msgData.NormalVector[1].ToString(CultureInfo.InvariantCulture)}," +
                             $"{msgData.NormalVector[2].ToString(CultureInfo.InvariantCulture)}";

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='rot']");
            if (node != null)
                node.InnerText = $"{msgData.SrfRelRotation[0].ToString(CultureInfo.InvariantCulture)}," +
                             $"{msgData.SrfRelRotation[1].ToString(CultureInfo.InvariantCulture)}," +
                             $"{msgData.SrfRelRotation[2].ToString(CultureInfo.InvariantCulture)}," +
                             $"{msgData.SrfRelRotation[3].ToString(CultureInfo.InvariantCulture)}";

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='INC']");
            if (node != null) node.InnerText = msgData.Orbit[0].ToString(CultureInfo.InvariantCulture);
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='ECC']");
            if (node != null) node.InnerText = msgData.Orbit[1].ToString(CultureInfo.InvariantCulture);
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='SMA']");
            if (node != null) node.InnerText = msgData.Orbit[2].ToString(CultureInfo.InvariantCulture);
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='LAN']");
            if (node != null) node.InnerText = msgData.Orbit[3].ToString(CultureInfo.InvariantCulture);
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='LPE']");
            if (node != null) node.InnerText = msgData.Orbit[4].ToString(CultureInfo.InvariantCulture);
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='MNA']");
            if (node != null) node.InnerText = msgData.Orbit[5].ToString(CultureInfo.InvariantCulture);
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='EPH']");
            if (node != null) node.InnerText = msgData.Orbit[6].ToString(CultureInfo.InvariantCulture);
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ORBIT']/{ConfigNodeXmlParser.ValueNode}[@name='REF']");
            if (node != null) node.InnerText = msgData.Orbit[7].ToString(CultureInfo.InvariantCulture);

            return document.ToIndentedString();
        }
    }
}
