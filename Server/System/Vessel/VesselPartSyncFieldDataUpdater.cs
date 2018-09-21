using LmpCommon.Enums;
using LmpCommon.Message.Data.Vessel;
using System;
using System.Globalization;
using System.Threading.Tasks;

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
        public static void WritePartSyncFieldDataToFile(VesselBaseMsgData message)
        {
            if (!(message is VesselPartSyncFieldMsgData msgData)) return;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            //Sync part changes ALWAYS and ignore the rate they arrive
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd(msgData.VesselId, new object()))
                {
                    if (!VesselStoreSystem.CurrentVessels.TryGetValue(msgData.VesselId, out var vessel)) return;

                    UpdateProtoVesselFileWithNewPartSyncFieldData(vessel, msgData);
                }
            });
        }

        /// <summary>
        /// Updates the proto vessel with the values we received about a part module change
        /// </summary>
        private static void UpdateProtoVesselFileWithNewPartSyncFieldData(Structures.Vessel vessel, VesselPartSyncFieldMsgData msgData)
        {
            var part = vessel.GetPart(msgData.PartFlightId);
            if (part != null)
            {
                var module = part.GetSingleModule(msgData.ModuleName);
                if (module != null)
                {
                    switch (msgData.FieldType)
                    {
                        case PartSyncFieldType.Boolean:
                            module.UpdateValue(msgData.FieldName, msgData.BoolValue.ToString(CultureInfo.InvariantCulture));
                            break;
                        case PartSyncFieldType.Integer:
                            module.UpdateValue(msgData.FieldName, msgData.IntValue.ToString(CultureInfo.InvariantCulture));
                            break;
                        case PartSyncFieldType.Float:
                            module.UpdateValue(msgData.FieldName, msgData.FloatValue.ToString(CultureInfo.InvariantCulture));
                            break;
                        case PartSyncFieldType.Double:
                            module.UpdateValue(msgData.FieldName, msgData.DoubleValue.ToString(CultureInfo.InvariantCulture));
                            break;
                        case PartSyncFieldType.Vector3:
                            module.UpdateValue(msgData.FieldName, $"{msgData.VectorValue[0].ToString(CultureInfo.InvariantCulture)}," +
                                                  $"{msgData.VectorValue[1].ToString(CultureInfo.InvariantCulture)}," +
                                                  $"{msgData.VectorValue[2].ToString(CultureInfo.InvariantCulture)}");
                            break;
                        case PartSyncFieldType.Quaternion:
                            module.UpdateValue(msgData.FieldName, $"{msgData.QuaternionValue[0].ToString(CultureInfo.InvariantCulture)}," +
                                             $"{msgData.QuaternionValue[1].ToString(CultureInfo.InvariantCulture)}," +
                                             $"{msgData.QuaternionValue[2].ToString(CultureInfo.InvariantCulture)}," +
                                             $"{msgData.QuaternionValue[3].ToString(CultureInfo.InvariantCulture)}");
                            break;
                        case PartSyncFieldType.Object:
                        case PartSyncFieldType.String:
                        case PartSyncFieldType.Enum:
                            module.UpdateValue(msgData.FieldName, msgData.StrValue);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}
