using System.Collections.Generic;
using System.Linq;

namespace GaussianCore.Spheric
{
    public class FixedCoreManager : GaussianCoreManager
    {
        #region Properties

        public IList<Core> Cores { get; } = new List<Core>();

        public const double DistanceTolerance = 0.0001;

        #endregion

        #region Methods

        #region GaussianCoreManager members
        public override IEnumerator<Core> GetEnumerator()
        {
            return Cores.GetEnumerator();
        }

        #endregion

        public void UpdateCoreCoeffs()
        {
            UpdateMinimumDistances();

            foreach (var c in Cores)
            {
                c.UpdateCoefficients(OutputStartingFrom);
            }
        }

        public void UpdateMinimumDistances()
        {
            foreach (var c in Cores)
            {
                c.MinimumInputSquareDistance = double.MaxValue;
                c.MinimumOutputSquareDistance = double.MaxValue;
            }
            var eliminated = new HashSet<int>();
            var listOfEliminated = new LinkedList<int>();
            var componentSize = Cores[0].Components.Count;
            for (var i = 0; i < Cores.Count-1; i++)
            {
                if (eliminated.Contains(i))
                {
                    listOfEliminated.AddLast(i);
                    continue;
                }
                var c1 = Cores[i];
                for (var j = i + 1; j < Cores.Count; j++)
                {
                    var c2 = Cores[j];
                    var d = c1.GetSquareDistanceEuclid(c2, 0, OutputStartingFrom);
                    if (d < DistanceTolerance)
                    {
                        // merge i and j by removing j
                        eliminated.Add(j);
                        c1.Multiple++;
                        continue;
                    }
                    IList<double> v = null;
                    double vds = 0;
                    if (d <= c2.MinimumInputSquareDistance)
                    {
                        v = c2.GetVector(c1, OutputStartingFrom, componentSize);
                        vds = v.Sum(x => x * x);

                        if (d < c2.MinimumInputSquareDistance || vds < c2.MinimumOutputSquareDistance)
                        {
                            c2.MinimumInputSquareDistance = d;
                            c2.OutputOffsets = v;
                            c2.MinimumOutputSquareDistance = vds;
                            c2.ClosestNeighbour = c1;
                        }
                    }
                    
                    if (d < c1.MinimumInputSquareDistance)
                    {
                        if (v != null)
                        {
                            v = v.Select(x => -x).ToList();
                        }
                        else
                        {
                            v = c1.GetVector(c2, OutputStartingFrom, componentSize);
                            vds = v.Sum(x => x * x);
                        }

                        if (d < c1.MinimumInputSquareDistance || vds < c1.MinimumOutputSquareDistance)
                        {
                            c1.MinimumInputSquareDistance = d;
                            c1.OutputOffsets = v;
                            c1.MinimumOutputSquareDistance = vds;
                            c1.ClosestNeighbour = c2;
                        }
                    }
                }
            }
            foreach (var i in listOfEliminated.Reverse())
            {
                Cores.RemoveAt(i);
            }
        }

        #endregion

    }
}
