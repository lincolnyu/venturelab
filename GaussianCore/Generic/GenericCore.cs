using System;
using System.Collections.Generic;

namespace GaussianCore.Generic
{
    public abstract class GenericCore
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

        public double[] CentersInput { get; }

        public double[] CentersOutput { get; }

        public int InputLength => CentersInput.Length;

        public int OutputLength => CentersOutput.Length;

        public double[] L { get; set; }

        public double Multiple { get; set; } = 1;

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
            for (var i = 0; i < CentersOutput.Length; i++)
            {
                var d = outputs[i] - CentersOutput[i];
                d *= d;
                d *= L[i];
                s += d;
            }
            return Multiple * a * Math.Exp(b * s);
        }

        public void UpdateInvLCoeff()
        {
            var result = 1.0;
            foreach (var l in L)
            {
                result *= l;
            }
            if (result < 0) result = -result;
            InvLCoeff = 1 / Math.Sqrt(result);
        }

        #endregion
    }
}
