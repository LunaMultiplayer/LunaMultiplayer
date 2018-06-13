using KSP.UI;
using KSP.UI.Screens;
using LunaClient.Base;
using LunaClient.Events;
using System.Collections.Concurrent;
using System.Reflection;
using Object = UnityEngine.Object;

namespace LunaClient.Systems.KerbalSys
{
    /// <summary>
    /// System that handles the kerbals between client and the server.
    /// </summary>
    public class KerbalSystem : MessageSystem<KerbalSystem, KerbalMessageSender, KerbalMessageHandler>
    {
        #region Fields

        public ConcurrentQueue<string> KerbalsToRemove { get; private set; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<ConfigNode> KerbalsToProcess { get; private set; } = new ConcurrentQueue<ConfigNode>();

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
        private static readonly MethodInfo InitiateGui = typeof(AstronautComplex).GetMethod("InitiateGUI",BindingFlags.NonPublic | BindingFlags.Instance);

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
            GameEvents.onGameSceneLoadRequested.Add(KerbalEvents.SwitchSceneRequested);
            GameEvents.onKerbalStatusChange.Add(KerbalEvents.StatusChange);
            GameEvents.onKerbalTypeChange.Add(KerbalEvents.TypeChange);
            RevertEvent.onReturningToEditor.Add(KerbalEvents.ReturningToEditor);
            RevertEvent.onRevertingToPrelaunch.Add(KerbalEvents.ReturningToEditor);
            GameEvents.onVesselTerminated.Add(KerbalEvents.OnVesselTerminated);
            GameEvents.onVesselRecovered.Add(KerbalEvents.OnVesselRecovered);
            GameEvents.onVesselWillDestroy.Add(KerbalEvents.OnVesselWillDestroy);

            VesselLoadEvent.onVesselLoaded.Add(KerbalEvents.OnVesselLoaded);
            VesselReloadEvent.onVesselReloaded.Add(KerbalEvents.OnVesselReloaded);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            KerbalsToRemove = new ConcurrentQueue<string>();
            KerbalsToProcess = new ConcurrentQueue<ConfigNode>();
            GameEvents.onGameSceneLoadRequested.Remove(KerbalEvents.SwitchSceneRequested);
            GameEvents.onKerbalStatusChange.Remove(KerbalEvents.StatusChange);
            GameEvents.onKerbalTypeChange.Remove(KerbalEvents.TypeChange);
            RevertEvent.onReturningToEditor.Remove(KerbalEvents.ReturningToEditor);
            RevertEvent.onRevertingToPrelaunch.Remove(KerbalEvents.ReturningToEditor);
            GameEvents.onVesselTerminated.Remove(KerbalEvents.OnVesselTerminated);
            GameEvents.onVesselRecovered.Remove(KerbalEvents.OnVesselRecovered);
            GameEvents.onVesselWillDestroy.Remove(KerbalEvents.OnVesselWillDestroy);

            VesselLoadEvent.onVesselLoaded.Remove(KerbalEvents.OnVesselLoaded);
            VesselReloadEvent.onVesselReloaded.Remove(KerbalEvents.OnVesselReloaded);
        }

        #endregion

        #region Public

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
                    HighLogic.CurrentGame.CrewRoster.Remove(kerbalNameToRemove);
                    refreshDialog = true;
                }

                if (refreshDialog) RefreshCrewDialog();
            }
        }

        /// <summary>
        /// Loads the unloaded (either because they are new or they are updated) kerbals into the game.
        /// We load them only when we are actually ready to play
        /// </summary>
        private void LoadKerbals()
        {
            if (KerbalSystemReady && HighLogic.LoadedScene >= GameScenes.SPACECENTER)
            {
                ProcessKerbalQueue();
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
                CrewAssignmentDialog.Instance.RefreshCrewLists(CrewAssignmentDialog.Instance.GetManifest(true), false, true, null);
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
            
            if (!HighLogic.CurrentGame.CrewRoster.Exists(protoCrew.name))
            {
                HighLogic.CurrentGame.CrewRoster.AddCrewMember(protoCrew);
            }
            else
            {
                UpdateKerbalData(crewNode, protoCrew);
            }
        }

        /// <summary>
        /// Updates an existing Kerbal
        /// </summary>
        private void UpdateKerbalData(ConfigNode crewNode, ProtoCrewMember protoCrew)
        {
            var careerLogNode = crewNode.GetNode("CAREER_LOG");
            if (careerLogNode != null)
            {
                //Insert wolf howling at the moon here
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].careerLog.Entries.Clear();
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].careerLog.Load(careerLogNode);
            }
            else
            {
                LunaLog.Log($"[LMP]: Career log node for {protoCrew.name} is empty!");
            }

            var flightLogNode = crewNode.GetNode("FLIGHT_LOG");
            if (flightLogNode != null)
            {
                //And here. Someone "cannot into" lists and how to protect them.
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].flightLog.Entries.Clear();
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].flightLog.Load(flightLogNode);
            }
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].courage = protoCrew.courage;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].experience = protoCrew.experience;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].experienceLevel = protoCrew.experienceLevel;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].experienceTrait = protoCrew.experienceTrait;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].gExperienced = protoCrew.gExperienced;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].gIncrement = protoCrew.gIncrement;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].geeForce = protoCrew.geeForce;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].gender = protoCrew.gender;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].hasToured = protoCrew.hasToured;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].isBadass = protoCrew.isBadass;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].inactiveTimeEnd = protoCrew.inactiveTimeEnd;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].outDueToG = protoCrew.outDueToG;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].seat = protoCrew.seat;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].seatIdx = protoCrew.seatIdx;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].stupidity = protoCrew.stupidity;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].trait = protoCrew.trait;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].UTaR = protoCrew.UTaR;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].veteran = protoCrew.veteran;

            SetKerbalTypeWithoutTriggeringEvent(HighLogic.CurrentGame.CrewRoster[protoCrew.name], protoCrew.type);
            SetKerbalStatusWithoutTriggeringEvent(HighLogic.CurrentGame.CrewRoster[protoCrew.name], protoCrew.rosterStatus);
        }

        #endregion
    }
}
