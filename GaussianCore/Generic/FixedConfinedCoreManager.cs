using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GaussianCore.Generic
{
    public class FixedConfinedCoreManager : GenericCoreManager
    {
        #region Properties

        #region Methods

        // TODO Review the EPSILON mechanism
        public override double EpsilonY => double.Epsilon;

        public override double EpsilonSquareY => double.Epsilon;

        #endregion

        public List<GaussianConfinedCore> Cores { get; } = new List<GaussianConfinedCore>();

        public const double Attenuation = 0.5;

        #endregion

        #region Methods

        #region GenericCoreManager members

        public override IEnumerator<GenericCore> GetEnumerator()
        {
            return Cores.Cast<GenericCore>().GetEnumerator();
        }

        #endregion

        public void UpdateCoreCoeffs()
        {
            double[] sqdilist;
            double[] sqdolist;
            GetMaxMinSquareDistance(out sqdilist, out sqdolist);
            const double relax = 4.0;// empirical? 4.0 for theoreticallydropping to half at halfway to the nearest
            Parallel.ForEach(Cores, core =>
            {
                for (var k = 0; k < core.InputLength; k++)
                {
                    core.K[k] = Math.Log(Attenuation) * relax * 0.5 / sqdilist[k];
                }
                for (var k = 0; k < core.OutputLength; k++)
                {
                    core.L[k] = Math.Log(Attenuation) * relax / sqdolist[k];
                }
                core.UpdateInvLCoeff();
            });
        }

        private void GetMaxMinSquareDistance(out double[] sqdilist, out double[] sqdolist)
        {
            var sqditable = new double[Cores.Count][];
            var sqdotable = new double[Cores.Count][];
            var inputLen = Cores[0].InputLength;
            var outputLen = Cores[0].OutputLength;
            Parallel.For(0, Cores.Count, i =>
            {
                sqditable[i] = new double[inputLen];
                sqdotable[i] = new double[outputLen];
                for (var k = 0; k < inputLen; k++)
                {
                    sqditable[i][k] = double.MaxValue;
                }
                for (var k = 0; k < outputLen; k++)
                {
                    sqdotable[i][k] = double.MaxValue;
                }
            });

            Parallel.For(0, Cores.Count - 1, i =>
              {
                  var c1 = Cores[i];
                  for (var j = i + 1; j < Cores.Count; j++)
                  {
                      var c2 = Cores[j];
                      for (var k = 0; k < inputLen; k++)
                      {
                          var x1 = c1.CentersInput[k];
                          var x2 = c2.CentersInput[k];
                          var sqd = (x2 - x1) * (x2 - x1);
                          if (sqd < sqditable[i][k])
                          {
                              sqditable[i][k] = sqd;
                          }
                          if (sqd < sqditable[j][k])
                          {
                              sqditable[j][k] = sqd;
                          }
                      }

                      for (var k = 0; k < outputLen; k++)
                      {
                          var x1 = c1.CentersOutput[k];
                          var x2 = c2.CentersOutput[k];
                          var sqd = (x2 - x1) * (x2 - x1);
                          if (sqd < sqdotable[i][k])
                          {
                              sqdotable[i][k] = sqd;
                          }
                          if (sqd < sqdotable[j][k])
                          {
                              sqdotable[j][k] = sqd;
                          }
                      }
                  }
              });

            sqdilist = new double[inputLen];
            for (var k = 0; k < inputLen; k++)
            {
                var max = 0.0;
                for (var i = 0; i < Cores.Count; i++)
                {
                    if (sqditable[i][k] > max)
                    {
                        max = sqditable[i][k];
                    }
                }
                sqdilist[k] = max;
            }

            sqdolist = new double[outputLen];
            for (var k = 0; k < outputLen; k++)
            {
                var max = 0.0;
                for (var i = 0; i < Cores.Count; i++)
                {
                    if (sqdotable[i][k] > max)
                    {
                        max = sqdotable[i][k];
                    }
                }
                sqdolist[k] = max;
            }
        }

        #endregion
    }
}

