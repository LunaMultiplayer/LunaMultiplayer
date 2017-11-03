using LunaClient.Base;
using System.Collections.Concurrent;
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

        public bool KerbalSystemReady => Enabled && Time.timeSinceLevelLoad > 1f && FlightGlobals.ready && 
            HighLogic.LoadedScene >= GameScenes.SPACECENTER;

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, LoadKerbals));
            SetupRoutine(new RoutineDefinition(5000, RoutineExecution.Update, CheckKerbalNumber));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            Kerbals.Clear();
        }

        #endregion

        #region Routines

        /// <summary>
        /// Loads the unloaded (either because they are new or they are updated) kerbals into the game
        /// </summary>
        private void LoadKerbals()
        {
            if (KerbalSystemReady)
            {
                foreach (var kerbal in Kerbals.Values.Where(v => !v.Loaded))
                {
                    LoadKerbal(kerbal.KerbalData);
                    kerbal.Loaded = true;

                    UpdateKerbalInDictionary(kerbal);
                }
            }
        }

        /// <summary>
        /// Checks the kerbals that are in the server in order to spawn some more if needed
        /// </summary>
        private void CheckKerbalNumber()
        {
            if (KerbalSystemReady)
            {
                switch (HighLogic.CurrentGame.CrewRoster.GetAvailableCrewCount())
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

        #region Private

        /// <summary>
        /// Updates a value in the dictionary
        /// </summary>
        private void UpdateKerbalInDictionary(KerbalStructure kerbal)
        {
            Kerbals.TryGetValue(kerbal.Name, out var existingKerbal);
            if (existingKerbal != null)
            {
                Kerbals.TryUpdate(kerbal.Name, kerbal, existingKerbal);
            }
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
