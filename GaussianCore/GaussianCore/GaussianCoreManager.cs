using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GaussianCore
{
    public abstract class GaussianCoreManager : ICoreManager
    {
        #region Properties
        public int OutputStartingFrom { get; set; }

        #endregion

        public abstract IEnumerator<Core> GetEnumerator();
        

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public double GetIntensity(IList<double> inoutputs)
        {
            return this.Sum(core => core.GetIntensity(inoutputs));
        }

        public double GetExpectedY(IList<double> inputs, int k)
        {
            var res = 0.0;
            var denom = 0.0;
            foreach (var core in this)
            {
                var p = core.GetLeadingIntensity(inputs);

                denom += p;
                p *= core.Components[k + OutputStartingFrom].Center;

                res += p;
            }
            res /= denom;
            return res;
        }

        public double GetExpectedSquareY(IList<double> inputs, int k)
        {
            var res = 0.0;
            var denom = 0.0;
            foreach (var core in this)
            {
                var p = core.GetLeadingIntensity(inputs);

                denom += p;
                var comp = core.Components[k + OutputStartingFrom];
                var d = comp.Center;
                d *= d;
                d -= 0.5 / comp.L;
                p *= d;
                res += p;
            }
            res /= denom;
            return res;
        }
    }
}
