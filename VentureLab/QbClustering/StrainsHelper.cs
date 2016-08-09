using System.Collections.Generic;
using System.Threading.Tasks;

namespace VentureLab.QbClustering
{
    public static class StrainsHelper
    {
        public delegate void ReportGetScoresProgress(int done, int total);

        public static void UpdatePointsIndicators(this StrainAdapter adapter, IEnumerable<IStrain> strains)
        {
            foreach (var strain in strains)
            {
                adapter.UpdatePointsIndicators(strain.Points);
                adapter.ReorderPoints(strain);
            }
        }

        public static ScoreTable GetScores(this IList<IStrain> strains, StrainAdapter adapter, IScorer scorer, ReportGetScoresProgress reportProgress = null)
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
                    var score = adapter.Score(strain1, strain2, scorer);
                    scores[strain1, strain2] = score;
                    if (reportProgress != null)
                    {
                        reportProgress(++count, total);
                    }
                }
            }
            return scores;
        }

        public static ScoreTable GetScoresParallel(this IList<IStrain> strains, StrainAdapter adapter, IScorer scorer, ReportGetScoresProgress reportProgress = null)
        {
            var numStrains = strains.Count;
            var scores = new ScoreTable();
            var total = (numStrains + 1) * numStrains / 2;
            var count = 0;
            Parallel.For(0, numStrains, i=>
            {
                var strain1 = strains[i];
                Parallel.For(i, numStrains, j =>
                {
                    var strain2 = strains[j];
                    var score = adapter.Score(strain1, strain2, scorer);
                    lock(scores)
                    {
                        scores[strain1, strain2] = score;
                        reportProgress?.Invoke(++count, total);
                    }
                });
            });
            return scores;
        }
    }
}
