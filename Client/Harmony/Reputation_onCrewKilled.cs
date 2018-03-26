using System.Collections.Generic;
using System.Linq;
using Harmony;

// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to stop the reputation loss on kerbal dead.
    /// Maybe in the future there could be a workaround for appliyng the reputation loss
    /// in case the player has caused it himself and NOT by vessels that are crashed by other players
    /// (through time or synchronization bugs).
    /// </summary>
    [HarmonyPatch(typeof(Reputation))]
    [HarmonyPatch("OnCrewKilled")]
    class Reputation_OnCrewKilled
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TranspilerOnCrewKilled(IEnumerable<CodeInstruction> instructions)
        {
            /*
            if (evt.eventType == FlightEvents.CREW_KILLED)
            {
                this.AddReputation(GameVariables.Instance.reputationKerbalDeath * HighLogic.CurrentGame.Parameters.Career.RepLossMultiplier, TransactionReasons.VesselLoss);
            }
            */
            var codes = new List<CodeInstruction>(instructions);
            codes.Clear();
            return codes.AsEnumerable();
        }
    }
}
