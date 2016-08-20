using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VentureLab.Asx;
using VentureLab.Helpers;
using VentureLab.QbClustering;
using VentureLab.QbGaussianMethod.Cores;

namespace VentureLabTests
{
    [TestClass]
    public class StrainScoringTests
    {
        public class SimpleStrainPoint : Point, IStrainPoint
        {
            public class Factory : IPointFactory
            {
                public IPoint Create()
                {
                    return new SimpleStrainPoint(SampleAccessor.InputCount, SampleAccessor.OutputCount);
                }

                public ICore CreateCore(IPoint point)
                {
                    throw new NotSupportedException();
                }

                public static Factory Instance { get; } = new Factory();
            }

            public SimpleStrainPoint(int inputLen, int outputLen) : base(inputLen, outputLen)
            {
            }

            public double Indicator
            {
                get;set;
            }

            public int CompareTo(IStrainPoint other) => Indicator.CompareTo(other.Indicator);
        }

        private delegate IEnumerable<double> GenerateSequenceCallback(int numSamples);
        private delegate IEnumerable<double> GenerateVolumeCallback(int numDays);

        private const double inputAbsThr = SampleAccessor.InputCount * 0.1;
        private const double outputAbsThr = SampleAccessor.OutputCount * 0.1;
        private const double outputAbsNormalizer = 1.0 / inputAbsThr;

        private readonly static double inputSqrThr = Math.Sqrt(SampleAccessor.InputCount) * 0.1;
        private readonly static double outputSqrThr = Math.Sqrt(SampleAccessor.OutputCount) * 0.1;
        private readonly static double outputSqrNormalizer = 1.0 / inputSqrThr;

        private class Generator
        {
            public Generator(GenerateSequenceCallback gscb, GenerateVolumeCallback gvcb)
            {
                GenerateSequence = gscb;
                GenerateVolume = gvcb;
            }

            public GenerateSequenceCallback GenerateSequence { get; }
            public GenerateVolumeCallback GenerateVolume { get; }
        }

        private Generator SineGenerator = new Generator(GenerateSineCallback, GenerateSineCallback);
        private Generator RandomGenerator = new Generator(numSamples => GenerateRandomSequence(numSamples),
            numSamples => GenerateRandomSequence(numSamples));

        private static Random _rand = new Random(123);

        #region Function tests

        [TestMethod]
        public void TestRandomStrainAbs()
        {
            var strains = GenerateStrains(20, RandomGenerator).ToList();
            var adapter = AbsStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputAbsThr, outputAbsNormalizer);
            var stfs = GetStrainScoresFullSearch(strains, adapter, scorer);
            adapter.UpdatePointsIndicators(strains);
            var stsmart = strains.GetScores(adapter, scorer);
            var stsmartprl = strains.GetScoresParallel(adapter, scorer);
            for (var i = 0; i < strains.Count; i++)
            {
                var strain1 = strains[i];
                for (var j = i; j < strains.Count; j++)
                {
                    var strain2 = strains[j];
                    var scorefs = stfs[strain1, strain2];
                    var scoresmart = stsmart[strain1, strain2];
                    var scoresmartprl = stsmartprl[strain1, strain2];
                    Assert.IsTrue(Math.Abs(scorefs - scoresmart) < 0.01);
                    Assert.IsTrue(Math.Abs(scoresmartprl - scoresmart) < 0.01);
                }
            }
        }
        

        [TestMethod]
        public void TestRandomStrainSqr()
        {
            var strains = GenerateStrains(20, RandomGenerator).ToList();
            var adapter = SqrStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputSqrThr, outputSqrNormalizer);
            var stfs = GetStrainScoresFullSearch(strains, adapter, scorer);
            adapter.UpdatePointsIndicators(strains);
            var stsmart = strains.GetScores(adapter, scorer);
            var stsmartprl = strains.GetScoresParallel(adapter, scorer);
            for (var i = 0; i < strains.Count; i++)
            {
                var strain1 = strains[i];
                for (var j = i; j < strains.Count; j++)
                {
                    var strain2 = strains[j];
                    var scorefs = stfs[strain1, strain2];
                    var scoresmart = stsmart[strain1, strain2];
                    var scoresmartprl = stsmartprl[strain1, strain2];
                    Assert.IsTrue(Math.Abs(scorefs - scoresmart) < 0.01);
                    Assert.IsTrue(Math.Abs(scoresmartprl - scoresmart) < 0.01);
                }
            }
        }

        [TestMethod]
        public void TestSineStrainAbs()
        {
            var strains = GenerateStrains(20, SineGenerator).ToList();
            var adapter = AbsStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputAbsThr, outputAbsNormalizer);
            var stfs = GetStrainScoresFullSearch(strains, adapter, scorer);
            adapter.UpdatePointsIndicators(strains);
            var stsmart = strains.GetScores(adapter, scorer);
            var stsmartprl = strains.GetScoresParallel(adapter, scorer);
            for (var i = 0; i < strains.Count; i++)
            {
                var strain1 = strains[i];
                for (var j = i; j < strains.Count; j++)
                {
                    var strain2 = strains[j];
                    var scorefs = stfs[strain1, strain2];
                    var scoresmart = stsmart[strain1, strain2];
                    var scoresmartprl = stsmartprl[strain1, strain2];
                    Assert.IsTrue(Math.Abs(scorefs - scoresmart) < 0.01);
                    Assert.IsTrue(Math.Abs(scoresmartprl - scoresmart) < 0.01);
                }
            }
        }

        [TestMethod]
        public void TestSineStrainSqr()
        {
            var strains = GenerateStrains(20, SineGenerator).ToList();
            var adapter = SqrStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputSqrThr, outputSqrNormalizer);
            var stfs = GetStrainScoresFullSearch(strains, adapter, scorer);
            adapter.UpdatePointsIndicators(strains);
            var stsmart = strains.GetScores(adapter, scorer);
            var stsmartprl = strains.GetScoresParallel(adapter, scorer);
            for (var i = 0; i < strains.Count; i++)
            {
                var strain1 = strains[i];
                for (var j = i; j < strains.Count; j++)
                {
                    var strain2 = strains[j];
                    var scorefs = stfs[strain1, strain2];
                    var scoresmart = stsmart[strain1, strain2];
                    var scoresmartprl = stsmartprl[strain1, strain2];
                    Assert.IsTrue(Math.Abs(scorefs - scoresmart) < 0.01);
                    Assert.IsTrue(Math.Abs(scoresmartprl - scoresmart) < 0.01);
                }
            }
        }

        #endregion

        #region Kind of performance tests

        [TestMethod]
        public void TestSpeedRandomStrainAbsSmart()
        {
            var strains = GenerateStrains(20, RandomGenerator).ToList();
            var adapter = AbsStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputAbsThr, outputAbsNormalizer);
            adapter.UpdatePointsIndicators(strains);
            var stsmart = strains.GetScores(adapter, scorer);
        }

        [TestMethod]
        public void TestSpeedRandomStrainAbsSmartParallel()
        {
            var strains = GenerateStrains(20, RandomGenerator).ToList();
            var adapter = AbsStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputAbsThr, outputAbsNormalizer);
            adapter.UpdatePointsIndicators(strains);
            var stsmart = strains.GetScoresParallel(adapter, scorer);
        }

        [TestMethod]
        public void TestSpeedRandomStrainAbsFullSearch()
        {
            var strains = GenerateStrains(20, RandomGenerator).ToList();
            var adapter = AbsStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputAbsThr, outputAbsNormalizer);
            var stfs = GetStrainScoresFullSearch(strains, adapter, scorer);
        }


        [TestMethod]
        public void TestSpeedRandomStrainSqrSmart()
        {
            var strains = GenerateStrains(20, RandomGenerator).ToList();
            var adapter = SqrStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputSqrThr, outputSqrNormalizer);
            adapter.UpdatePointsIndicators(strains);
            var stsmart = strains.GetScores(adapter, scorer);
        }

        [TestMethod]
        public void TestSpeedRandomStrainSqrSmartParallel()
        {
            var strains = GenerateStrains(20, RandomGenerator).ToList();
            var adapter = SqrStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputSqrThr, outputSqrNormalizer);
            adapter.UpdatePointsIndicators(strains);
            var stsmart = strains.GetScoresParallel(adapter, scorer);
        }

        [TestMethod]
        public void TestSpeedRandomStrainSqrFullSearch()
        {
            var strains = GenerateStrains(20, RandomGenerator).ToList();
            var adapter = SqrStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputSqrThr, outputSqrNormalizer);
            var stfs = GetStrainScoresFullSearch(strains, adapter, scorer);
        }

        [TestMethod]
        public void TestSpeedSineStrainAbsSmart()
        {
            var strains = GenerateStrains(20, SineGenerator).ToList();
            var adapter = AbsStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputAbsThr, outputAbsNormalizer);
            adapter.UpdatePointsIndicators(strains);
            var stsmart = strains.GetScores(adapter, scorer);
        }

        [TestMethod]
        public void TestSpeedSineStrainAbsSmartParallel()
        {
            var strains = GenerateStrains(20, SineGenerator).ToList();
            var adapter = AbsStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputAbsThr, outputAbsNormalizer);
            adapter.UpdatePointsIndicators(strains);
            var stsmart = strains.GetScoresParallel(adapter, scorer);
        }

        [TestMethod]
        public void TestSpeedSineStrainAbsFullSearch()
        {
            var strains = GenerateStrains(20, SineGenerator).ToList();
            var adapter = AbsStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputAbsThr, outputAbsNormalizer);
            var stfs = GetStrainScoresFullSearch(strains, adapter, scorer);
        }

        [TestMethod]
        public void TestSpeedSineStrainSqrSmart()
        {
            var strains = GenerateStrains(20, SineGenerator).ToList();
            var adapter = SqrStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputSqrThr, outputSqrNormalizer);
            adapter.UpdatePointsIndicators(strains);
            var stsmart = strains.GetScores(adapter, scorer);
        }

        [TestMethod]
        public void TestSpeedSineStrainSqrSmartParallel()
        {
            var strains = GenerateStrains(20, SineGenerator).ToList();
            var adapter = SqrStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputSqrThr, outputSqrNormalizer);
            adapter.UpdatePointsIndicators(strains);
            var stsmart = strains.GetScoresParallel(adapter, scorer);
        }

        [TestMethod]
        public void TestSpeedSineStrainSqrFullSearch()
        {
            var strains = GenerateStrains(20, SineGenerator).ToList();
            var adapter = SqrStrainAdapter.Instance;
            var scorer = new SimpleScorer(inputSqrThr, outputSqrNormalizer);
            var stfs = GetStrainScoresFullSearch(strains, adapter, scorer);
        }

        #endregion

        private static ScoreTable GetStrainScoresFullSearch(IList<IStrain> strains, StrainAdapter adapter, IScorer scorer)
        {
            var numStrains = strains.Count;
            var scores = new ScoreTable();
            for (var i = 0; i < numStrains; i++)
            {
                var strain1 = strains[i];
                for (var j = i; j < numStrains; j++)
                {
                    var strain2 = strains[j];
                    var score = GetStrainScoreFullSearch(strain1, strain2, adapter, scorer);
                    scores[strain1, strain2] = score;
                }
            }
            return scores;
        }

        private static double GetStrainScoreFullSearch(IStrain strain1, IStrain strain2, StrainAdapter adapter, IScorer scorer)
        {
            var sum = 0.0;
            foreach (var p1 in strain1.Points)
            {
                foreach (var p2 in strain2.Points)
                {
                    var inputDiff = adapter.GetInputDiff(p1, p2);
                    var outputDiff = adapter.GetOutputDiff(p1, p2);
                    var s = scorer.Score(inputDiff, outputDiff);
                    sum += s;
                }
            }
            var minCount = Math.Min(strain1.Points.Count, strain2.Points.Count);
            return sum / minCount;
        }

        private IEnumerable<IStrain> GenerateStrains(int numStrains, Generator generator, int samplesPerDay = 4, int lenMin = SampleAccessor.DaysBefore + SampleAccessor.DaysAfter + 1, int lenMax = SampleAccessor.DaysBefore + SampleAccessor.DaysAfter + 200)
        {
            for (var i = 0; i < numStrains; i++)
            {
                var numDays = _rand.Next(lenMin, lenMax);
                var de = GenerateDailyEntries(numDays, samplesPerDay, generator).ToList();
                int start, end;
                AsxSamplingHelper.GetStartAndEnd(de.Count, out start, out end);
                var strainPoints = SimpleStrainPoint.Factory.Instance.Sample(de, start, end, 1).ToList();
                var strain = new Strain
                {
                    Points = strainPoints
                };
                yield return strain;
            }
        }
        
        private IEnumerable<DailyEntry> GenerateDailyEntries(int numDays, int samplesPerDay, Generator generator)
        {
            var hourSequence = generator.GenerateSequence(numDays*samplesPerDay);
            var volumes = generator.GenerateVolume(numDays).GetEnumerator();
            var step = 0;
            DailyEntry entry = null;
            double low = 0, high = 0;
            foreach (var v in hourSequence)
            {
                if (step == 0)
                {
                    entry = new DailyEntry();
                    entry.Open = v;
                    low = v;
                    high = v;
                }
                else
                {
                    if (v > high) high = v;
                    else if (v < low) low = v;
                }
                if (step == samplesPerDay-1)
                {
                    entry.Close = v;
                    entry.High = high;
                    entry.Low = low;
                    if (volumes.MoveNext())
                    {
                        entry.Volume = volumes.Current;
                    }
                    yield return entry;
                }
                step++;
                if (step == samplesPerDay) step = 0;
            }
        }

        private static IEnumerable<double> GenerateRandomSequence(int len, double min=0, double max=100, double delta = 1)
        {
            var range = max - min;
            var v = _rand.NextDouble() * range + min;
            yield return v;
            for (var i = 1; i < len; i++)
            {
                var d = _rand.NextDouble();
                var pmin = v < min ? v : min;
                var pmax = v > max ? v : max;
                var prange = pmax - pmin;
                var p = (v - pmin) / prange;
                d -= p;
                d *= delta;
                v = v + d;
                yield return v;
            }
        }

        private static IEnumerable<double> GenerateSine(int len, double period, double min = 0.0, double max = 1.0, double initPhase = 0.0,  double noise = 0.0)
        {
            var f = 2 * Math.PI / period;
            for (var i = 0; i < len; i++)
            {
                var v = Math.Sin(f * i + initPhase);
                v += 1;
                v *= (max - min) / 2;
                v += min;
                if (noise != 0.0)
                {
                    var n = _rand.NextDouble() * 2 - 1;
                    n *= noise;
                    v += n;
                }
                yield return v;        
            }
        }

        private static IEnumerable<double> GenerateSineCallback(int numSamples)
        {
            var period = _rand.NextDouble() * 100 + 50;
            var phase = _rand.NextDouble() * Math.PI * 2;
            var min = Math.Max(0, _rand.NextDouble() * 20 - 5);
            var max = min * (1.2 + _rand.NextDouble());
            var noise = (max - min) / 20;
            return GenerateSine(numSamples, period, min, max, phase, noise);
        }
    }
}
