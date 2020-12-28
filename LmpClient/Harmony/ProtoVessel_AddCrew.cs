using Harmony;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to fix protovessel add crew. In some cases the pcm comes with a null value since the kerbals are not totally synced and then
    /// the commnet is messed up and spams the log
    /// </summary>
    [HarmonyPatch(typeof(ProtoVessel))]
    [HarmonyPatch("AddCrew")]
    public class ProtoVessel_AddCrew
    {
        [HarmonyPrefix]
        private static bool PrefixAddCrew(ProtoCrewMember pcm)
        {
            return pcm != null;
        }
    }
}
