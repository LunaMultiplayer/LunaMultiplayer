using LunaCommon.Enums;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Xml;
using Server.Utilities;
using System;
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
        /// We received a part module change from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later receive an updated vesselproto
        /// </summary>
        public static void WritePartSyncUiFieldDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselPartSyncUiFieldMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            //Sync part changes ALWAYS and ignore the rate they arrive
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd(msgData.VesselId, new object()))
                {
                    if (!VesselStoreSystem.CurrentVesselsInXmlFormat.TryGetValue(msgData.VesselId, out var xmlData)) return;

                    var updatedText = UpdateProtoVesselFileWithNewPartSyncUiFieldData(xmlData, msgData);
                    VesselStoreSystem.CurrentVesselsInXmlFormat.TryUpdate(msgData.VesselId, updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Updates the proto vessel with the values we received about a part module change
        /// </summary>
        private static string UpdateProtoVesselFileWithNewPartSyncUiFieldData(string vesselData, VesselPartSyncUiFieldMsgData msgData)
        {
            var document = new XmlDocument();
            document.LoadXml(vesselData);

            var module = $@"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='PART']/{ConfigNodeXmlParser.ValueNode}[@name='uid' and text()=""{msgData.PartFlightId}""]/" +
                         $"following-sibling::{ConfigNodeXmlParser.ParentNode}[@name='MODULE']/{ConfigNodeXmlParser.ValueNode}" +
                         $@"[@name='name' and text()=""{msgData.ModuleName}""]/parent::{ConfigNodeXmlParser.ParentNode}[@name='MODULE']/";

            var xpath = $"{module}/{ConfigNodeXmlParser.ValueNode}[@name='{msgData.FieldName}']";

            var fieldNode = document.SelectSingleNode(xpath);
            if (fieldNode != null)
            {
                switch (msgData.FieldType)
                {
                    case PartSyncFieldType.Boolean:
                        fieldNode.InnerText = msgData.BoolValue.ToString(CultureInfo.InvariantCulture);
                        break;
                    case PartSyncFieldType.Integer:
                        fieldNode.InnerText = msgData.IntValue.ToString(CultureInfo.InvariantCulture);
                        break;
                    case PartSyncFieldType.Float:
                        fieldNode.InnerText = msgData.FloatValue.ToString(CultureInfo.InvariantCulture);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }

            return document.ToIndentedString();
        }
    }
}
