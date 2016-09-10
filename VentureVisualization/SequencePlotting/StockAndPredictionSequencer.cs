using System;
using System.Collections.Generic;
using System.Linq;
using VentureVisualization.Samples;

namespace VentureVisualization.SequencePlotting
{
    public class StockAndPredictionSequencer : StockSequencer
    {
        private double _totalDataLength;

        public StockAndPredictionSequencer(List<RecordSample> records, IList<PredictionSample> prediction) : base(records)
        {
            Prediction = prediction;
            UpdateTotalDataLength();
        }

        IList<PredictionSample> Prediction { get; }

        public override double TotalDataLength => _totalDataLength;

        public override IEnumerable<ISample> GetStocksStarting(int index)
        {
            var excess = index - Records.Count + 1;
            if (excess > 0)
            {
                return GetPurePrediction(excess);
            }
            else
            {
                var bres = base.GetStocksStarting(index);
                return bres.Concat(Prediction);
            }
        }

        private IEnumerable<ISample> GetPurePrediction(int excess)
        {
            var s = 0.0;
            var found = false;
            for (var i = 0; i < Prediction.Count; i++)
            {
                var p = Prediction[i];
                if (!found)
                {
                    s += p.Step;
                    if (s >= excess)
                    {
                        p.Offset = s - excess;
                    }
                }
                if (found)
                {
                    yield return p;
                }
            }
        }

        public override IEnumerable<ISample> GetStocksAtTimes(IEnumerable<DateTime> times)
        {
            var bres = base.GetStocksAtTimes(times);
            foreach (var s in bres)
            {
                yield return s;
                if (s == Records[Records.Count - 1]) break;
            }
            foreach (var p in Prediction)
            {
                yield return p;
            }
        }

        private void UpdateTotalDataLength()
        {
            _totalDataLength = base.TotalDataLength;
            _totalDataLength += Prediction.Sum(x => x.Step);
        }
    }
}
