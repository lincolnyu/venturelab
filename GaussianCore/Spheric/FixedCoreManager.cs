using System;
using System.Collections.Generic;

namespace GaussianCore.Spheric
{
    public class FixedCoreManager : GaussianCoreManager
    {
        #region Properties

        public IList<Core> Cores { get; } = new List<Core>();

        #endregion

        #region Methods

        public override IEnumerator<Core> GetEnumerator()
        {
            return Cores.GetEnumerator();
        }

        private double[] GetMeanSquareDistance()
        {
            var componentSize = Cores[0].Components.Count;
            var sqdlist = new double[componentSize];
            for (var i = 0; i < Cores.Count - 1; i++)
            {
                var c1 = Cores[i];
                for (var j = i + 1; j < Cores.Count; j++)
                {
                    var c2 = Cores[j];
                    for (var k = 0; k < componentSize; k++)
                    {
                        var x1 = c1.Components[k].Center;
                        var x2 = c2.Components[k].Center;
                        sqdlist[k] = (x2 - x1) * (x2 - x1);
                    }
                }
            }
            for (var k = 0; k < componentSize; k++)
            {
                sqdlist[k] /= Cores.Count*Cores.Count;
            }
            return sqdlist;
        }

        private double[] GetMaxMinSquareDistance()
        {
            var componentSize = Cores[0].Components.Count;
            var sqdtable = new double[Cores.Count][];
            for (var i = 0; i < Cores.Count; i++)
            {
                sqdtable[i] = new double[componentSize];
                for (var k = 0; k < componentSize; k++)
                {
                    sqdtable[i][k] = double.MaxValue;
                }
            }
            for (var i = 0; i < Cores.Count - 1; i++)
            {
                var c1 = Cores[i];
                for (var j = i + 1; j < Cores.Count; j++)
                {
                    var c2 = Cores[j];
                    for (var k = 0; k < componentSize; k++)
                    {
                        var x1 = c1.Components[k].Center;
                        var x2 = c2.Components[k].Center;
                        var sqd = (x2 - x1) * (x2 - x1);
                        if (sqd < sqdtable[i][k])
                        {
                            sqdtable[i][k] = sqd;
                        }
                        if (sqd < sqdtable[j][k])
                        {
                            sqdtable[j][k] = sqd;
                        }
                    }
                }
            }
            var sqdlist = new double[componentSize];
            for (var k = 0; k < componentSize; k++)
            {
                var max = 0.0;
                for (var i = 0; i < Cores.Count; i++)
                {
                    if (sqdtable[i][k] > max)
                    {
                        max = sqdtable[i][k];
                    }

                }
                sqdlist[k] = max;
            }

            return sqdlist;
        }

        public void UpdateCoreCoeffs()
        {
            var componentSize = Cores[0].Components.Count;
            var sqdlist = GetMaxMinSquareDistance();

            var llist = new double[componentSize];
            var invPi = 1 / Math.PI;
            var ainput = Math.Pow(invPi, OutputStartingFrom/2.0);
            var outputCount = componentSize - OutputStartingFrom;
            var aoutput = Math.Pow(invPi, outputCount/2.0);
            for (var k = 0; k < componentSize; k++)
            {
                llist[k] = Math.Log(Core.Attenuation) * 4 / sqdlist[k];
                if (k < OutputStartingFrom)
                {
                    ainput *= -llist[k];
                }
                else
                {
                    aoutput *= -llist[k];
                }
            }

            foreach (var core in Cores)
            {
                for (var k = 0; k < componentSize; k++)
                {
                    core.Components[k].L = llist[k];
                }
                core.Weight = 1;
                core.AInput = ainput;
                core.AOutput = aoutput;
            }
        }

        #endregion
    }
}
