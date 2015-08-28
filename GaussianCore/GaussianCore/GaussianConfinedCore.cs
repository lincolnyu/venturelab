using System;
using System.Collections.Generic;

namespace GaussianCore
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

        #endregion

        #region Methods

        #region GenericCore

        public override double A(IList<double> inputs)
        {
            var c = C(inputs);
            var a = Math.Pow(c, OutputLength);
            var l = 1.0;
            for (var i = 0; i < L.Length; i++)
            {
                l *= L[i];
            }
            l = Math.Sqrt(Math.Abs(l));
            a *= l;
            return a;
        }

        public override double B(IList<double> inputs)
        {
            var c = C(inputs);
            return Math.Pow(c, 1.0/OutputLength);
        }

        #endregion

        private double C(IList<double> inputs)
        {
            var s = 0.0;
            for (var i = 0; i < K.Length; i++)
            {
                var d = inputs[i] - K[i];
                d *= d;
                d *= L[i];
                s += d;
            }
            var c = Math.Exp(s);
            return c;
        }

        #endregion
    }
}
