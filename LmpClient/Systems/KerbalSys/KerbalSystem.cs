using KSP.UI;
using KSP.UI.Screens;
using LmpClient.Base;
using LmpClient.Events;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LmpClient.Systems.KerbalSys
{
    /// <summary>
    /// System that handles the kerbals between client and the server.
    /// </summary>
    public class KerbalSystem : MessageSystem<KerbalSystem, KerbalMessageSender, KerbalMessageHandler>
    {
        #region Fields

        public ConcurrentQueue<string> KerbalsToRemove { get; private set; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<ConfigNode> KerbalsToProcess { get; private set; } = new ConcurrentQueue<ConfigNode>();

        private bool _pendingGameUpdate;

        public bool KerbalSystemReady => Enabled && HighLogic.CurrentGame?.CrewRoster != null;

        public KerbalEvents KerbalEvents { get; } = new KerbalEvents();

        private static AstronautComplex _astronautComplex;
        public AstronautComplex AstronautComplex
        {
            get
            {
                if (_astronautComplex == null)
                {
                    _astronautComplex = Object.FindObjectOfType<AstronautComplex>();
                }
                return _astronautComplex;
            }
        }

        #region Reflection fields

        private static readonly FieldInfo KerbalStatusField = typeof(ProtoCrewMember).GetField("_rosterStatus", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo KerbalTypeField = typeof(ProtoCrewMember).GetField("_type", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo CreateAvailableList = typeof(AstronautComplex).GetMethod("CreateAvailableList", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo CreateAssignedList = typeof(AstronautComplex).GetMethod("CreateAssignedList", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo CreateKiaList = typeof(AstronautComplex).GetMethod("CreateKiaList", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo InitiateGui = typeof(AstronautComplex).GetMethod("InitiateGUI", BindingFlags.NonPublic | BindingFlags.Instance);

        #endregion

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(KerbalSystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, RemoveQueuedKerbals));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, LoadKerbals));

            VesselAssemblyEvent.onVesselValidationBeforAssembly.Add(KerbalEvents.ValidationBeforeAssembly);
            GameEvents.onKerbalLevelUp.Add(KerbalEvents.KerbalLevelUp);
            GameEvents.onKerbalStatusChange.Add(KerbalEvents.StatusChange);
            GameEvents.onKerbalTypeChange.Add(KerbalEvents.TypeChange);
            RevertEvent.onReturningToEditor.Add(KerbalEvents.ReturningToEditor);
            RemoveEvent.onLmpTerminatedVessel.Add(KerbalEvents.OnVesselTerminated);
            RemoveEvent.onLmpRecoveredVessel.Add(KerbalEvents.OnVesselRecovered);
            RemoveEvent.onLmpDestroyVessel.Add(KerbalEvents.OnVesselWillDestroy);

            VesselLoadEvent.onLmpVesselLoaded.Add(KerbalEvents.OnVesselLoaded);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            KerbalsToRemove = new ConcurrentQueue<string>();
            KerbalsToProcess = new ConcurrentQueue<ConfigNode>();
            _pendingGameUpdate = false;
            VesselAssemblyEvent.onVesselValidationBeforAssembly.Remove(KerbalEvents.ValidationBeforeAssembly);
            GameEvents.onKerbalStatusChange.Remove(KerbalEvents.StatusChange);
            GameEvents.onKerbalTypeChange.Remove(KerbalEvents.TypeChange);
            RevertEvent.onReturningToEditor.Remove(KerbalEvents.ReturningToEditor);
            RemoveEvent.onLmpTerminatedVessel.Remove(KerbalEvents.OnVesselTerminated);
            RemoveEvent.onLmpRecoveredVessel.Remove(KerbalEvents.OnVesselRecovered);
            RemoveEvent.onLmpDestroyVessel.Remove(KerbalEvents.OnVesselWillDestroy);

            VesselLoadEvent.onLmpVesselLoaded.Remove(KerbalEvents.OnVesselLoaded);
        }

        #endregion

        #region Public

        /// <summary>
        /// Schedules a single <see cref="Game.Updated"/> call to be made on the next routine tick.
        /// Callers should use this instead of calling Updated() directly so that rapid successive
        /// requests (e.g. bulk vessel loads) are coalesced into one call.
        /// </summary>
        public void RequestGameUpdate()
        {
            _pendingGameUpdate = true;
        }

        /// <summary>
        /// Load all the received kerbals from the server into the game
        /// This should be called before the game starts as otherwise loading vessels with crew will fail
        /// </summary>
        public void LoadKerbalsIntoGame()
        {
            ProcessKerbalQueue();
        }

        /// <summary>
        /// Sets the kerbal status without triggering the event (usefull when receiveing kerbals from other clients)
        /// </summary>
        public void SetKerbalStatusWithoutTriggeringEvent(ProtoCrewMember crew, ProtoCrewMember.RosterStatus newStatus)
        {
            if (crew == null) return;

            KerbalStatusField?.SetValue(crew, newStatus);
        }

        /// <summary>
        /// Sets the kerbal type without triggering the event (usefull when receiveing kerbals from other clients)
        /// </summary>
        public void SetKerbalTypeWithoutTriggeringEvent(ProtoCrewMember crew, ProtoCrewMember.KerbalType newType)
        {
            if (crew == null) return;

            KerbalTypeField?.SetValue(crew, newType);
        }

        #endregion

        #region Routines

        /// <summary>
        /// Removes the kerbals that we received
        /// </summary>
        private void RemoveQueuedKerbals()
        {
            if (KerbalSystemReady)
            {
                var refreshDialog = false;
                while (KerbalsToRemove.TryDequeue(out var kerbalNameToRemove))
                {
                    var kerbalToRemove = HighLogic.CurrentGame.CrewRoster.Crew.FirstOrDefault(k => k.name == kerbalNameToRemove);
                    if (kerbalToRemove != null)
                    {
                        HighLogic.CurrentGame.CrewRoster.Remove(kerbalToRemove);
                    }
                    refreshDialog = true;
                }

                if (refreshDialog) RefreshCrewDialog();
            }
        }

        /// <summary>
        /// Loads the unloaded (either because they are new or they are updated) kerbals into the game.
        /// We load them only when we are actually ready to play.
        /// Also flushes any pending <see cref="Game.Updated"/> request that was deferred to avoid
        /// calling it once per vessel during bulk loads.
        /// </summary>
        private void LoadKerbals()
        {
            if (KerbalSystemReady && HighLogic.LoadedScene >= GameScenes.SPACECENTER)
            {
                ProcessKerbalQueue();

                if (_pendingGameUpdate)
                {
                    _pendingGameUpdate = false;
                    HighLogic.CurrentGame.Updated();
                }
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Runs trough the concurrentKerbalQueue and process them
        /// </summary>
        private void ProcessKerbalQueue()
        {
            var refreshDialog = false;
            while (KerbalsToProcess.TryDequeue(out var kerbalNode))
            {
                LoadKerbal(kerbalNode);
                refreshDialog = true;
            }

            if (refreshDialog) RefreshCrewDialog();
        }

        /// <summary>
        /// Call this method to refresh the crews in the vessel spawn, vessel editor and astronaut complex
        /// </summary>
        public void RefreshCrewDialog()
        {
            if (CrewAssignmentDialog.Instance != null)
            {
                CrewAssignmentDialog.Instance.RefreshCrewLists(CrewAssignmentDialog.Instance.GetManifest(), false, true);
                CrewAssignmentDialog.Instance.ButtonClear();
                CrewAssignmentDialog.Instance.ButtonFill();
            }

            if (AstronautComplex != null)
            {
                InitiateGui.Invoke(AstronautComplex, null);
                CreateAvailableList.Invoke(AstronautComplex, null);
                CreateAssignedList.Invoke(AstronautComplex, null);
                CreateKiaList.Invoke(AstronautComplex, null);
            }
        }

        /// <summary>
        /// Creates or updates a kerbal
        /// </summary>
        private void LoadKerbal(ConfigNode crewNode)
        {
            var protoCrew = new ProtoCrewMember(HighLogic.CurrentGame.Mode, crewNode);
            if (string.IsNullOrEmpty(protoCrew.name))
            {
                LunaLog.LogError("[LMP]: protoName is blank!");
                return;
            }

            var existingKerbal = HighLogic.CurrentGame.CrewRoster.Crew.FirstOrDefault(k => k.name == protoCrew.name);

            if (existingKerbal == null)
            {
                HighLogic.CurrentGame.CrewRoster.AddCrewMember(protoCrew);
            }
            else
            {
                UpdateKerbalData(crewNode, existingKerbal);
            }
        }

        /// <summary>
        /// Updates an existing Kerbal
        /// </summary>
        private void UpdateKerbalData(ConfigNode crewNode, ProtoCrewMember existingProtoCrew)
        {
            var newProtoCrew = new ProtoCrewMember(HighLogic.CurrentGame.Mode, crewNode);

            var careerLogNode = crewNode.GetNode("CAREER_LOG");
            if (careerLogNode != null)
            {
                //Insert wolf howling at the moon here
                existingProtoCrew.careerLog.Entries.Clear();
                existingProtoCrew.careerLog.Load(careerLogNode);
            }
            else
            {
                LunaLog.Log($"[LMP]: Career log node for {existingProtoCrew.name} is empty!");
            }

            var flightLogNode = crewNode.GetNode("FLIGHT_LOG");
            if (flightLogNode != null)
            {
                //And here. Someone "cannot into" lists and how to protect them.
                existingProtoCrew.flightLog.Entries.Clear();
                existingProtoCrew.flightLog.Load(flightLogNode);
            }
            existingProtoCrew.courage = newProtoCrew.courage;
            existingProtoCrew.experience = newProtoCrew.experience;
            existingProtoCrew.experienceLevel = newProtoCrew.experienceLevel;
            existingProtoCrew.experienceTrait = newProtoCrew.experienceTrait;
            existingProtoCrew.gExperienced = newProtoCrew.gExperienced;
            existingProtoCrew.gIncrement = newProtoCrew.gIncrement;
            existingProtoCrew.geeForce = newProtoCrew.geeForce;
            existingProtoCrew.gender = newProtoCrew.gender;
            existingProtoCrew.hasToured = newProtoCrew.hasToured;
            existingProtoCrew.isBadass = newProtoCrew.isBadass;
            existingProtoCrew.inactiveTimeEnd = newProtoCrew.inactiveTimeEnd;
            existingProtoCrew.outDueToG = newProtoCrew.outDueToG;
            existingProtoCrew.seat = newProtoCrew.seat;
            existingProtoCrew.seatIdx = newProtoCrew.seatIdx;
            existingProtoCrew.stupidity = newProtoCrew.stupidity;
            existingProtoCrew.trait = newProtoCrew.trait;
            existingProtoCrew.UTaR = newProtoCrew.UTaR;
            existingProtoCrew.veteran = newProtoCrew.veteran;

            SetKerbalTypeWithoutTriggeringEvent(existingProtoCrew, newProtoCrew.type);
            SetKerbalStatusWithoutTriggeringEvent(existingProtoCrew, newProtoCrew.rosterStatus);
        }

        #endregion
    }
}
