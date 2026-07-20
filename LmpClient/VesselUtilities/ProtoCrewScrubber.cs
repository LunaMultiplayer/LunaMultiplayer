namespace LmpClient.VesselUtilities
{
    /// <summary>
    /// Shared, side-effect-light removal of <c>null</c> entries from a protovessel's
    /// <see cref="ProtoPartSnapshot.protoModuleCrew"/> lists (and their parallel
    /// <c>protoCrewNames</c> slots, in lockstep).
    ///
    /// Stock KSP's <c>ProtoPartSnapshot.LoadCrew</c> inserts a <c>null</c> placeholder whenever a
    /// wire-side <c>crew = NAME</c> value can't be resolved through the local
    /// <c>CrewRoster</c> (empty name, or a kerbal/tourist the server hasn't replicated to us yet).
    /// Those nulls NRE several stock code paths — including <c>ProtoVessel.Save</c>, which aborts
    /// the entire game save. This helper is the single place that removes them so both the load
    /// path (<see cref="VesselLoader"/>) and the save path (ProtoVessel.Save Harmony patch) stay in
    /// sync. See <see cref="VesselLoader"/> for the full rationale on why the removal must be done
    /// in lockstep with <c>protoCrewNames</c>.
    /// </summary>
    public static class ProtoCrewScrubber
    {
        public struct ScrubResult
        {
            public int Removed;
            public int PartsAffected;
            public int NameDesyncs;
        }

        /// <summary>
        /// Removes every <c>null</c> <c>protoModuleCrew</c> entry (and the parallel
        /// <c>protoCrewNames</c> slot) from the vessel. Does not log and does not throw on a null
        /// vessel; callers own logging so they can phrase it for their context.
        /// </summary>
        public static ScrubResult ScrubNullCrew(ProtoVessel vesselProto)
        {
            var result = new ScrubResult();
            if (vesselProto?.protoPartSnapshots == null) return result;

            for (var p = 0; p < vesselProto.protoPartSnapshots.Count; p++)
            {
                var snapshot = vesselProto.protoPartSnapshots[p];
                var crew = snapshot?.protoModuleCrew;
                if (crew == null || crew.Count == 0) continue;

                var names = snapshot.protoCrewNames;
                var removedHere = 0;

                //Walk back-to-front so RemoveAt does not invalidate indices we haven't visited yet.
                for (var i = crew.Count - 1; i >= 0; i--)
                {
                    if (crew[i] != null) continue;

                    crew.RemoveAt(i);
                    if (names != null)
                    {
                        if (i < names.Count) names.RemoveAt(i);
                        else result.NameDesyncs++;
                    }
                    removedHere++;
                }

                if (removedHere > 0)
                {
                    result.Removed += removedHere;
                    result.PartsAffected++;
                }
            }

            return result;
        }
    }
}
