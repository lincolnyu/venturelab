using System;
using System.Collections.Generic;

namespace GaussianCore
{
    public abstract class GenericCore
    {
        public double[] CentersInput { get; set; }

        public double[] CentersOutput { get; set; }

        public IList<double> L { get; set; }

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
            return a * Math.Exp(b * s);
        }
    }
}
