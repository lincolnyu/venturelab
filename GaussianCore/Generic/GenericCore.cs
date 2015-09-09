using System;
using System.Collections.Generic;

namespace GaussianCore.Generic
{
    public abstract class GenericCore : ICore
    {
        #region Constructors

        protected GenericCore(int inputLen, int outputLen)
        {
            CentersInput = new double[inputLen];
            CentersOutput = new double[outputLen];
            L = new double[outputLen];
        }

        #endregion

        #region Properties

        #region ICore members


        public IList<double> CentersInput
        {
            get;
        }

        public IList<double> CentersOutput
        {
            get;
        }

        #endregion

        public int InputLength => CentersInput.Count;

        public int OutputLength => CentersOutput.Count;

        /// <summary>
        ///  Output precision coeffs
        /// </summary>
        public double[] L { get; set; }

        public double Weight { get; set; } = 1;

        public double InvLCoeff { get; set; }

        #endregion

        #region Methods

        public abstract double A(IList<double> inputs);

        public abstract double B(IList<double> inputs);

        public double GetIntensity(IList<double> inputs, IList<double> outputs)
        {
            var a = A(inputs);
            var b = B(inputs);
            var s = 0.0;
            for (var i = 0; i < CentersOutput.Count; i++)
            {
                var d = outputs[i] - CentersOutput[i];
                d *= d;
                d *= L[i];
                s += d;
            }
            return Weight * a * Math.Exp(b * s);
        }

        public void UpdateInvLCoeff()
        {
            var invl = 1.0;
            foreach (var l in L)
            {
                invl *= l;
            }
            if (invl < 0) invl = -invl;
            InvLCoeff = 1 / Math.Sqrt(invl);
        }

        #endregion
    }
}
