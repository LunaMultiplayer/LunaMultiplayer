using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;

namespace LunaClient.Systems.KerbalSys
{
    public class KerbalEvents : SubSystem<KerbalSystem>
    {
        public void CrewAdd(ProtoCrewMember protoCrew, int crewCount)
        {
            System.MessageSender.SendKerbal(protoCrew);
        }
        
        /// <summary>
        /// Use this event to send the kerbals just when we start a flight.
        /// We use this event instead of onFlightReady as the latter is triggered once UI and everythign is ready and this one is triggered
        /// earlier in the chain
        /// </summary>
        public void SwitchSceneRequested(GameScenes data)
        {
            if (data == GameScenes.FLIGHT)
            {
                var crew = FlightDriver.newShipManifest;
                if (crew == null) return;

                foreach (var protoCrew in crew?.GetAllCrew(false))
                {
                    //Always set the kerbals in a vessel as assigned
                    System.SetKerbalStatusWithoutTriggeringEvent(protoCrew, ProtoCrewMember.RosterStatus.Assigned);
                    System.MessageSender.SendKerbal(protoCrew);
                }
            }
        }

        /// <summary>
        /// Event triggered when any kerbal status changes (kerbal dies, etc)
        /// </summary>
        public void StatusChange(ProtoCrewMember kerbal, ProtoCrewMember.RosterStatus previousStatus, ProtoCrewMember.RosterStatus newStatus)
        {
            if (previousStatus != newStatus)
            {
                var vesselId = System.FindVesselForKerbal(kerbal);
                if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                {
                    System.SetKerbalStatusWithoutTriggeringEvent(kerbal, newStatus);
                    System.MessageSender.SendKerbal(kerbal);
                }
                else
                {
                    System.SetKerbalStatusWithoutTriggeringEvent(kerbal, previousStatus);
                }
            }
        }

        /// <summary>
        /// This event is triggered when we hire a kerbal (previous type was applicant, new is crew)
        /// Also triggered when we sack a kerbal (previous type was crew, new is applicant)
        /// </summary>
        public void TypeChange(ProtoCrewMember kerbal, ProtoCrewMember.KerbalType previousType, ProtoCrewMember.KerbalType newType)
        {
            if (previousType != newType)
            {
                if (previousType == ProtoCrewMember.KerbalType.Crew && newType == ProtoCrewMember.KerbalType.Applicant && !SettingsSystem.ServerSettings.AllowSackKerbals)
                {
                    //This means that we sacked the crew and we are not allowed to do it
                    System.SetKerbalTypeWithoutTriggeringEvent(kerbal, ProtoCrewMember.KerbalType.Crew);
                    return;
                }

                System.SetKerbalTypeWithoutTriggeringEvent(kerbal, newType);
                System.MessageSender.SendKerbal(kerbal);
            }
        }
    }
}
