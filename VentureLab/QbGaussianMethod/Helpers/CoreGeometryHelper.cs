using System;
using System.Collections.Generic;
using VentureLab.QbGaussianMethod.Cores;

namespace VentureLab.QbGaussianMethod.Helpers
{
    public static class CoreGeometryHelper
    {
        public static void InitInputOutputDistances(IList<double> initedInputDistances, IList<double> initedOutputDistances)
        {
            for (var k = 0; k < initedInputDistances.Count; k++)
            {
                initedInputDistances[k] = 0;
            }
            for (var k = 0; k < initedOutputDistances.Count; k++)
            {
                initedOutputDistances[k] = 0;
            }
        }

        public static void GetMeanCompsMinDistance(this IList<IPoint> cores, IList<double> initedInputDistances, IList<double> initedOutputDistances)
        {
            var count = 0;
            for (var i = 0; i < cores.Count - 1; i++)
            {
                double minD = double.MaxValue;
                int mindj = 0;
                for (var j = i + 1; j < cores.Count;j++)
                {
                    var d =cores[i].SquareDistance(cores[j]);
                    if (d < minD)
                    {
                        minD = d;
                        mindj = j;
                    }
                }
                if (mindj > 0)
                {
                    for (var k = 0; k < initedInputDistances.Count; k++)
                    {
                        var d = Math.Abs(cores[i].Input[k] - cores[mindj].Input[k]);
                        initedInputDistances[k] += d;
                    }
                    for (var k = 0; k < initedOutputDistances.Count; k++)
                    {
                        var d = Math.Abs(cores[i].Output[k] - cores[mindj].Output[k]);
                        initedOutputDistances[k] += d;
                    }
                    count++;
                }
            }
            for (var k = 0; k < initedInputDistances.Count; k++)
            {
                initedInputDistances[k] /= count;
            }
            for (var k = 0; k < initedOutputDistances.Count; k++)
            {
                initedOutputDistances[k] /= count;
            }
        }

        public static void GetMaxCompsMinDistance(this IList<IPoint> cores, IList<double> initedInputDistances, IList<double> initedOutputDistances)
        {
            for (var i = 0; i < cores.Count - 1; i++)
            {
                double minD = double.MaxValue;
                int mindj = 0;
                for (var j = i + 1; j < cores.Count; j++)
                {
                    var d = cores[i].SquareDistance(cores[j]);
                    if (d < minD)
                    {
                        minD = d;
                        mindj = j;
                    }
                }
                if (mindj > 0)
                {
                    for (var k = 0; k < initedInputDistances.Count; k++)
                    {
                        var d = Math.Abs(cores[i].Input[k] - cores[mindj].Input[k]);
                        if (initedInputDistances[k] <  d)
                        {
                            initedInputDistances[k] = d;
                        }
                    }
                    for (var k = 0; k < initedOutputDistances.Count; k++)
                    {
                        var d = Math.Abs(cores[i].Output[k] - cores[mindj].Output[k]);
                        if (initedOutputDistances[k] < d)
                        {
                            initedOutputDistances[k] = d;
                        }
                    }
                }
            }
        }
    }
}
