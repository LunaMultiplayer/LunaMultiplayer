using KSP.Sim.impl;
using System;
using System.Collections.Generic;

namespace LunaMultiplayer.KSP2.Sync
{
    /// <summary>
    /// 飞船状态同步层（KSP2 专属，重写自 LMP 的 KSP1 同步代码）。
    /// 使用的 KSP.Sim API 均经 .NET 反射实测，见 KSP2_MP_DESIGN.md §2。
    /// </summary>
    public static class VesselSync
    {
        // ---- 本地权威飞船：采集状态并广播 ----
        public static void CollectAndSend()
        {
            var sim = Plugin.Sim;
            if (sim == null) return;

            foreach (var guid in sim.GetVesselGuids())
            {
                var vc = sim.GetSimulationObjectComponent<VesselComponent>(guid);
                if (vc == null || !vc.IsLocallyOwned) continue; // 只同步本地拥有的飞船

                // 采集（逐字段，绕过不透明的 VesselState）：
                var pos = vc.transform?.Position;          // 位置
                var rot = vc.transform?.Rotation;          // 旋转
                var vel = vc.Velocity;                     // 速度
                var angVel = vc.AngularVelocity;           // 角速度
                var orbit = vc.Orbit;                      // 轨道根数
                var fuel = vc.FuelPercentage;              // 总燃料百分比
                var elec = vc.StoredElectricityPercentage; // 电力
                var situation = vc.Situation;              // 姿态（着陆/溅落/飞行…）

                // TODO: 序列化为 VesselStatePacket -> Network 层发送
            }
        }

        // ---- 远端飞船：应用插值后的状态 ----
        public static void ApplyRemote(IGGuid guid, VesselStatePacket pkt)
        {
            var sim = Plugin.Sim;
            if (sim == null) return;

            var vc = sim.GetSimulationObjectComponent<VesselComponent>(guid);
            if (vc == null || vc.IsLocallyOwned) return; // 不覆盖本地权威飞船

            // 写入 transform（位置/旋转），由 VesselBehavior.SyncTo 推到渲染层
            // TODO: vc.transform.Position = pkt.Pos; vc.transform.Rotation = pkt.Rot;
            //       vc.Velocity = pkt.Vel; vc.AngularVelocity = pkt.AngVel;
            //       vc.FuelPercentage = pkt.Fuel; 等
        }
    }

    // TODO: 定义数据包结构（后续从 LMP 的 MessageSystem 移植）。
    public struct VesselStatePacket
    {
        public IGGuid Guid;
        public double PosX, PosY, PosZ;
        public double RotX, RotY, RotZ, RotW;
        public double VelX, VelY, VelZ;
        public double AngVelX, AngVelY, AngVelZ;
        public double Fuel;
        public double Electric;
        // 轨道/姿态按需扩展
    }
}
