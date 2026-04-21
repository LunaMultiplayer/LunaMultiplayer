using KSP.UI.Screens.Flight;
using LmpClient.Extensions;
using LmpClient.Systems.VesselPositionSys;
using System;
using Object = UnityEngine.Object;

namespace LmpClient.VesselUtilities
{
    public class VesselLoader
    {
        /// <summary>
        /// Loads/Reloads a vessel into game
        /// </summary>
        public static bool LoadVessel(ProtoVessel vesselProto, bool forceReload)
        {
            try
            {
                return vesselProto.Validate(true) && LoadVesselIntoGame(vesselProto, forceReload);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error loading vessel: {e}");
                return false;
            }
        }

        #region Private methods

        /// <summary>
        /// Loads the vessel proto into the current game
        /// </summary>
        private static bool LoadVesselIntoGame(ProtoVessel vesselProto, bool forceReload)
        {
            if (HighLogic.CurrentGame?.flightState == null)
                return false;

            var reloadingOwnVessel = FlightGlobals.ActiveVessel && vesselProto.vesselID == FlightGlobals.ActiveVessel.id;

            //In case the vessel exists, silently remove them from unity and recreate it again
            var existingVessel = FlightGlobals.FindVessel(vesselProto.vesselID);
            if (existingVessel != null)
            {
                if (!forceReload && existingVessel.Parts.Count == vesselProto.protoPartSnapshots.Count &&
                    existingVessel.GetCrewCount() == vesselProto.GetVesselCrew().Count)
                {
                    return true;
                }

                LunaLog.Log($"[LMP]: Reloading vessel {vesselProto.vesselID}");
                if (reloadingOwnVessel)
                    existingVessel.RemoveAllCrew();

                FlightGlobals.RemoveVessel(existingVessel);
                // Disable immediately so Unity stops calling FixedUpdate on this vessel before
                // Object.Destroy is processed — same deferred-destroy race that causes
                // Vessel.UpdateCaches() NullReferenceExceptions (see VesselRemoveSystem.KillVessel).
                existingVessel.gameObject.SetActive(false);
                foreach (var part in existingVessel.parts)
                {
                    Object.Destroy(part.gameObject);
                }
                Object.Destroy(existingVessel.gameObject);
            }
            else
            {
                LunaLog.Log($"[LMP]: Loading vessel {vesselProto.vesselID}");
            }

            // Vessel.Start() calls Vessel.RebuildCrewList() which iterates protoModuleCrew on every
            // ProtoPartSnapshot.  A null slot in that list causes a NullReferenceException that Unity
            // catches internally (logged as [EXC]) — it never reaches our catch block below.  Strip
            // nulls here so RebuildCrewList() always receives a clean list.
            foreach (var snapshot in vesselProto.protoPartSnapshots)
                snapshot.protoModuleCrew?.RemoveAll(c => c == null);

            try
            {
                vesselProto.Load(HighLogic.CurrentGame.flightState);
            }
            catch (Exception loadEx)
            {
                // KSP may have created the Vessel GameObject before the exception (e.g. OrbitSnapshot.Load
                // throws when the vessel's referenceBody index is out of range because the server has extra
                // celestial bodies from a mod the client doesn't have).  Without cleanup the zombie vessel
                // stays in FlightGlobals and causes NullReferenceExceptions in Vessel.UpdateCaches() on
                // every physics tick.
                LunaLog.LogError($"[LMP]: Vessel {vesselProto.vesselID} threw during ProtoVessel.Load — removing to prevent zombie vessel. Error: {loadEx.Message}");
                if (vesselProto.vesselRef != null)
                {
                    FlightGlobals.RemoveVessel(vesselProto.vesselRef);
                    foreach (var part in vesselProto.vesselRef.parts)
                        Object.Destroy(part.gameObject);
                    Object.Destroy(vesselProto.vesselRef.gameObject);
                }
                HighLogic.CurrentGame.flightState.protoVessels.Remove(vesselProto);
                return false;
            }

            if (vesselProto.vesselRef == null)
            {
                LunaLog.Log($"[LMP]: Protovessel {vesselProto.vesselID} failed to create a vessel!");
                return false;
            }

            // Verify that every part module loaded successfully.  When the server has a mod that the
            // client lacks, KSP may instantiate a part but leave null slots in Part.Modules — these
            // cause Vessel.UpdateCaches() to throw a NullReferenceException on every physics tick.
            if (vesselProto.vesselRef.parts != null)
            {
                string badDetail = null;
                for (var pi = 0; pi < vesselProto.vesselRef.parts.Count && badDetail == null; pi++)
                {
                    var p = vesselProto.vesselRef.parts[pi];
                    if (p == null) { badDetail = $"null part at index {pi}"; break; }
                    if (p.Modules == null) continue;
                    for (var mi = 0; mi < p.Modules.Count; mi++)
                    {
                        if (p.Modules[mi] == null)
                        {
                            badDetail = $"null module at index {mi} on part '{p.partName}'";
                            break;
                        }
                    }
                }

                if (badDetail != null)
                {
                    LunaLog.LogError($"[LMP]: Vessel {vesselProto.vesselID} ({vesselProto.vesselName}) loaded with {badDetail} — removing to prevent Vessel.UpdateCaches NullReferenceException spam.");
                    FlightGlobals.RemoveVessel(vesselProto.vesselRef);
                    vesselProto.vesselRef.gameObject.SetActive(false);
                    foreach (var p in vesselProto.vesselRef.parts)
                        if (p?.gameObject != null) Object.Destroy(p.gameObject);
                    Object.Destroy(vesselProto.vesselRef.gameObject);
                    HighLogic.CurrentGame.flightState.protoVessels.Remove(vesselProto);
                    return false;
                }
            }

            // Safety-net: verify the ProtoVessel can be saved before keeping it in the flight state.
            // If ProtoVessel.Save() throws (e.g. from a null resource definition left by a server mod),
            // GamePersistence.SaveGame() would also throw, causing the UI to freeze on any menu close.
            try
            {
                vesselProto.Save(new ConfigNode());
            }
            catch (Exception saveEx)
            {
                LunaLog.LogError($"[LMP]: Vessel {vesselProto.vesselID} ({vesselProto.vesselName}) cannot be saved — removing to prevent UI freezes. Error: {saveEx.Message}");
                FlightGlobals.RemoveVessel(vesselProto.vesselRef);
                foreach (var part in vesselProto.vesselRef.parts)
                    Object.Destroy(part.gameObject);
                Object.Destroy(vesselProto.vesselRef.gameObject);
                HighLogic.CurrentGame.flightState.protoVessels.Remove(vesselProto);
                return false;
            }

            VesselPositionSystem.Singleton.ForceUpdateVesselPosition(vesselProto.vesselRef.id);

            vesselProto.vesselRef.protoVessel = vesselProto;
            if (vesselProto.vesselRef.isEVA)
            {
                var evaModule = vesselProto.vesselRef.FindPartModuleImplementing<KerbalEVA>();
                if (evaModule != null && evaModule.fsm != null && !evaModule.fsm.Started)
                {
                    evaModule.fsm?.StartFSM("Idle (Grounded)");
                }
                vesselProto.vesselRef.GoOnRails();
            }

            if (vesselProto.vesselRef.situation > Vessel.Situations.PRELAUNCH)
            {
                vesselProto.vesselRef.orbitDriver.updateFromParameters();
            }

            if (double.IsNaN(vesselProto.vesselRef.orbitDriver.pos.x))
            {
                LunaLog.Log($"[LMP]: Protovessel {vesselProto.vesselID} has an invalid orbit");
                return false;
            }

            if (reloadingOwnVessel)
            {
                vesselProto.vesselRef.Load();
                vesselProto.vesselRef.RebuildCrewList();

                //Do not do the setting of the active vessel manually, too many systems are dependant of the events triggered by KSP
                FlightGlobals.ForceSetActiveVessel(vesselProto.vesselRef);

                vesselProto.vesselRef.SpawnCrew();
                foreach (var crew in vesselProto.vesselRef.GetVesselCrew())
                {
                    ProtoCrewMember._Spawn(crew);
                    if (crew.KerbalRef)
                        crew.KerbalRef.state = Kerbal.States.ALIVE;
                }

                if (KerbalPortraitGallery.Instance.ActiveCrewItems.Count != vesselProto.vesselRef.GetCrewCount())
                {
                    KerbalPortraitGallery.Instance.StartReset(FlightGlobals.ActiveVessel);
                }
            }

            return true;
        }

        #endregion
    }
}
