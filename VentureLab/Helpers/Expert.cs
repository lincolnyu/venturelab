﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VentureLab.Asx;
using VentureLab.Prediction;
using static VentureLab.Asx.StockManager;
using static VentureLab.QbGaussianMethod.Helpers.PredictionCommon;

namespace VentureLab.Helpers
{
    public static class Expert
    {
        public delegate bool ReportExpertProgress(Result result, int done, int total);

        public class Result : PredictionResult
        {
            public Result(int outLen) : base(outLen)
            {
            }

            public StockItem Item { get; set; }
            public double Score { get; private set; }

            public void UpdateScore()
            {
                // TODO incorporate strength, length and data
                //      sophistication
                var a = 1;
                var b = 1;
                Score = 0;
                for (var i = 0; i < Y.Count; i++)
                {
                    var currScore = GetScore(Y[i], YY[i]);
                    Score += currScore * a;
                    var c = a;
                    a += b;
                    b = c;
                }
            }

            public static double GetScore(double y, double yy)
            {
                var stdyi = StatisticsHelper.GetStandardVariance(y, yy);
                var max = y + stdyi;
                var min = y - stdyi;
                var score = min + (max - min) / Math.E;
                return score;
            }
        }
        
        public static List<Result> Run(StockManager stockManager, IPointManager pointManager, PredictDelegate predict, GetItemIndexCallback giicb, int maxDaysAheadAllowed, ReportExpertProgress progress)
        {
            var items = stockManager.Items;
            var outLen = stockManager.GetOutputLength();
            var results = new List<Result>();
            var count = 0;
            var latestDate = DateTime.MinValue;
            foreach (var item in items)
            {
                var result = new Result(outLen) { Item = item.Value };
                predict(stockManager, pointManager, item.Value, giicb, result);
                if (result.ErrorMessage == null)
                {
                    result.UpdateScore();
                    results.Add(result);
                    if (result.Date > latestDate)
                    {
                        latestDate = result.Date;
                    }
                }
                if (!progress(result, ++count, items.Count))
                {
                    break;
                }
            }
            RemoveResultsTooAhead(results, latestDate, maxDaysAheadAllowed);
            results.Sort((a, b) => -a.Score.CompareTo(b.Score));
            return results;
        }

        public static List<Result> RunParallel(StockManager stockManager, IPointManagerFactory pmFactory, PredictDelegate predict, GetItemIndexCallback giicb, int maxDaysAheadAllowed, ReportExpertProgress progress, int maxDegreeOfParallelism = int.MaxValue)
        {
            var items = stockManager.Items;
            var outLen = stockManager.GetOutputLength();
            var results = new List<Result>();
            var count = 0;
            var latestDate = DateTime.MinValue;
            var options = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
            Parallel.ForEach(items, options, (item, loopState) =>
            {
                var result = new Result(outLen) { Item = item.Value };
                var pointManager = pmFactory.Create();
                predict(stockManager, pointManager, item.Value, giicb, result);
                if (result.ErrorMessage == null)
                {
                    result.UpdateScore();
                }
                lock (results)
                {
                    if (result.ErrorMessage == null)
                    {
                        if (result.Date > latestDate)
                        {
                            latestDate = result.Date;
                        }
                        // TODO is List<> thread-safe?
                        results.Add(result);
                    }
                    if (!progress(result, ++count, items.Count))
                    {
                        loopState.Break();
                    }
                }
            });
            RemoveResultsTooAhead(results, latestDate, maxDaysAheadAllowed);
            results.Sort((a, b) => -a.Score.CompareTo(b.Score));
            return results;
        }

        private static void RemoveResultsTooAhead(IList<Result> results, DateTime latestDate, int maxDaysAheadAllowed)
        {
            var tsMaxDaysAhead = TimeSpan.FromDays(maxDaysAheadAllowed);
            var earliestAllowed = latestDate.Subtract(tsMaxDaysAhead);
            for (var i = results.Count - 1; i >= 0; i--)
            {
                var result = results[i];
                if (result.Date < earliestAllowed)
                {
                    results.RemoveAt(i);
                }
            }
        }
    }
}
