using System.Collections.Generic;
using System.Linq;

namespace GaussianCore.Spheric
{
    public class VariableCoreManager : GaussianCoreManager
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
                    var sqid = c1.GetSquareDistanceEuclid(c2, 0, OutputStartingFrom);
                    var outOffset = c2.GetSquareOffset(c1, OutputStartingFrom, componentSize);
                    var sqod = outOffset.Sum();
                    var sqd = sqid + sqod;

                    if (sqd < DistanceTolerance)
                    {
                        // merge i and j by removing j
                        eliminated.Add(j);
                        c1.Multiple++;
                        continue;
                    }

                    if (sqid <= c2.MinimumInputSquareDistance)
                    {
                        
                        if (sqid < c2.MinimumInputSquareDistance || sqod < c2.MinimumOutputSquareDistance)
                        {
                            c2.MinimumInputSquareDistance = sqid;
                            c2.OutputOffsets = outOffset;
                            c2.MinimumOutputSquareDistance = sqod;
                            c2.ClosestNeighbour = c1;
                        }
                    }
                    
                    if (sqid < c1.MinimumInputSquareDistance)
                    {
                        if (sqid < c1.MinimumInputSquareDistance || sqod < c1.MinimumOutputSquareDistance)
                        {
                            c1.MinimumInputSquareDistance = sqid;
                            c1.OutputOffsets = outOffset;
                            c1.MinimumOutputSquareDistance = sqod;
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
