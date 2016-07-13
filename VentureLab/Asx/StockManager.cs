using System.Collections.Generic;
using System.Linq;
using VentureLab.Helpers;
using VentureLab.QbClustering;
using VentureLab.QbGaussianMethod.Cores;

namespace VentureLab.Asx
{
    public class StockManager
    {
        public delegate ScoreTable GetScoresMethod(IList<IStrain> strains, StrainAdapter adapter, IScorer scorer);

        public class StockItem : Strain
        {
            public StockItem(Stock stock)
            {
                Stock = stock;
            }
            public Stock Stock { get; }

            public Dictionary<StockItem, double> Weights = new Dictionary<StockItem, double>();

            public void LoadStrain(IPointFactory pointFactory, int interval = 1)
            {
                int start, end;
                AsxSamplingHelper.GetStartAndEnd(Stock.Data.Count, out start, out end);
                Points = pointFactory.Sample(Stock.Data, start, end, interval).ToList();
            }
        }

        public Dictionary<string, StockItem> Items { get; } = new Dictionary<string, StockItem>();

        public bool Add(string code, DailyEntry de)
        {
            StockItem stockItem;
            if (!Items.TryGetValue(code, out stockItem))
            {
                var stock = new Stock(code);
                stockItem = new StockItem(stock);
            }
            return stockItem.Stock.Add(de);
        }

        public void UpdateScoreTable(StrainAdapter adapter, IScorer scorer)
        {
            UpdateScoreTable(adapter, scorer, StrainsHelper.GetScores);
        }

        public void UpdateScoreTableParallel(StrainAdapter adapter, IScorer scorer)
        {
            UpdateScoreTable(adapter, scorer, StrainsHelper.GetScoresParallel);
        }

        private void UpdateScoreTable(StrainAdapter adapter, IScorer scorer, GetScoresMethod getScores)
        {
            var strainList = Items.Values.Cast<IStrain>().ToList();
            adapter.UpdatePointsIndicators(strainList);
            var scoreTable = getScores(strainList, adapter, scorer);
            foreach (var item in Items.Values)
            {
                item.Weights.Clear();
            }
            foreach (var kvp in scoreTable.Data)
            {
                var item1 = (StockItem)kvp.Key.Strain1;
                var item2 = (StockItem)kvp.Key.Strain2;
                var score = scoreTable[item1, item2];
                item1.Weights[item2] = score;
                item2.Weights[item1] = score;
            }
        }

        /// <summary>
        ///  Prepares prediction for the given stock by returning all its 
        ///  weighted correlated points
        /// </summary>
        /// <param name="stock">The stock to predict</param>
        /// <returns>The related points</returns>
        public IEnumerable<IPoint> PreparePrediction(StockItem stock)
        {
            foreach (var kvp in stock.Weights)
            {
                var relitem = kvp.Key;
                var weight = kvp.Value;
                foreach (var p in relitem.Points.Cast<IWeightedCore>())
                {
                    p.Weight = weight;
                    yield return (IPoint)p;
                }
            }
        }
    }
}
