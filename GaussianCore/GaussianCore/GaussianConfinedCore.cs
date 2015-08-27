using System;
using System.Collections.Generic;

namespace GaussianCore
{
    public class GaussianConfinedCore : GenericCore
    {
        #region Properties

        /// <summary>
        ///  Input precision coefficients
        /// </summary>
        public double[] K { get; set; }

        #endregion

        #region Methods

        #region GenericCore

        public override double A(IList<double> inputs)
        {
            var b = B(inputs);
            var a = Math.Pow(b, OutputLength);
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
            var s = 0.0;
            for (var i = 0; i< K.Length; i++)
            {
                var d = inputs[i] - K[i];
                d *= d;
                s += d;
            }
            var b = Math.Exp(s);
            return b;
        }

        #endregion

        #endregion
    }
}
