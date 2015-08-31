using System;
using System.Collections.Generic;

namespace GaussianCore.Generic
{
    public class GaussianConfinedCore : GenericCore
    {
        #region Constructors

        public GaussianConfinedCore(int inputLen, int outputLen) : base(inputLen, outputLen)
        {
            K = new double[inputLen];
        }

        #endregion

        #region Properties

        /// <summary>
        ///  Input precision coefficients
        /// </summary>
        public double[] K { get; }

        public int Alpha { get; internal set; } = 5;

        #endregion

        #region Methods

        #region GenericCore

        public override double A(IList<double> inputs)
        {
            var a = C(inputs);
            var l = 1.0;
            for (var i = 0; i < L.Length; i++)
            {
                l *= L[i];
            }
            l = Math.Sqrt(Math.Abs(l));
            a *= l; // to counteract the output coefficients
            return a;
        }

        public override double B(IList<double> inputs)
        {
            var c = C(inputs);
            return Math.Pow(c, 1.0 / (Alpha * OutputLength));
        }

        #endregion

        private double C(IList<double> inputs)
        {
            var s = 0.0;
            for (var i = 0; i < K.Length; i++)
            {
                var d = inputs[i] - CentersInput[i];
                d *= d;
                d *= K[i];
                s += d;
            }
            var c = Math.Exp(s);
            return c;
        }

        #endregion
    }
}
