using System;
using System.Collections.Generic;
using System.Linq;
using VentureVisualization.Samples;

namespace VentureVisualization.SequencePlotting
{
    public class StockAndPredictionSequencer : StockSequencer
    {
        public StockAndPredictionSequencer(List<RecordSample> data, IList<PredictionSample> prediction) : base(data)
        {
            Prediction = prediction;
        }

        IList<PredictionSample> Prediction { get; }

        public new IEnumerable<ISample> GetStocksStarting(int index)
        {
            var bres = base.GetStocksStarting(index);
            return bres.Concat(Prediction);
        }

        public new IEnumerable<ISample> GetStocksAtTimes(IEnumerable<DateTime> times)
        {
            var bres = base.GetStocksAtTimes(times);
            foreach (var s in bres)
            {
                yield return s;
                if (s == Data[Data.Count - 1]) break;
            }
            foreach (var p in Prediction)
            {
                yield return p;
            }
        }
    }
}
