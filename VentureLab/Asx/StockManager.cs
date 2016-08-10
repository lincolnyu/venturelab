using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VentureLab.Helpers;
using VentureLab.QbClustering;
using VentureLab.QbGaussianMethod.Cores;
using static VentureLab.QbClustering.StrainsHelper;

namespace VentureLab.Asx
{
    public class StockManager
    {
        public delegate ScoreTable GetScoresMethod(IList<IStrain> strains, StrainAdapter adapter, IScorer scorer, ReportGetScoresProgress reportProgress = null);

        public delegate void LoadStrainCallback(IPointFactory pointFactory, StockItem stockItem);

        public class StockItem : Strain
        {
            public StockItem(Stock stock)
            {
                Stock = stock;
            }

            public Stock Stock { get; }

            public Dictionary<StockItem, double> Weights = new Dictionary<StockItem, double>();

            public SampleAccessor SampleInput(IPointFactory pointFactory, int index)
            {
                var sa = pointFactory.CreateSampleAccessor();
                sa.SampleOneInput(Stock.Data, index);
                return sa;
            }

            public void LoadStrain(IPointFactory pointFactory, int interval = 1)
            {
                int firstIndex, lastIndex;
                AsxSamplingHelper.GetStartAndEnd(Stock.Data.Count, out firstIndex, out lastIndex);
                Points = pointFactory.Sample(Stock.Data, firstIndex, lastIndex, interval).ToList();
            }

            public void LoadStrain(IPointFactory pointFactory, DateTime start, DateTime end, int interval = 1)
            {
                int firstIndex, lastIndex;
                AsxSamplingHelper.GetStartAndEnd(Stock.Data.Count, out firstIndex, out lastIndex);
                var startIndex = Stock.GetInsertIndex(start);
                var endIndex = Stock.GetInsertIndex(end);
                startIndex = Math.Max(firstIndex, startIndex);
                endIndex = Math.Min(lastIndex, endIndex);
                if (startIndex < endIndex)
                {
                    Points = pointFactory.Sample(Stock.Data, startIndex, endIndex, interval).ToList();
                }
            }

            public void LoadStrain(IPointFactory pointFactory, int startToFirst, int count, int interval = 1)
            {
                int firstIndex, lastIndex;
                AsxSamplingHelper.GetStartAndEnd(Stock.Data.Count, out firstIndex, out lastIndex);
                var startIndex = firstIndex + startToFirst;
                var endIndex = startIndex + (count - 1) * interval + 1;
                endIndex = Math.Min(lastIndex, endIndex);
                if (startIndex < endIndex)
                {
                    Points = pointFactory.Sample(Stock.Data, startIndex, endIndex, interval).ToList();
                }
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
                Items.Add(code, stockItem);
            }
            return stockItem.Stock.Add(de);
        }

        public void ReloadStrains(IPointFactory pointFactory) => ReloadStrains(pointFactory, DefaultLoadStrainCallback);

        public void ReloadStrainsParallel(IPointFactory pointFactory) => ReloadStrainsParallel(pointFactory, DefaultLoadStrainCallback);

        public void ReloadStrains(IPointFactory pointFactory, LoadStrainCallback lscb) 
        {
            foreach (var strain in Items.Values)
            {
                lscb(pointFactory, strain);
            }
        }

        public void ReloadStrainsParallel(IPointFactory pointFactory, LoadStrainCallback lscb) => Parallel.ForEach(Items.Values, strain =>
           lscb(pointFactory, strain));

        public static void DefaultLoadStrainCallback(IPointFactory pointFactory, StockItem stockItem) => stockItem.LoadStrain(pointFactory);

        public ScoreTable GetScoreTable(StrainAdapter adapter, IScorer scorer, ReportGetScoresProgress reportProgress = null)
        {
            var scoreTable = GetScoreTable(adapter, scorer, StrainsHelper.GetScores, reportProgress);
            return scoreTable;
        }

        public ScoreTable GetScoreTableParallel(StrainAdapter adapter, IScorer scorer, ReportGetScoresProgress reportProgress = null)
        {
            var scoreTable = GetScoreTable(adapter, scorer, StrainsHelper.GetScoresParallel, reportProgress);
            return scoreTable;
        }

        private ScoreTable GetScoreTable(StrainAdapter adapter, IScorer scorer, GetScoresMethod getScores, ReportGetScoresProgress reportProgress)
        {
            var strainList = Items.Values.Cast<IStrain>().ToList();
            adapter.UpdatePointsIndicators(strainList);
            var scoreTable = getScores(strainList, adapter, scorer, reportProgress);
            return scoreTable;
        }

        public void SetScoreTableToItems(ScoreTable scoreTable)
        {
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
