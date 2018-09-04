using Harmony;
using LunaClient.Systems.SettingsSys;

namespace LunaClient.Extensions
{
    public static class OrbitDriverExtension
    {
        public static void LmpUpdateOrbit(this OrbitDriver driver, double time, bool offset = true)
        {
            if (SettingsSystem.CurrentSettings.Debug9) return;

            if (!Traverse.Create(driver).Field("ready").GetValue<bool>())
            {
                return;
            }
            driver.lastMode = driver.updateMode;
            var updateMode = driver.updateMode;
            if (updateMode != OrbitDriver.UpdateMode.IDLE && updateMode != OrbitDriver.UpdateMode.TRACK_Phys)
            {
                if (updateMode == OrbitDriver.UpdateMode.UPDATE)
                {
                    driver.LmpUpdateFromParameters(time);
                    if (driver.vessel != null)
                    {
                        driver.CheckDominantBody(driver.referenceBody.position + driver.pos);
                    }
                }
            }
            else if (!(driver.vessel == null) && !(driver.vessel.rootPart == null) && !(driver.vessel.rootPart.rb == null))
            {
                if (!offset)
                {
                    Traverse.Create(driver).Field("fdtLast").SetValue(0);
                }
                if (!driver.CheckDominantBody(driver.vessel.CoMD))
                {
                    driver.LmpTrackRigidbody(driver.referenceBody, -Traverse.Create(driver).Field("fdtLast").GetValue<double>(), time);
                }
            }

            Traverse.Create(driver).Field("fdtLast").SetValue(TimeWarp.fixedDeltaTime);
            if (Traverse.Create(driver).Field("isHyperbolic").GetValue<bool>() && driver.orbit.eccentricity < 1)
            {
                Traverse.Create(driver).Field("isHyperbolic").SetValue(false);
                if (driver.vessel != null)
                {
                    GameEvents.onVesselOrbitClosed.Fire(driver.vessel);
                }
            }
            if (!Traverse.Create(driver).Field("isHyperbolic").GetValue<bool>() && driver.orbit.eccentricity > 1)
            {
                Traverse.Create(driver).Field("isHyperbolic").SetValue(true);
                if (driver.vessel != null)
                {
                    GameEvents.onVesselOrbitEscaped.Fire(driver.vessel);
                }
            }
            if (driver.drawOrbit)
            {
                driver.orbit.DrawOrbit();
            }
        }


        public static void LmpUpdateFromParameters(this OrbitDriver driver, double time)
        {
            Traverse.Create(driver).Field("updateUT").SetValue(time);
            //driver.updateUT = Planetarium.GetUniversalTime();

            driver.orbit.UpdateFromUT(time);
            driver.pos = driver.orbit.pos;
            driver.vel = driver.orbit.vel;
            driver.pos.Swizzle();
            driver.vel.Swizzle();
            if (driver.reverse)
            {
                driver.referenceBody.position = (driver.celestialBody ? (Vector3d)driver.driverTransform.position : driver.celestialBody.position) - driver.pos;
            }
            else if (driver.vessel != null)
            {
                driver.vessel.SetPosition((driver.referenceBody.position + driver.pos) - (driver.driverTransform.rotation * driver.vessel.localCoM));
            }
            else if (!driver.celestialBody)
            {
                driver.driverTransform.position = driver.referenceBody.position + driver.pos;
            }
            else
            {
                driver.celestialBody.position = driver.referenceBody.position + driver.pos;
            }
        }

        public static void LmpTrackRigidbody(this OrbitDriver driver, CelestialBody refBody, double fdtOffset, double time)
        {
            Traverse.Create(driver).Field("updateUT").SetValue(time);
            //driver.updateUT = Planetarium.GetUniversalTime();

            if (driver.vessel != null)
            {
                driver.pos = (driver.vessel.CoMD - driver.referenceBody.position).xzy;
            }
            if (driver.vessel != null && driver.vessel.rootPart != null && driver.vessel.rootPart.rb != null && !driver.vessel.rootPart.rb.isKinematic)
            {
                Traverse.Create(driver).Field("updateUT").SetValue(time + fdtOffset);
                //driver.updateUT += fdtOffset;
                driver.vel = driver.vessel.velocityD.xzy + driver.orbit.GetRotFrameVelAtPos(driver.referenceBody, driver.pos);
            }
            else if (driver.updateMode == OrbitDriver.UpdateMode.IDLE)
            {
                driver.vel = driver.orbit.GetRotFrameVel(driver.referenceBody);
            }
            if (refBody != driver.referenceBody)
            {
                if (driver.vessel != null)
                {
                    driver.pos = (driver.vessel.CoMD - refBody.position).xzy;
                }
                var frameVel = driver;
                frameVel.vel = frameVel.vel + (driver.referenceBody.GetFrameVel() - refBody.GetFrameVel());
            }

            driver.lastTrackUT = Traverse.Create(driver).Field("updateUT").GetValue<double>();
            //driver.lastTrackUT = driver.updateUT;
            driver.orbit.UpdateFromStateVectors(driver.pos, driver.vel, refBody, driver.lastTrackUT);
            driver.pos.Swizzle();
            driver.vel.Swizzle();
        }
    }
}
