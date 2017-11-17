using KSP.UI;
using KSP.UI.Screens;
using LunaClient.Base;
using System.Collections.Concurrent;
using System.Reflection;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.KerbalSys
{
    /// <summary>
    /// System that handles the kerbals between client and the server.
    /// </summary>
    public class KerbalSystem : MessageSystem<KerbalSystem, KerbalMessageSender, KerbalMessageHandler>
    {
        #region Fields

        public ConcurrentDictionary<string, KerbalStructure> Kerbals { get; } = new ConcurrentDictionary<string, KerbalStructure>();

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
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, LoadKerbals));
            SetupRoutine(new RoutineDefinition(5000, RoutineExecution.Update, CheckKerbalNumber));
            GameEvents.OnCrewmemberHired.Add(KerbalEvents.CrewAdd);
            GameEvents.OnCrewmemberLeftForDead.Add(KerbalEvents.CrewRemove);
            GameEvents.OnCrewmemberSacked.Add(KerbalEvents.CrewRemove);
            GameEvents.onFlightReady.Add(KerbalEvents.FlightReady);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            Kerbals.Clear();
            GameEvents.OnCrewmemberHired.Remove(KerbalEvents.CrewAdd);
            GameEvents.OnCrewmemberLeftForDead.Remove(KerbalEvents.CrewRemove);
            GameEvents.OnCrewmemberSacked.Remove(KerbalEvents.CrewRemove);
            GameEvents.onFlightReady.Remove(KerbalEvents.FlightReady);
        }

        #endregion

        #region Routines

        /// <summary>
        /// Checks if the crew on a protoVessel has changed and sends the message accordingly
        /// </summary>
        public void ProcessKerbalsInVessel(ProtoVessel protoVessel)
        {
            if (protoVessel == null) return;

            foreach (var protoCrew in protoVessel.GetVesselCrew())
            {
                MessageSender.SendKerbalIfDifferent(protoCrew);
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
                MessageSender.SendKerbalIfDifferent(protoCrew);
            }
        }

        /// <summary>
        /// Loads the unloaded (either because they are new or they are updated) kerbals into the game
        /// </summary>
        private void LoadKerbals()
        {
            if (KerbalSystemReady)
            {
                var refreshDialog = Kerbals.Values.Any(v => !v.Loaded);
                foreach (var kerbal in Kerbals.Values.Where(v => !v.Loaded))
                {
                    LoadKerbal(kerbal.KerbalData);
                    kerbal.Loaded = true;
                }

                if (refreshDialog)
                    RefreshCrewDialog();
            }
        }

        /// <summary>
        /// Checks the kerbals that are in the server in order to spawn some more if needed
        /// </summary>
        private void CheckKerbalNumber()
        {
            if (KerbalSystemReady)
            {
                switch (Kerbals.Count)
                {
                    case 0:
                        //Server is new and don't have kerbals at all
                        var newRoster = KerbalRoster.GenerateInitialCrewRoster(HighLogic.CurrentGame.Mode);
                        foreach (var pcm in newRoster.Crew)
                        {
                            HighLogic.CurrentGame.CrewRoster.AddCrewMember(pcm);
                            MessageSender.SendKerbalIfDifferent(pcm);
                        }
                        break;
                    case int n when n < 20:
                        //Server has less than 20 kerbals so generate some
                        var generateKerbals = 20 - Kerbals.Count;
                        LunaLog.Log($"[LMP]: Generating {generateKerbals} new kerbals");
                        for (var i = 0; i < generateKerbals; i++)
                        {
                            var protoKerbal = HighLogic.CurrentGame.CrewRoster.GetNewKerbal();
                            MessageSender.SendKerbalIfDifferent(protoKerbal);
                        }
                        break;
                }
            }
        }

        #endregion

        #region Public

        /// <summary>
        /// Load all the received kerbals from the server into the game
        /// This should be called before the game starts as otherwise loading vessels with crew will fail
        /// </summary>
        public void LoadKerbalsIntoGame()
        {
            LoadKerbals();
        }

        /// <summary>
        /// Call this method to refresh the crews in the vessel spawn, vessel editor and astronaut complex
        /// </summary>
        public void RefreshCrewDialog()
        {
            CrewAssignmentDialog.Instance?.RefreshCrewLists(CrewAssignmentDialog.Instance.GetManifest(true), false, true, null);

            if (AstronautComplex != null)
                RebuildCrewLists?.Invoke(AstronautComplex, null);
        }

        #endregion

        #region Private

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
