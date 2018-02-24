using LunaCommon.Message.Data.Vessel;
using LunaCommon.Xml;
using Server.Utilities;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;

namespace Server.System
{
    /// <summary>
    /// We try to avoid working with protovessels as much as possible as they can be huge files.
    /// This class patches the vessel file with the information messages we receive about a position and other vessel properties.
    /// This way we send the whole vessel definition only when there are parts that have changed 
    /// </summary>
    public class VesselDataUpdater
    {
        #region Constants and update dictionaries

        #region Position

        /// <summary>
        /// Update the vessel files with position data max at a 2,5 seconds interval
        /// </summary>
        private const int FilePositionUpdateIntervalMs = 2500;

        /// <summary>
        /// Avoid updating the vessel files so often as otherwise the server will lag a lot!
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, DateTime> LastPositionUpdateDictionary = new ConcurrentDictionary<Guid, DateTime>();

        #endregion

        #region Updates

        /// <summary>
        /// Update the vessel files with update data max at a 2,5 seconds interval
        /// </summary>
        private const int FileUpdateIntervalMs = 2500;

        /// <summary>
        /// Avoid updating the vessel files so often as otherwise the server will lag a lot!
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, DateTime> LastUpdateDictionary = new ConcurrentDictionary<Guid, DateTime>();

        #endregion

        #region Resources

        /// <summary>
        /// Update the vessel files with resource data max at a 2,5 seconds interval
        /// </summary>
        private const int FileResourcesUpdateIntervalMs = 2500;

        /// <summary>
        /// Avoid updating the vessel files so often as otherwise the server will lag a lot!
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, DateTime> LastResourcesUpdateDictionary = new ConcurrentDictionary<Guid, DateTime>();

        #endregion

        #endregion

        #region Semaphore

        /// <summary>
        /// To not overwrite our own data we use a lock
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, object> Semaphore = new ConcurrentDictionary<Guid, object>();

        #endregion

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
        /// We received a resource information from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later received an update vesselproto
        /// </summary>
        public static void WriteResourceDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselResourceMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            if (!LastResourcesUpdateDictionary.TryGetValue(msgData.VesselId, out var lastUpdated) || (DateTime.Now - lastUpdated).TotalMilliseconds > FileResourcesUpdateIntervalMs)
            {
                LastResourcesUpdateDictionary.AddOrUpdate(msgData.VesselId, DateTime.Now, (key, existingVal) => DateTime.Now);

                Task.Run(() =>
                {
                    lock (Semaphore.GetOrAdd(msgData.VesselId, new object()))
                    {
                        if (!VesselStoreSystem.CurrentVesselsInXmlFormat.TryGetValue(msgData.VesselId, out var xmlData)) return;

                        var updatedText = UpdateProtoVesselFileWithNewResourceData(xmlData, msgData);
                        VesselStoreSystem.CurrentVesselsInXmlFormat.TryUpdate(msgData.VesselId, updatedText, xmlData);
                    }
                });
            }
        }

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
                    VesselStoreSystem.CurrentVesselsInXmlFormat.AddOrUpdate(vesselId, vesselAsXml, (key, existingVal) => vesselAsXml);
                }
            });
        }

        /// <summary>
        /// We received a part module change from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later receive an updated vesselproto
        /// </summary>
        public static void WriteModuleDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselPartSyncMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            //Sync part changes ALWAYS and ignore the rate they arrive
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd(msgData.VesselId, new object()))
                {
                    if (!VesselStoreSystem.CurrentVesselsInXmlFormat.TryGetValue(msgData.VesselId, out var xmlData)) return;

                    var updatedText = UpdateProtoVesselFileWithNewPartModulesData(xmlData, msgData);
                    VesselStoreSystem.CurrentVesselsInXmlFormat.TryUpdate(msgData.VesselId, updatedText, xmlData);
                }
            });
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

            //NEVER! patch the CoM in the protovessel as then it will be drawn with incorrect CommNet lines!
            //node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='CoM']");
            //if (node != null) node.InnerText = $"{msgData.Com[0].ToString(CultureInfo.InvariantCulture)}," +
            //                                $"{msgData.Com[1].ToString(CultureInfo.InvariantCulture)}," +
            //                                $"{msgData.Com[2].ToString(CultureInfo.InvariantCulture)}";

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/ORBIT/{ConfigNodeXmlParser.ValueNode}[@name='INC']");
            if (node != null) node.InnerText = msgData.Orbit[0].ToString(CultureInfo.InvariantCulture);
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/ORBIT/{ConfigNodeXmlParser.ValueNode}[@name='ECC']");
            if (node != null) node.InnerText = msgData.Orbit[1].ToString(CultureInfo.InvariantCulture);
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/ORBIT/{ConfigNodeXmlParser.ValueNode}[@name='SMA']");
            if (node != null) node.InnerText = msgData.Orbit[2].ToString(CultureInfo.InvariantCulture);
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/ORBIT/{ConfigNodeXmlParser.ValueNode}[@name='LAN']");
            if (node != null) node.InnerText = msgData.Orbit[3].ToString(CultureInfo.InvariantCulture);
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/ORBIT/{ConfigNodeXmlParser.ValueNode}[@name='LPE']");
            if (node != null) node.InnerText = msgData.Orbit[4].ToString(CultureInfo.InvariantCulture);
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/ORBIT/{ConfigNodeXmlParser.ValueNode}[@name='MNA']");
            if (node != null) node.InnerText = msgData.Orbit[5].ToString(CultureInfo.InvariantCulture);
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/ORBIT/{ConfigNodeXmlParser.ValueNode}[@name='EPH']");
            if (node != null) node.InnerText = msgData.Orbit[6].ToString(CultureInfo.InvariantCulture);
            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/ORBIT/{ConfigNodeXmlParser.ValueNode}[@name='REF']");
            if (node != null) node.InnerText = msgData.Orbit[7].ToString(CultureInfo.InvariantCulture);

            return document.ToIndentedString();
        }

        /// <summary>
        /// Updates the proto vessel with the values we received about a position of a vessel
        /// </summary>
        private static string UpdateProtoVesselWithNewUpdateData(string vesselData, VesselUpdateMsgData msgData)
        {
            var document = new XmlDocument();
            document.LoadXml(vesselData);

            var node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='name']");
            if (node != null) node.InnerText = msgData.Name;

            node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='type']");
            if (node != null) node.InnerText = msgData.Type;

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

            foreach (var actionGroup in msgData.ActionGroups)
            {
                node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/ACTIONGROUPS/{ConfigNodeXmlParser.ValueNode}[@name='{actionGroup.ActionGroupName}']");
                if (node != null) node.InnerText = $"{actionGroup.State.ToString(CultureInfo.InvariantCulture)}, {actionGroup.Time.ToString(CultureInfo.InvariantCulture)}";
            }

            return document.ToIndentedString();
        }

        /// <summary>
        /// Updates the proto vessel with the values we received about the resources of a vessel
        /// </summary>
        private static string UpdateProtoVesselFileWithNewResourceData(string vesselData, VesselResourceMsgData msgData)
        {
            var document = new XmlDocument();
            document.LoadXml(vesselData);

            foreach (var resourceInfo in msgData.Resources)
            {
                var xpath = $@"/{ConfigNodeXmlParser.StartElement}/PART/{ConfigNodeXmlParser.ValueNode}[@name='uid' and text()=""{resourceInfo.PartFlightId}""]/" +
                    $"following-sibling::RESOURCE/{ConfigNodeXmlParser.ValueNode}" +
                    $@"[@name='name' and text()=""{resourceInfo.ResourceName}""]/parent::RESOURCE";

                var resourceNode = document.SelectSingleNode(xpath);

                var amountNode = resourceNode?.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/PART/RESOURCE/{ConfigNodeXmlParser.ValueNode}[@name='amount']");
                if (amountNode != null) amountNode.InnerText = resourceInfo.Amount.ToString(CultureInfo.InvariantCulture);

                var flowNode = resourceNode?.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/PART/RESOURCE/{ConfigNodeXmlParser.ValueNode}[@name='flowState']");
                if (flowNode != null) flowNode.InnerText = resourceInfo.FlowState.ToString(CultureInfo.InvariantCulture);

            }

            return document.ToIndentedString();
        }

        /// <summary>
        /// Updates the proto vessel with the values we received about a part module change of a vessel
        /// </summary>
        private static string UpdateProtoVesselFileWithNewPartModulesData(string vesselData, VesselPartSyncMsgData msgData)
        {
            var document = new XmlDocument();
            document.LoadXml(vesselData);

            var xpath = $@"/{ConfigNodeXmlParser.StartElement}/PART/{ConfigNodeXmlParser.ValueNode}[@name='uid' and text()=""{msgData.PartFlightId}""]/" +
                        $"following-sibling::MODULE/{ConfigNodeXmlParser.ValueNode}" +
                        $@"[@name='name' and text()=""{msgData.ModuleName}""]/parent::MODULE/{ConfigNodeXmlParser.ValueNode}[@name='{msgData.FieldName}']";

            var fieldNode = document.SelectSingleNode(xpath);
            if (fieldNode != null) fieldNode.InnerText = msgData.Value;

            return document.ToIndentedString();
        }
    }
}
