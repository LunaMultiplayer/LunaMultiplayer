using KSP.UI;
using KSP.UI.Screens;
using LunaClient.Base;
using System.Collections.Concurrent;
using System.Reflection;
using UnityEngine;

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

        /// <summary>
        /// Invoke this private method to rebuild the crew lists that appear on the astronaut complex
        /// </summary>
        private static MethodInfo RebuildCrewLists { get; } = typeof(AstronautComplex).GetMethod("InitiateGUI", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        public KerbalEvents KerbalEvents { get; } = new KerbalEvents();

        private static AstronautComplex _astronautComplex;
        private static AstronautComplex AstronautComplex => _astronautComplex ?? (_astronautComplex = Object.FindObjectOfType<AstronautComplex>());

        #endregion

        #region Base overrides

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, RemoveQueuedKerbals));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, LoadKerbals));
            GameEvents.onCrewKilled.Add(KerbalEvents.OnCrewKilled);
            GameEvents.OnCrewmemberHired.Add(KerbalEvents.CrewAdd);
            GameEvents.OnCrewmemberLeftForDead.Add(KerbalEvents.CrewSetAsDead);
            GameEvents.OnCrewmemberSacked.Add(KerbalEvents.CrewRemove);
            GameEvents.onFlightReady.Add(KerbalEvents.FlightReady);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            KerbalsToRemove = new ConcurrentQueue<string>();
            KerbalsToProcess = new ConcurrentQueue<ConfigNode>();
            GameEvents.onCrewKilled.Remove(KerbalEvents.OnCrewKilled);
            GameEvents.OnCrewmemberHired.Remove(KerbalEvents.CrewAdd);
            GameEvents.OnCrewmemberLeftForDead.Remove(KerbalEvents.CrewSetAsDead);
            GameEvents.OnCrewmemberSacked.Remove(KerbalEvents.CrewRemove);
            GameEvents.onFlightReady.Remove(KerbalEvents.FlightReady);
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
        /// Checks if the crew on a protoVessel has changed and sends the message accordingly
        /// </summary>
        public void ProcessKerbalsInVessel(ProtoVessel protoVessel)
        {
            if (protoVessel == null) return;

            foreach (var protoCrew in protoVessel.GetVesselCrew())
            {
                MessageSender.SendKerbal(protoCrew);
            }
        }

        /// <summary>
        /// Checks if the crew on a vessel has changed and sends the message accordingly
        /// </summary>
        public void ProcessKerbalsInVessel(Vessel vessel)
        {
            if (vessel == null) return;

            foreach (var protoCrew in vessel.GetVesselCrew())
            {
                MessageSender.SendKerbal(protoCrew);
            }
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
                    refreshDialog |= HighLogic.CurrentGame.CrewRoster.Remove(kerbalNameToRemove);
                }

                if (refreshDialog)
                    RefreshCrewDialog();
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
            var refreshDialog = KerbalsToProcess.Count > 0;
            while (KerbalsToProcess.TryDequeue(out var kerbalNode))
            {
                LoadKerbal(kerbalNode);
            }

            if (refreshDialog)
                RefreshCrewDialog();
        }

        /// <summary>
        /// Call this method to refresh the crews in the vessel spawn, vessel editor and astronaut complex
        /// </summary>
        private static void RefreshCrewDialog()
        {
            CrewAssignmentDialog.Instance?.RefreshCrewLists(CrewAssignmentDialog.Instance.GetManifest(true), false, true, null);
            CrewAssignmentDialog.Instance?.ButtonClear();
            CrewAssignmentDialog.Instance?.ButtonFill();

            if (AstronautComplex != null)
                RebuildCrewLists?.Invoke(AstronautComplex, null);
        }

        /// <summary>
        /// Creates or updates a kerbal
        /// </summary>
        /// <param name="crewNode"></param>
        private static void LoadKerbal(ConfigNode crewNode)
        {
            var protoCrew = new ProtoCrewMember(HighLogic.CurrentGame.Mode, crewNode);
            if (string.IsNullOrEmpty(protoCrew.name))
            {
                LunaLog.LogError("[LMP]: protoName is blank!");
                return;
            }

            protoCrew.type = ProtoCrewMember.KerbalType.Crew;
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
        private static void UpdateKerbalData(ConfigNode crewNode, ProtoCrewMember protoCrew)
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
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].rosterStatus = protoCrew.rosterStatus;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].seat = protoCrew.seat;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].seatIdx = protoCrew.seatIdx;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].stupidity = protoCrew.stupidity;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].trait = protoCrew.trait;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].UTaR = protoCrew.UTaR;
            HighLogic.CurrentGame.CrewRoster[protoCrew.name].veteran = protoCrew.veteran;
        }

        #endregion
    }
}
