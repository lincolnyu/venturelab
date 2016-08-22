using QLogger.Parallelism;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VentureLab.QbClustering
{
    public static class StrainsHelper
    {
        public delegate void ReportGetScoresProgress(int done, int total);

        public class GetScoresParallelizer : Parallelizer
        {
            private class StrainPair
            {
                public StrainPair(IStrain strain1, IStrain strain2) { Strain1 = strain1; Strain2 = strain2; }
                public IStrain Strain1 { get; }
                public IStrain Strain2 { get; }
            }

            public GetScoresParallelizer(int maxDegreeOfParallelism = int.MaxValue)
                : base(maxDegreeOfParallelism)
            {
            }

            public static GetScoresParallelizer Instance = new GetScoresParallelizer();

            public ScoreTable GetScoresParallel(IList<IStrain> strains, StrainAdapter adapter, IScorer scorer, int inc = 1, ReportGetScoresProgress reportProgress = null)
            {
                var numStrains = strains.Count;
                var scores = new ScoreTable();
                var total = (numStrains + 1) * numStrains / 2;
                var count = 0;

                var strainPairs = GetStrainPairs(strains);
                Parallel.ForEach(strainPairs, ParallelOptions, strainPair =>
                {
                    var strain1 = strainPair.Strain1;
                    var strain2 = strainPair.Strain2;
                    var score = adapter.Score(strain1, strain2, scorer, inc);
                    if (score > 0)
                    {
                        lock (scores)
                        {
                            scores[strain1, strain2] = score;
                            reportProgress?.Invoke(++count, total);
                        }
                    }
                });
                return scores;
            }

            private static IEnumerable<StrainPair> GetStrainPairs(IList<IStrain> strains)
            {
                var strainCount = strains.Count;
                for (var i = 0; i < strainCount; i++)
                {
                    var strain1 = strains[i];
                    for (var j = i; j < strainCount; j++)
                    {
                        var strain2 = strains[j];
                        yield return new StrainPair(strain1, strain2);
                    }
                }
            }
        }

        public static void UpdatePointsIndicators(this StrainAdapter adapter, IEnumerable<IStrain> strains)
        {
            foreach (var strain in strains)
            {
                adapter.UpdatePointsIndicators(strain.Points);
                adapter.ReorderPoints(strain);
            }
        }

        public static ScoreTable GetScores(this IList<IStrain> strains, StrainAdapter adapter, IScorer scorer, int inc = 1, ReportGetScoresProgress reportProgress = null)
        {
            var numStrains = strains.Count;
            var scores = new ScoreTable();
            var total = (numStrains + 1) * numStrains / 2;
            var count = 0;
            for (var i = 0; i < numStrains; i++)
            {
                var strain1 = strains[i];
                for (var j = i; j < numStrains; j++)
                {
                    var strain2 = strains[j];
                    var score = adapter.Score(strain1, strain2, scorer, inc);
                    if (score > 0)
                    {
                        scores[strain1, strain2] = score;
                    }
                    reportProgress?.Invoke(++count, total);
                }
            }
            return scores;
        }

        public static ScoreTable GetScoresParallel(this IList<IStrain> strains, StrainAdapter adapter, IScorer scorer, int inc = 1, ReportGetScoresProgress reportProgress = null)
        {
            return GetScoresParallelizer.Instance.GetScoresParallel(strains, adapter, scorer, inc, reportProgress);
        }
    }
}
