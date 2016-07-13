using System.Collections.Generic;
using System.Threading.Tasks;

namespace VentureLab.QbClustering
{
    public static class StrainsHelper
    {
        public static void UpdatePointsIndicators(this StrainAdapter adapter, IEnumerable<IStrain> strains)
        {
            foreach (var strain in strains)
            {
                adapter.UpdatePointsIndicators(strain.Points);
                adapter.ReorderPoints(strain);
            }
        }

        public static ScoreTable GetScores(this IList<IStrain> strains, StrainAdapter adapter, IScorer scorer)
        {
            var numStrains = strains.Count;
            var scores = new ScoreTable();
            for (var i = 0; i < numStrains; i++)
            {
                var strain1 = strains[i];
                for (var j = i; j < numStrains; j++)
                {
                    var strain2 = strains[j];
                    var score = adapter.Score(strain1, strain2, scorer);
                    scores[strain1, strain2] = score;
                }
            }
            return scores;
        }

        public static ScoreTable GetScoresParallel(this IList<IStrain> strains, StrainAdapter adapter, IScorer scorer)
        {
            var numStrains = strains.Count;
            var scores = new ScoreTable();
            Parallel.For(0, numStrains, i=>
            {
                var strain1 = strains[i];
                Parallel.For(i, numStrains, j =>
                {
                    var strain2 = strains[j];
                    var score = adapter.Score(strain1, strain2, scorer);
                    scores[strain1, strain2] = score;
                });
            });
            return scores;
        }
    }
}
