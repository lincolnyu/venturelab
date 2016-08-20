using System;
using System.Collections.Generic;
using System.Linq;
using VentureLab.QbGaussianMethod.Helpers;

namespace VentureLab.QbGaussianMethod.Cores
{
    public class GaussianRegulatedCore : IWeightedCore, IPointWrapper
    {
        private GaussianRegulatedCoreVariables _variables;

        public GaussianRegulatedCore(IPoint point, GaussianRegulatedCoreConstants constants, GaussianRegulatedCoreVariables variables, double weight = 1.0) 
        {
            Point = point;
            Constants = constants;
            Variables = variables;
            Weight = weight;
        }

        #region IPointWrapper members

        public IPoint Point { get; }

        #endregion

        public GaussianRegulatedCoreConstants Constants { get; set; }

        public GaussianRegulatedCoreVariables Variables { get { return _variables; }
            set { _variables = value; _variables.Core = this; }  }

        #region IWeightedCore members

        #region ICore members

        public IList<double> L => Variables.L;

        #endregion

        public double Weight { get; set; }

        #endregion

        public double S => Weight * Variables.Normalizer;

        #region ICore members

        public double A(IList<double> x)
        {
            return E(x, Constants.N);
        }

        public double B(IList<double> x)
        {
            return S * E(x, Constants.M);
        }

        #endregion

        public double E(IList<double> x, double t)
        {
            var sum = 0.0;
            for (var i = 0; i < Point.InputLength; i++)
            {
                var d = x[i] - Point.Input[i];
                var dd = d * d;
                dd *= Variables.K[i];
                sum += dd;
            }
            return Math.Exp(t * sum);
        }

        public delegate void UpdateCoefficients(int index, double coeff);
        public delegate void UpdateNormalizer(double normalizer);

        /// <summary>
        ///  Use equi-distal method to set the cores
        /// </summary>
        /// <param name="cores">The cores</param>
        public static void SetCoreVariables(IEnumerable<GaussianRegulatedCore> cores, IEnumerable<GaussianRegulatedCoreVariables> coreVars, double dropThr = 0.6)
        {
            var firstCore = cores.First();
            var outputLen = firstCore.Point.OutputLength;
            var inputLen = firstCore.Point.InputLength;

            var inputDistances = new double[inputLen];
            var outputDistances = new double[outputLen];
            CoreGeometryHelper.InitInputOutputDistances(inputDistances, outputDistances);
            var coresDiscending = cores.OrderByDescending(x => x.Weight);
            var first = coresDiscending.First();
            var primaryCores = coresDiscending.TakeWhile(x => x.Weight >= dropThr * first.Weight).Select(x=>x.Point).ToList();
            primaryCores.GetMeanCompsMinDistance(inputDistances, outputDistances);

            var m = firstCore.Constants.M; // we assume M's are the same
            var n = firstCore.Constants.N;
            var coreCountFactor = Math.Log(2.0);
                        
            // output
            for (var i = 0; i < outputLen; i++)
            {
                var outputSpan = outputDistances[i] / 2;
                var l = -coreCountFactor / (outputSpan * outputSpan);
                foreach (var cv in coreVars)
                {
                    cv.L[i] = l;
                }
            }

            // input
            for (var i = 0; i < inputLen; i++)
            {
                var inputSpan = inputDistances[i] / 2;
                var k = -coreCountFactor / ((m - 0.5 * n) * inputSpan * inputSpan);
                foreach (var cv in coreVars)
                {
                    cv.K[i] = k;
                }
            }

            foreach (var cv in coreVars)
            {
                cv.UpdateLp();
                // This method makes all cores have exactly the same parameters
                cv.Normalizer = 1;
            }
        }
    }
}
