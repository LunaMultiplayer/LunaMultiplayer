using LmpCommon.Message.Data.Vessel;
using LmpCommon.Xml;
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
        /// Update the vessel files with resource data max at a 2,5 seconds interval
        /// </summary>
        private const int FileResourcesUpdateIntervalMs = 2500;

        /// <summary>
        /// Avoid updating the vessel files so often as otherwise the server will lag a lot!
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, DateTime> LastResourcesUpdateDictionary = new ConcurrentDictionary<Guid, DateTime>();


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
        /// Updates the proto vessel with the values we received about the resources of a vessel
        /// </summary>
        private static string UpdateProtoVesselFileWithNewResourceData(string vesselData, VesselResourceMsgData msgData)
        {
            var document = new XmlDocument();
            document.LoadXml(vesselData);

            foreach (var resourceInfo in msgData.Resources)
            {
                var xpath = $@"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='PART']/{ConfigNodeXmlParser.ValueNode}[@name='uid' and text()=""{resourceInfo.PartFlightId}""]/" +
                            $"following-sibling::RESOURCE/{ConfigNodeXmlParser.ValueNode}" +
                            $@"[@name='name' and text()=""{resourceInfo.ResourceName}""]/parent::RESOURCE";

                var resourceNode = document.SelectSingleNode(xpath);

                var amountNode = resourceNode?.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='PART']/{ConfigNodeXmlParser.ParentNode}[@name='RESOURCE']/{ConfigNodeXmlParser.ValueNode}[@name='amount']");
                if (amountNode != null) amountNode.InnerText = resourceInfo.Amount.ToString(CultureInfo.InvariantCulture);

                var flowNode = resourceNode?.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='PART']/{ConfigNodeXmlParser.ParentNode}[@name='RESOURCE']/{ConfigNodeXmlParser.ValueNode}[@name='flowState']");
                if (flowNode != null) flowNode.InnerText = resourceInfo.FlowState.ToString(CultureInfo.InvariantCulture);
            }

            return document.ToIndentedString();
        }
    }
}
