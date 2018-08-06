using Harmony;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to fix the cheat button of "Rendezvous" in the debug dialog
    /// </summary>
    [HarmonyPatch(typeof(FlightGlobals))]
    [HarmonyPatch("SetShipOrbitRendezvous")]
    public class FlightGlobals_SetShipOrbitRendezvous
    {
        [HarmonyPrefix]
        public static bool PrefixSetShipOrbitRendezvous(Vessel target, Vector3d relPosition, Vector3d relVelocity)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;
            if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(target.id, SettingsSystem.CurrentSettings.PlayerName)) return true;

            var body = FlightGlobals.ActiveVessel.mainBody;
            FlightGlobals.currentMainBody = target.mainBody;

            Traverse.Create(FlightGlobals.fetch).Method("PrepForOrbitSet").GetValue();

            var currentOrbit = target.GetCurrentOrbit();

            //var universalTime = Planetarium.GetUniversalTime();
            var universalTime = currentOrbit.epoch;

            var vector3d = currentOrbit.vel.normalized;
            var orbitNormal = currentOrbit.GetOrbitNormal();
            var vector3d1 = Vector3d.Cross(vector3d, orbitNormal);
            var vector3d2 = currentOrbit.pos + relPosition.Basis(vector3d1, vector3d, orbitNormal);
            var vector3d3 = currentOrbit.vel + relVelocity.Basis(vector3d1, vector3d, orbitNormal);

            FlightGlobals.ActiveVessel.orbit.UpdateFromStateVectors(vector3d2, vector3d3, currentOrbit.referenceBody, universalTime);

            Traverse.Create(FlightGlobals.fetch).Method("PostOrbitSet").GetValue();

            return false;
        }
    }
}
