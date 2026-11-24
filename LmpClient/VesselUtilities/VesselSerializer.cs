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
                var vesselNode = data.DeserializeToConfigNode(numBytes);
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
            return PreSerializationChecks(protoVessel, out var configNode) ? configNode.Serialize() : new byte[0];
        }

        /// <summary>
        /// Serializes a vessel to a previous preallocated array (avoids garbage generation)
        /// </summary>
        public static void SerializeVesselToArray(ProtoVessel protoVessel, byte[] data, out int numBytes)
        {
            if (PreSerializationChecks(protoVessel, out var configNode))
            {
                configNode.SerializeToArray(data, out numBytes);
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

                //Pre-construction sanitisation pass: catches vessels whose wire-side ConfigNode
                //already contains "Infinity"/"NaN" inside DISCOVERY (asteroids/comets that
                //round-tripped through the bug on a peer). The sister post-construction pass in
                //VesselLoader catches the case this one cannot see -- vessels with no DISCOVERY
                //sub-node at all, where stock KSP's ProtoVessel constructor synthesises the
                //offending values itself.
                DiscoveryInfoSanitizer.SanitizeVesselNode(inputNode, protoVesselId, "wire-input");

                //Evict stale owners BEFORE the stock constructor deserialises the snapshots:
                //it renames colliding persistentIds, and the VesselLoader pass is too late here.
                var wireIdentities = StalePersistentIdEvictor.PreflightWireIdentityAndEvict(inputNode, protoVesselId);

                //Cannot reuse the Protovessel to save memory garbage as it does not have any clear method :(
                var constructed = new ProtoVessel(inputNode, HighLogic.CurrentGame);

                StalePersistentIdEvictor.LogWireConstructionResult(constructed, wireIdentities, protoVesselId);
                return constructed;
            }
            catch (Exception e)
            {
                //Constructor threw after the preflight evicted entries (possibly after a partial
                //constructor registered some) — roll back so the previous copy stays registered.
                StalePersistentIdEvictor.RollbackEvictions(protoVesselId);
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

            return true;
        }

        #endregion
    }
}
