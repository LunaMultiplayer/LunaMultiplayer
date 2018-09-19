using LmpCommon.Message.Data.Vessel;
using LmpCommon.Xml;
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
        /// We received a action group information from a player
        /// Then we rewrite the vesselproto with that last information so players that connect later receive an updated vesselproto
        /// </summary>
        public static void WriteActionGroupDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselActionGroupMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            //Sync part changes ALWAYS and ignore the rate they arrive
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd(msgData.VesselId, new object()))
                {
                    if (!VesselStoreSystem.CurrentVesselsInXmlFormat.TryGetValue(msgData.VesselId, out var xmlData)) return;

                    var updatedText = UpdateProtoVesselWithNewUpdateData(xmlData, msgData);
                    VesselStoreSystem.CurrentVesselsInXmlFormat.TryUpdate(msgData.VesselId, updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Updates the proto vessel with the values we received of an action group value
        /// </summary>
        private static string UpdateProtoVesselWithNewUpdateData(string vesselData, VesselActionGroupMsgData msgData)
        {
            var document = new XmlDocument();
            document.LoadXml(vesselData);

            var node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='ACTIONGROUPS']/{ConfigNodeXmlParser.ValueNode}[@name='{msgData.ActionGroupString}']");
            if (node != null) node.InnerText = $"{msgData.Value.ToString(CultureInfo.InvariantCulture)}, 0";

            return document.ToIndentedString();
        }
    }
}
