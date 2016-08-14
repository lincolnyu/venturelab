using System;
using System.Collections.Generic;

namespace VentureLab.QbClustering
{
    public abstract class StrainAdapter
    {
        public void AddPoint(IStrain strain, IStrainPoint point, bool keepSorted = true)
        {
            if (!keepSorted)
            {
                strain.Points.Add(point);
            }
            else
            {
                var index = strain.Points.BinarySearch(point);
            }
        }

        public virtual double Score(IStrain a, IStrain b, IScorer scorer)
        {
            int start = 0;
            var totalScore = 0.0;
            foreach (var pa in a.Points)
            {
                var i = start;
                for (; i < b.Points.Count; i++)
                {
                    var pb = b.Points[i];
                    var diffInd = Math.Abs(pa.Indicator - pb.Indicator);
                    if (diffInd <= scorer.InputDiffTolerance)
                    {
                        start = i;
                        break;
                    }
                }
                for (; i < b.Points.Count; i++)
                {
                    var pb = b.Points[i];
                    var diffInd = Math.Abs(pa.Indicator - pb.Indicator);
                    if (diffInd > scorer.InputDiffTolerance)
                    {
                        break;
                    }
                    var inputDiff = GetInputDiff(pa, pb);
                    if (inputDiff > scorer.InputDiffTolerance)
                    {
                        continue;
                    }
                    var outputDiff = GetOutputDiff(pa, pb);
                    var s = scorer.Score(inputDiff, outputDiff);
                    totalScore += s;
                }
            }
            var minPoints = Math.Min(a.Points.Count, b.Points.Count);
            return minPoints > 0 ? totalScore / minPoints : 0;
        }

        public abstract double GetPointIndicator(IStrainPoint point);
        public abstract double GetInputDiff(IStrainPoint pthis, IStrainPoint pthat);
        public abstract double GetOutputDiff(IStrainPoint pthis, IStrainPoint pthat);

        public void ReorderPoints(IStrain strain)
        {
            strain.Points.Sort();
        }

        public void UpdatePointsIndicators(IEnumerable<IStrainPoint> points)
        {
            foreach (var point in points)
            {
                point.Indicator = GetPointIndicator(point);
            }
        }

        public void UpdatePointsIndicators(IStrain strain)
        {
            UpdatePointsIndicators(strain.Points);
        }
    }
}
