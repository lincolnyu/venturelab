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

        /// <summary>
        ///  centers of components of input
        /// </summary>
        public IList<double> CentersInput { get; }

        /// <summary>
        ///  centers of components of output
        /// </summary>
        public IList<double> CentersOutput { get; }


        #endregion

        public int InputLength => CentersInput.Count;

        public int OutputLength => CentersOutput.Count;

        /// <summary>
        ///  Output precision coeffs
        /// </summary>
        public double[] L { get; set; }

        /// <summary>
        ///  1/Sqrt(Abs(L0*L1*...))
        /// </summary>
        public double InvLCoeff { get; private set; }

        public abstract double Weight { get; set; }

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
            // note weight is provided by A(x)
            return a * Math.Exp(b * s);
        }

        public void UpdateInvLCoeff()
        {
            var invl = 1.0;
            foreach (var l in L)
            {
                invl *= l;
            }
            if (invl < 0)
            {
                invl = -invl;
            }
            InvLCoeff = 1 / Math.Sqrt(invl);
        }

        #endregion
    }
}
