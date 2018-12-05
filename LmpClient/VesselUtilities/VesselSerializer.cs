using LmpClient.Extensions;
using LmpClient.Utilities;
using System;

namespace LmpClient.VesselUtilities
{
    public class VesselSerializer
    {
        /// <summary>
        /// Deserialize a byte array into a protovessel
        /// </summary>
        public static ProtoVessel DeserializeVessel(byte[] data, int numBytes)
        {
            try
            {
                var vesselNode = ConfigNodeSerializer.Deserialize(data, numBytes);
                var configGuid = vesselNode?.GetValue("pid");

                return CreateSafeProtoVesselFromConfigNode(vesselNode, new Guid(configGuid));
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while deserializing vessel: {e}");
                return null;
            }
        }

        /// <summary>
        /// Serialize a protovessel into a byte array
        /// </summary>
        public static byte[] SerializeVessel(ProtoVessel protoVessel)
        {
            return PreSerializationChecks(protoVessel, out var configNode) ? ConfigNodeSerializer.Serialize(configNode) : new byte[0];
        }

        /// <summary>
        /// Serializes a vessel to a previous preallocated array (avoids garbage generation)
        /// </summary>
        public static void SerializeVesselToArray(ProtoVessel protoVessel, byte[] data, out int numBytes)
        {
            if (PreSerializationChecks(protoVessel, out var configNode))
            {
                ConfigNodeSerializer.SerializeToArray(configNode, data, out numBytes);
            }
            else
            {
                numBytes = 0;
            }
        }

        /// <summary>
        /// Creates a protovessel from a ConfigNode
        /// </summary>
        public static ProtoVessel CreateSafeProtoVesselFromConfigNode(ConfigNode inputNode, Guid protoVesselId)
        {
            try
            {
                //Cannot create a protovessel if HighLogic.CurrentGame is null as we don't have a CrewRoster
                //and the protopartsnapshot constructor needs it
                if (HighLogic.CurrentGame == null)
                    return null;

                //Cannot reuse the Protovessel to save memory garbage as it does not have any clear method :(
                return new ProtoVessel(inputNode, HighLogic.CurrentGame);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Damaged vessel {protoVesselId}, exception: {e}");
                return null;
            }
        }

        #region Private methods

        private static bool PreSerializationChecks(ProtoVessel protoVessel, out ConfigNode configNode)
        {
            configNode = new ConfigNode();

            if (protoVessel == null)
            {
                LunaLog.LogError("[LMP]: Cannot serialize a null protovessel");
                return false;
            }

            try
            {
                protoVessel.Save(configNode);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while saving vessel: {e}");
                return false;
            }

            var vesselId = new Guid(configNode.GetValue("pid"));

            //Defend against NaN orbits
            if (configNode.VesselHasNaNPosition())
            {
                LunaLog.LogError($"[LMP]: Vessel {vesselId} has NaN position");
                return false;
            }

            //Do not send the maneuver nodes
            RemoveManeuverNodesFromProtoVessel(configNode);
            return true;
        }


        #region Config node fixing

        /// <summary>
        /// Removes maneuver nodes from the vessel
        /// </summary>
        private static void RemoveManeuverNodesFromProtoVessel(ConfigNode vesselNode)
        {
            var flightPlanNode = vesselNode?.GetNode("FLIGHTPLAN");
            flightPlanNode?.ClearData();
        }

        #endregion

        #endregion
    }
}
