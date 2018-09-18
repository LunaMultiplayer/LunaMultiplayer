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
        /// Update the vessel files with update data max at a 2,5 seconds interval
        /// </summary>
        private const int FileUpdateIntervalMs = 2500;

        /// <summary>
        /// Avoid updating the vessel files so often as otherwise the server will lag a lot!
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, DateTime> LastUpdateDictionary = new ConcurrentDictionary<Guid, DateTime>();

        /// <summary>
        /// We received a update information from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later receive an updated vesselproto
        /// </summary>
        public static void WriteUpdateDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselUpdateMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            if (!LastUpdateDictionary.TryGetValue(msgData.VesselId, out var lastUpdated) || (DateTime.Now - lastUpdated).TotalMilliseconds > FileUpdateIntervalMs)
            {
                LastUpdateDictionary.AddOrUpdate(msgData.VesselId, DateTime.Now, (key, existingVal) => DateTime.Now);

                Task.Run(() =>
                {
                    if (!VesselStoreSystem.CurrentVesselsInXmlFormat.TryGetValue(msgData.VesselId, out var xmlData)) return;

                    lock (Semaphore.GetOrAdd(msgData.VesselId, new object()))
                    {
                        var updatedText = UpdateProtoVesselWithNewUpdateData(xmlData, msgData);
                        VesselStoreSystem.CurrentVesselsInXmlFormat.TryUpdate(msgData.VesselId, updatedText, xmlData);
                    }
                });
            }
        }
        
        /// <summary>
        /// Updates the proto vessel with the values we received with values of a vessel
        /// </summary>
        private static string UpdateProtoVesselWithNewUpdateData(string vesselData, VesselUpdateMsgData msgData)
        {
            var document = new XmlDocument();
            document.LoadXml(vesselData);

            var node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='name']");
            if (node != null) node.InnerText = msgData.Name;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='type']");
            if (node != null) node.InnerText = msgData.Type;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='distanceTraveled']");
            if (node != null) node.InnerText = msgData.DistanceTraveled.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='sit']");
            if (node != null) node.InnerText = msgData.Situation;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='landed']");
            if (node != null) node.InnerText = msgData.Landed.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='landedAt']");
            if (node != null) node.InnerText = msgData.LandedAt;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='displaylandedAt']");
            if (node != null) node.InnerText = msgData.DisplayLandedAt;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='splashed']");
            if (node != null) node.InnerText = msgData.Splashed.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='met']");
            if (node != null) node.InnerText = msgData.MissionTime.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='lct']");
            if (node != null) node.InnerText = msgData.LaunchTime.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='lastUT']");
            if (node != null) node.InnerText = msgData.LastUt.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='prst']");
            if (node != null) node.InnerText = msgData.Persistent.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='ref']");
            if (node != null) node.InnerText = msgData.RefTransformId.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='cln']");
            if (node != null) node.InnerText = msgData.AutoClean.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='clnRsn']");
            if (node != null) node.InnerText = msgData.AutoCleanReason;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='ctrl']");
            if (node != null) node.InnerText = msgData.WasControllable.ToString(CultureInfo.InvariantCulture);

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='stg']");
            if (node != null) node.InnerText = msgData.Stage.ToString(CultureInfo.InvariantCulture);

            //NEVER! patch the CoM in the protovessel as then it will be drawn with incorrect CommNet lines!
            //node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='CoM']");
            //if (node != null) node.InnerText = $"{msgData.Com[0].ToString(CultureInfo.InvariantCulture)}," +
            //                                $"{msgData.Com[1].ToString(CultureInfo.InvariantCulture)}," +
            //                                $"{msgData.Com[2].ToString(CultureInfo.InvariantCulture)}";

            return document.ToIndentedString();
        }
    }
}
