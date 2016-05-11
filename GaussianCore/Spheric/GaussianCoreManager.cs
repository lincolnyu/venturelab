using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GaussianCore.Spheric
{
    public abstract class GaussianCoreManager : ICoreManager, IEnumerable<Core>
    {
        #region Properties

        public int OutputStartingFrom { get; set; }

        #endregion

        #region Methods

        #region IEnumerable<Core> members

        public abstract IEnumerator<Core> GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public double GetIntensity(IList<double> inputs, IList<double> outputs)
        {
            return this.Sum(core => core.GetIntensity(inputs, outputs));
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

        #endregion
    }
}
