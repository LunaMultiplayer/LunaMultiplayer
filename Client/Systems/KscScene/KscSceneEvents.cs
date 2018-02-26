using KSP.UI.Screens;
using LunaClient.Base;
using LunaClient.Utilities;
using LunaClient.VesselStore;
using System.Linq;

namespace LunaClient.Systems.KscScene
{
    public class KscSceneEvents: SubSystem<KscSceneSystem>
    {
        public void AstronautComplexSpawn()
        {
            System.SceneIsAstronautComplex = true;
            HighLogic.CurrentGame?.flightState?.protoVessels?.Clear();
            HighLogic.CurrentGame?.flightState?.protoVessels?.AddRange(VesselsProtoStore.AllPlayerVessels.Values.Select(v => v.ProtoVessel));
        }

        public void AstronautComplexDespawn()
        {
            System.SceneIsAstronautComplex = false;
            HighLogic.CurrentGame?.flightState?.protoVessels?.Clear();
            System.ClearVesselMarkers?.Invoke(KSCVesselMarkers.fetch, null);
        }

        public void LevelLoaded(GameScenes data)
        {
            if (data == GameScenes.SPACECENTER)
            {
                System.ClearVesselMarkers?.Invoke(KSCVesselMarkers.fetch, null);

                //Delay it to have time to recover the vessel, crew and funds
                CoroutineUtil.StartDelayedRoutine("ClearVesselsInKsc", () =>
                {
                    HighLogic.CurrentGame?.flightState?.protoVessels?.Clear();
                    System.ClearVesselMarkers?.Invoke(KSCVesselMarkers.fetch, null);
                }, 3);
            }
        }
    }
}
