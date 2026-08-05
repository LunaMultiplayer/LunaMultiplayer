using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KSP.Game;
using System;

namespace LunaMultiplayer.KSP2
{
    // BepInEx 入口。KSP2 用 Mono + BepInEx 注入（已由 SpaceWarp2 证明可行）。
    [BepInPlugin(PluginId, "LunaMultiplayer KSP2", "0.1.0")]
    [BepInDependency("SpaceWarp", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginId = "lunamultiplayer.ksp2";
        internal static Plugin Instance;
        internal static Harmony Harmony;
        internal static new ManualLogSource Logger;

        public void Awake()
        {
            Instance = this;
            Logger = base.Logger;
            Harmony = new Harmony(PluginId);
            Harmony.PatchAll();
            Logger.LogInfo("LunaMultiplayer.KSP2 loaded; patches applied. Waiting for a game session...");
        }

        // 中央仿真入口。
        // TODO(verify): 确认 Game 的静态访问器。反射实测 KSP.Game.GameInstance 含 SpaceSimulation 属性，
        // 访问方式很可能是 Game.Instance（或某 GameManager.Instance）。需在联机初始化时解析。
        internal static KSP.Sim.impl.SpaceSimulation Sim => Game.Instance?.SpaceSimulation;
    }
}
