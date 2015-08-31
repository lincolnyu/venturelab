using System;
using System.Collections;
using System.Collections.Generic;

namespace GaussianCore.Generic
{
    public abstract class GenericCoreManager : ICoreManager, IEnumerable<GenericCore>
    {
        #region Properties

        public virtual double EpsilonY => double.Epsilon;

        public virtual double EpsilonSquareY => EpsilonY;

        #endregion

        #region Methods

        #region IEnumerable<GenericCore> members

        public abstract IEnumerator<GenericCore> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ICoreManager members

        public double GetIntensity(IList<double> inputs, IList<double> outputs)
        {
            var sum = 0.0;
            foreach (var core in this)
            {
                var i = core.GetIntensity(inputs, outputs);
                sum += i;
            }
            return sum;
        }

        public double GetExpectedY(IList<double> inputs, int k)
        {
            var denom = 0.0;
            var num = 0.0;
            foreach (var core in this)
            {
                var a = core.A(inputs) * core.Multiple;
                if (a >= EpsilonY)
                {
                    var b = core.B(inputs);
                    var bb = Math.Pow(b, -core.OutputLength / 2.0);
                    var prod = a * bb;
                    denom += prod;
                    num += prod * core.CentersOutput[k];
                }
            }
            return num / denom;
        }

        public double GetExpectedSquareY(IList<double> inputs, int k)
        {
            var denom = 0.0;
            var num = 0.0;
            foreach (var core in this)
            {
                var a = core.A(inputs) * core.Multiple;
                if (a >= EpsilonSquareY)
                {
                    var b = core.B(inputs);
                    var bb = Math.Pow(b, -core.OutputLength / 2.0);
                    var prod = a * bb;
                    denom += prod;
                    var d = core.CentersOutput[k];
                    d *= d;
                    d -= 0.5 / (core.L[k] * b);
                    num += prod * d;
                }
            }
            return num / denom;
        }

        #endregion

        #endregion
    }
}
