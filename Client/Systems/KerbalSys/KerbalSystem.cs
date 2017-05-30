using LunaClient.Base;
using LunaClient.Utilities;
using LunaCommon;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Systems.KerbalSys
{
    /// <summary>
    /// System that handles the kerbals between client and the server.
    /// TODO: we should add messages that "take" a kerbal and remove that kerbal from other clients roster and messages that "return" the kerbal once we recover a vessel
    /// </summary>
    public class KerbalSystem : MessageSystem<KerbalSystem, KerbalMessageSender, KerbalMessageHandler>
    {
        #region Fields

        public Queue<ConfigNode> KerbalQueue { get; } = new Queue<ConfigNode>();
        public Dictionary<string, string> ServerKerbals { get; } = new Dictionary<string, string>();

        #endregion

        #region Base overrides

        protected override void OnDisabled()
        {
            base.OnDisabled();
            KerbalQueue.Clear();
            ServerKerbals.Clear();
        }

        #endregion

        #region Public

        /// <summary>
        /// This method is called from the Main system to load all the kerbals we received 
        /// when connecting to a server into the game
        /// </summary>
        public void LoadKerbalsIntoGame()
        {
            LunaLog.Log("[LMP]: Loading kerbals into game");
            while (KerbalQueue.Count > 0)
            {
                LoadKerbal(KerbalQueue.Dequeue());
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
                    LunaLog.Log($"[LMP]: Generating {generateKerbals} new kerbals");
                    for (var i = 0; i < generateKerbals; i++)
                    {
                        var protoKerbal = HighLogic.CurrentGame.CrewRoster.GetNewKerbal();
                        MessageSender.SendKerbalIfDifferent(protoKerbal);
                    }
                }
            }

            LunaLog.Log("[LMP]: Kerbals loaded");
        }

        public void LoadKerbal(ConfigNode crewNode)
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
                CreateKerbal(protoCrew);
            }
            else
            {
                UpdateKerbalData(crewNode, protoCrew);
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Creates a new Kerbal
        /// </summary>
        private void CreateKerbal(ProtoCrewMember protoCrew)
        {
            HighLogic.CurrentGame.CrewRoster.AddCrewMember(protoCrew);
            var kerbalNode = new ConfigNode();
            protoCrew.Save(kerbalNode);
            var kerbalBytes = ConfigNodeSerializer.Serialize(kerbalNode);
            if (kerbalBytes != null && kerbalBytes.Length != 0)
                ServerKerbals[protoCrew.name] = Common.CalculateSha256Hash(kerbalBytes);
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
