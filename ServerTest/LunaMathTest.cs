using LmpCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ServerTest
{
    [TestClass]
    public class LunaMathTest
    {
        [TestMethod]
        public void TestLerp()
        {
            Assert.AreEqual(15.0, LunaMath.Lerp(10.0, 20.0, 0.5f));
            Assert.AreEqual(10.0, LunaMath.Lerp(10.0, 20.0, 0.0f));
            Assert.AreEqual(20.0, LunaMath.Lerp(10.0, 20.0, 1.0f));
        }

        [TestMethod]
        public void TestLerpAngleDeg()
        {
            // Normal lerp
            Assert.AreEqual(45.0, LunaMath.LerpAngleDeg(0.0, 90.0, 0.5f), 0.001);

            // Wrap around (350 to 10 should go through 0)
            // LunaMath.LerpAngleDeg(350, 10, 0.5) -> Repeat(10-350, 360) -> Repeat(-340, 360) -> 20.
            // 20 > 180 is false. Result: 350 + 20 * 0.5 = 360.
            Assert.AreEqual(360.0, LunaMath.LerpAngleDeg(350.0, 10.0, 0.5f), 0.001);

            // Reverse wrap around (10 to 350 should go through 0)
            // LunaMath.LerpAngleDeg(10, 350, 0.5) -> Repeat(350-10, 360) -> Repeat(340, 360) -> 340.
            // 340 > 180 is true. single = 340 - 360 = -20.
            // Result: 10 + (-20) * 0.5 = 0.
            Assert.AreEqual(0.0, LunaMath.LerpAngleDeg(10.0, 350.0, 0.5f), 0.001);
        }

        [TestMethod]
        public void TestLerpAngleRad()
        {
            // PI/4 (45 deg)
            Assert.AreEqual(Math.PI / 4.0, LunaMath.LerpAngleRad(0.0, Math.PI / 2.0, 0.5f), 0.001);

            // Wrap around near 2*PI
            // from = 2*PI - 0.1, to = 0.1, t = 0.5
            // Repeat(0.1 - (2*PI - 0.1), 2*PI) -> Repeat(0.2 - 2*PI, 2*PI) -> 0.2
            // Result: (2*PI - 0.1) + 0.2 * 0.5 = 2*PI
            var nearTwoPi = 2.0 * Math.PI - 0.1;
            var pastZero = 0.1;
            Assert.AreEqual(2.0 * Math.PI, LunaMath.LerpAngleRad(nearTwoPi, pastZero, 0.5f), 0.001);
        }

        [TestMethod]
        public void TestClamping()
        {
            Assert.AreEqual(0.5, LunaMath.Clamp(0.5, 0.0, 1.0));
            Assert.AreEqual(0.0, LunaMath.Clamp(-0.5, 0.0, 1.0));
            Assert.AreEqual(1.0, LunaMath.Clamp(1.5, 0.0, 1.0));

            Assert.AreEqual(0.0, LunaMath.Clamp01(-1.0));
            Assert.AreEqual(1.0, LunaMath.Clamp01(2.0));
        }

        [TestMethod]
        public void TestSafeDivision()
        {
            Assert.AreEqual(5.0, LunaMath.SafeDivision(10.0, 2.0));
            Assert.AreEqual(0.0, LunaMath.SafeDivision(10.0, 0.0));
        }
    }
}
