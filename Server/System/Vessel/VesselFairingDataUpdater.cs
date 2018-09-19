using LmpCommon.Message.Data.Vessel;
using LmpCommon.Xml;
using Server.Utilities;
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
        /// We received a fairing change from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later receive an updated vesselproto
        /// </summary>
        public static void WriteFairingDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselFairingMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            //Sync fairings ALWAYS and ignore the rate they arrive
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd(msgData.VesselId, new object()))
                {
                    if (!VesselStoreSystem.CurrentVesselsInXmlFormat.TryGetValue(msgData.VesselId, out var xmlData)) return;

                    var updatedText = UpdateProtoVesselFileWithNewFairingData(xmlData, msgData);
                    VesselStoreSystem.CurrentVesselsInXmlFormat.TryUpdate(msgData.VesselId, updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Updates the proto vessel with the values we received about a fairing change of a vessel
        /// </summary>
        private static string UpdateProtoVesselFileWithNewFairingData(string vesselData, VesselFairingMsgData msgData)
        {
            var document = new XmlDocument();
            document.LoadXml(vesselData);

            var module = $@"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='PART']/{ConfigNodeXmlParser.ValueNode}[@name='uid' and text()=""{msgData.PartFlightId}""]/" +
                         $"following-sibling::{ConfigNodeXmlParser.ParentNode}[@name='MODULE']/{ConfigNodeXmlParser.ValueNode}" +
                         @"[@name='name' and text()=""ModuleProceduralFairing""]/parent::{ConfigNodeXmlParser.ParentNode}[@name='MODULE']/";

            var xpath = $"{module}/{ConfigNodeXmlParser.ValueNode}[@name='fsm']";

            var fieldNode = document.SelectSingleNode(xpath);
            if (fieldNode != null) fieldNode.InnerText = "st_flight_deployed";

            var moduleNode = fieldNode.ParentNode;
            var fairingsSections = document.SelectNodes($"{module}/{ConfigNodeXmlParser.ParentNode}[@name='XSECTION']");
            if (moduleNode != null && fairingsSections != null)
            {
                for (var i = 0; i < fairingsSections.Count; i++)
                {
                    moduleNode.RemoveChild(fairingsSections[i]);
                }
            }

            return document.ToIndentedString();
        }
    }
}
