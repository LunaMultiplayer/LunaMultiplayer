using LunaCommon.Message.Data.Vessel;
using LunaCommon.Xml;
using Server.Utilities;
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
        /// We received a fairing change from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later receive an updated vesselproto
        /// </summary>
        public static void WritePersistentDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselPersistentMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            //Sync new persistent id's ALWAYS and ignore the rate they arrive
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd(msgData.VesselId, new object()))
                {
                    if (!VesselStoreSystem.CurrentVesselsInXmlFormat.TryGetValue(msgData.VesselId, out var xmlData)) return;

                    var updatedText = UpdateProtoVesselFileWithNewPersistentIdData(xmlData, msgData);
                    VesselStoreSystem.CurrentVesselsInXmlFormat.TryUpdate(msgData.VesselId, updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Updates the proto vessel with the values we received about a new persistent id of a vessel/part
        /// </summary>
        private static string UpdateProtoVesselFileWithNewPersistentIdData(string vesselData, VesselPersistentMsgData msgData)
        {
            var document = new XmlDocument();
            document.LoadXml(vesselData);

            if (msgData.PartPersistentChange)
            {
                var query = $@"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='PART']/{ConfigNodeXmlParser.ValueNode}[@name='persistentId' and text()=""{msgData.From}""]";
                var node = document.SelectSingleNode(query);
                if (node != null) node.InnerText = msgData.To.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                var node = document.SelectSingleNode($@"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='persistentId' and text()=""{msgData.From}""]");
                if (node != null) node.InnerText = msgData.To.ToString(CultureInfo.InvariantCulture);
            }

            return document.ToIndentedString();
        }
    }
}
