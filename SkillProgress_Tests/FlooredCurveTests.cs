using System;
using System.Collections.Generic;
using NUnit.Framework;
using SkillProgress;

namespace SkillProgress_Tests
{
    [TestFixture]
    public class FlooredCurveTests
    {
        private const float TOLERANCE = 0.000001f;
        
        [Test]
        public void TestValues()
        {
            var curve = CreateCurve();
            Assert.IsTrue(Math.Abs(curve.GetValue(-1)) < TOLERANCE);
            Assert.IsTrue(Math.Abs(curve.GetValue(1)) < TOLERANCE);
            Assert.IsTrue(Math.Abs(curve.GetValue(3) - 20) < TOLERANCE);
            Assert.IsTrue(Math.Abs(curve.GetValue(20) - 100) < TOLERANCE);
        }

        [Test]
        public void TestPercentage()
        {
            var curve = CreateCurve();
            Assert.IsTrue(Math.Abs(curve.GetPercentageToNextPoint(1) - 0.5f) < TOLERANCE);
            Assert.IsTrue(Math.Abs(curve.GetPercentageToNextPoint(3) - 0.5f) < TOLERANCE);
            Assert.IsTrue(Math.Abs(curve.GetPercentageToNextPoint(4)) < TOLERANCE);
            Assert.IsTrue(Math.Abs(curve.GetPercentageToNextPoint(9.5f) - 0.75f) < TOLERANCE);
            Assert.IsTrue(Math.Abs(curve.GetPercentageToNextPoint(10) - 1f) < TOLERANCE);
        }

        private FlooredCurve<float, float> CreateCurve()
        {
            return new FlooredCurve<float, float>(new List<Tuple<float, float>>()
            {
                new Tuple<float, float>(0, 0),
                new Tuple<float, float>(2, 20),
                new Tuple<float, float>(4, 40),
                new Tuple<float, float>(8, 80),
                new Tuple<float, float>(10, 100)
            });
        }
    }
}