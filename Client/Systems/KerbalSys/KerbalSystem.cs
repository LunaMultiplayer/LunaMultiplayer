using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Utilities;
using LunaCommon;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.KerbalSys
{
    public class KerbalSystem : MessageSystem<KerbalSystem, KerbalMessageSender, KerbalMessageHandler>
    {
        #region Fields
        
        public Dictionary<string, Queue<KerbalEntry>> KerbalProtoQueue { get; } = new Dictionary<string, Queue<KerbalEntry>>();
        public Dictionary<string, string> ServerKerbals { get; } = new Dictionary<string, string>();

        #endregion

        #region Base overrides

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (Enabled)
            {
                ProcessReceivedKerbals();
            }
        }

        #endregion

        #region Public

        /// <summary>
        /// This method is called from the Main system to load all the kerbals we received 
        /// when connecting to a server into the game
        /// </summary>
        public void LoadKerbalsIntoGame()
        {
            Debug.Log("[LMP]: Loading kerbals into game");
            foreach (var kerbalQueue in KerbalProtoQueue)
            {
                while (kerbalQueue.Value.Count > 0)
                {
                    var kerbalEntry = kerbalQueue.Value.Dequeue();
                    LoadKerbal(kerbalEntry.KerbalNode);
                }
            }

            //Server is new and don't have kerbals at all
            if (ServerKerbals.Count == 0)
            {
                var newRoster = KerbalRoster.GenerateInitialCrewRoster(HighLogic.CurrentGame.Mode);
                foreach (var pcm in newRoster.Crew)
                {
                    HighLogic.CurrentGame.CrewRoster.AddCrewMember(pcm);
                    MessageSender.SendKerbalIfDifferent(pcm);
                }
            }
            else
            {
                //Server has less than 20 kerbals so generate some
                var generateKerbals = ServerKerbals.Count < 20 ? 20 - ServerKerbals.Count : 0;
                if (generateKerbals > 0)
                {
                    Debug.Log($"[LMP]: Generating {generateKerbals} new kerbals");
                    for (var i = 0; i < generateKerbals; i++)
                    {
                        var protoKerbal = HighLogic.CurrentGame.CrewRoster.GetNewKerbal();
                        MessageSender.SendKerbalIfDifferent(protoKerbal);
                    }
                }
            }

            Debug.Log("[LMP]: Kerbals loaded");
        }

        #endregion

        #region Private

        private void LoadKerbal(ConfigNode crewNode)
        {
            var protoCrew = new ProtoCrewMember(HighLogic.CurrentGame.Mode, crewNode);
            if (string.IsNullOrEmpty(protoCrew.name))
            {
                Debug.LogError("[LMP]: protoName is blank!");
                return;
            }

            protoCrew.type = ProtoCrewMember.KerbalType.Crew;
            if (!HighLogic.CurrentGame.CrewRoster.Exists(protoCrew.name))
            {
                HighLogic.CurrentGame.CrewRoster.AddCrewMember(protoCrew);
                var kerbalNode = new ConfigNode();
                protoCrew.Save(kerbalNode);
                var kerbalBytes = ConfigNodeSerializer.Singleton.Serialize(kerbalNode);
                if ((kerbalBytes != null) && (kerbalBytes.Length != 0))
                    ServerKerbals[protoCrew.name] = Common.CalculateSha256Hash(kerbalBytes);
            }
            else
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
                    Debug.Log($"[LMP]: Career log node for {protoCrew.name} is empty!");
                }

                var flightLogNode = crewNode.GetNode("FLIGHT_LOG");
                if (flightLogNode != null)
                {
                    //And here. Someone "cannot into" lists and how to protect them.
                    HighLogic.CurrentGame.CrewRoster[protoCrew.name].careerLog.Entries.Clear();
                    HighLogic.CurrentGame.CrewRoster[protoCrew.name].careerLog.Load(careerLogNode);
                }
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].courage = protoCrew.courage;
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].experience = protoCrew.experience;
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].experienceLevel = protoCrew.experienceLevel;
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].gender = protoCrew.gender;
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].hasToured = protoCrew.hasToured;
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].isBadass = protoCrew.isBadass;
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].rosterStatus = protoCrew.rosterStatus;
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].seat = protoCrew.seat;
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].seatIdx = protoCrew.seatIdx;
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].stupidity = protoCrew.stupidity;
                HighLogic.CurrentGame.CrewRoster[protoCrew.name].UTaR = protoCrew.UTaR;
            }
        }

        private void ProcessReceivedKerbals()
        {
            foreach (var kerbalProtoSubspace in KerbalProtoQueue.Select(k => k.Value))
            {
                //Load kerbals that are in the past, not in the future
                while ((kerbalProtoSubspace.Count > 0) && (kerbalProtoSubspace.Peek().PlanetTime < Planetarium.GetUniversalTime()))
                {
                    var kerbalEntry = kerbalProtoSubspace.Dequeue();
                    LoadKerbal(kerbalEntry.KerbalNode);
                }
            }
        }

        #endregion
    }
}
