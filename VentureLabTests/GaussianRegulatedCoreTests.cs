using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VentureLab.QbGaussianMethod.Cores;
using VentureLab.QbGaussianMethod.Helpers;
using static VentureLab.QbGaussianMethod.Helpers.PredictionCommon;
using static VentureLab.QbGuassianMethod.Helpers.ConfinedGaussian;

namespace VentureLabTests
{
    [TestClass]
    public class GaussianRegulatedCoreTests
    {
        private Random _rand = new Random(123);

        private const double IdenticalThr = double.Epsilon;
        private const double RoughlyEqualThr = 0.0001;

        [TestMethod]
        public void TestGaussianRegulatedCoreRandomly()
        {
            var inputLen = _rand.Next(5, 25);
            var outputLen = _rand.Next(3, 10);
            var numCores = _rand.Next(50, 200);

            var cores = GenerateRandomCores(inputLen, outputLen, numCores).ToList();
            var input = GenerateRandomInput(inputLen).ToArray();

            var resg = new PredictionResult(outputLen);
            var resf = new PredictionResult(outputLen);
            PredictThruGeneric(resg, input, cores);
            PredictFast(resf, input, cores);


            double[] yg = new double[outputLen];
            GetExpectedYThruGeneric(yg, input, cores);
            double[] yf = new double[outputLen];
            GetExpectedYFast(yf, input, cores);

            for (var i = 0; i < outputLen; i++)
            {
                AssertRoughlyEqual(yg[i], yf[i]);
                AssertRoughlyEqual(yg[i], resg.Y[i], IdenticalThr);
                AssertRoughlyEqual(yf[i], resf.Y[i], IdenticalThr);
            }

            Generic.ZeroList(yg);
            Generic.ZeroList(yf);
            GetExpectedYYThruGeneric(yg, input, cores);
            GetExpectedYYFast(yf, input, cores);

            for (var i = 0; i < outputLen; i++)
            {
                AssertRoughlyEqual(yg[i], yf[i]);
                AssertRoughlyEqual(yg[i], resg.YY[i], IdenticalThr);
                AssertRoughlyEqual(yf[i], resf.YY[i], IdenticalThr);
            }

            var sg = GetStrengthThruGeneric(input, cores);
            var sf = GetStrengthFast(input, cores);
            AssertRoughlyEqual(sg, sf);
            AssertRoughlyEqual(sg, resg.Strength, IdenticalThr);
            AssertRoughlyEqual(sf, resf.Strength, IdenticalThr);
        }

        [TestMethod]
        public void TestGaussianRegulatedCoreRandomlyWithRecommendedParameters()
        {
            var inputLen = _rand.Next(5, 25);
            var outputLen = _rand.Next(3, 10);
            var numCores = _rand.Next(50, 200);
            var cores = GenerateRandomCores(inputLen, outputLen, numCores, false).ToList();
            var input = GenerateRandomInput(inputLen).ToArray();

            GaussianRegulatedCore.SetCoreVariables(cores, cores.Select(x=>x.Variables));

            double[] yg = new double[outputLen];
            GetExpectedYThruGeneric(yg, input, cores);
            double[] yf = new double[outputLen];
            GetExpectedYFast(yf, input, cores);

            for (var i = 0; i < outputLen; i++)
            {
                AssertRoughlyEqual(yg[i], yf[i]);
            }

            Generic.ZeroList(yg);
            Generic.ZeroList(yf);

            GetExpectedYYThruGeneric(yg, input, cores);
            GetExpectedYYFast(yf, input, cores);

            for (var i = 0; i < outputLen; i++)
            {
                AssertRoughlyEqual(yg[i], yf[i]);
            }
        }

        static void AssertRoughlyEqual(double d1, double d2, double thr = RoughlyEqualThr)
        {
            Assert.IsTrue(Math.Abs(d1 - d2) / (Math.Abs(d1) + Math.Abs(d2)) <= thr);
        }

        public IEnumerable<GaussianRegulatedCore> GenerateRandomCores(int inputLen, int outputLen, int numCores, bool generatePrecision=true)
        {
            for (var i = 0; i < numCores; i++)
            {
                yield return GenerateRandomCore(inputLen, outputLen, generatePrecision);
            }
        }

        public GaussianRegulatedCore GenerateRandomCore(int inputLen, int outputLen, bool generatePrecision = true)
        {
            var w = _rand.NextDouble();
            var r = _rand.NextDouble() + 0.5;
            var m = outputLen * r;
            var n = 1;
            var point = new Point(inputLen, outputLen);
            var coreConstants = new GaussianRegulatedCoreConstants(outputLen, m, n);
            var coreVariables = new GaussianRegulatedCoreVariables(inputLen, outputLen);
            var core = new GaussianRegulatedCore(point, coreConstants, coreVariables, w);
            for (var i= 0; i < inputLen; i++)
            {
                core.Point.Input[i] = _rand.NextDouble();
                if (generatePrecision)
                  core.Variables.K[i] = GenerateRandomNegPrecision();
            }
            for (var i = 0; i < outputLen; i++)
            {
                core.Point.Output[i] = _rand.NextDouble();
                if (generatePrecision)
                    core.L[i] = GenerateRandomNegPrecision();
            }
            core.Variables.UpdateLp();
            core.Variables.UpdateKp();
            core.Variables.UpdateNormalizer();
            return core;
        }

        private double GenerateRandomNegPrecision(double min = 0.01, double max = 100)
        {
            var r = _rand.NextDouble();
            var y = Math.Tan(r);
            if (y < min) return -min;
            if (y > max) return -max;
            return -y;
        }

        public IEnumerable<double> GenerateRandomInput(int inputLen)
        {
            for (var i = 0; i < inputLen; i++)
            {
                yield return _rand.NextDouble();
            }
        }
    }
}
