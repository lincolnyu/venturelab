using System;
using System.Collections.Generic;
using System.Linq;

namespace GaussianCore.Generic
{
    public class VariableConfinedCoreManager : GenericCoreManager
    {
        #region Nested types
        public class CoreInfo
        {
            #region Properties

            public GenericCore Core { get; set; }

            public double MinInputSquareDistance { get; set; }

            public double MinOutputSquareDistance { get; set; }

            public double[] OutputOffsets { get; set; }

            #endregion

            #region Methods

            public void UpdateCoreCoeffs()
            {
                const double relax = 4.0;// empirical? 4.0 for theoreticallydropping to half at halfway to the nearest
                var core = (GaussianConfinedCore)Core;

                var alpha2 = core.Alpha * 2;
                var r = alpha2 / (alpha2 - 1);
                var linput = Math.Log(Attenuation) * relax*0.5 / MinInputSquareDistance; //*4 to drop to half halfway to neighbour
                for (var i = 0; i < core.InputLength; i++)
                {
                    core.K[i] = linput;
                }

                for (var i = 0; i < core.OutputLength; i++)
                {
                    var sqoo = OutputOffsets[i];
                    var loutput = Math.Log(Attenuation) * relax / sqoo;
                    core.L[i] = loutput;
                }

                core.UpdateInvLCoeff();
            }

            #endregion
        }

        #endregion

        #region Properties

        public override double EpsilonY => double.Epsilon;

        public override double EpsilonSquareY => double.Epsilon;

        public List<CoreInfo> CoreInfos { get; } = new List<CoreInfo>();

        public const double DistanceToEliminate = 0.0001;

        public const double Attenuation = 0.5;

        public int InputLength => CoreInfos[0].Core.InputLength;

        public int OutputLength => CoreInfos[0].Core.OutputLength;

        #endregion

        #region Methods

        #region GenericCoreManager members

        public override IEnumerator<GenericCore> GetEnumerator()
        {
            return CoreInfos.Select(ci => ci.Core).GetEnumerator();
        }

        #endregion

        public void AddCore(GaussianConfinedCore core)
        {
            CoreInfos.Add(new CoreInfo
            {
                Core = core
            });
        }

        public void UpdateCoreCoeffs()
        {
            UpdateMinimumDistances();

            foreach (var ci in CoreInfos)
            {
                ci.UpdateCoreCoeffs();
            }
        }

        public void UpdateMinimumDistances()
        {
            var outputLen = OutputLength;
            
            foreach (var ci in CoreInfos)
            {
                ci.MinInputSquareDistance = double.MaxValue;
                ci.MinOutputSquareDistance = double.MaxValue;
            }

            var eliminated = new HashSet<int>();
            var listOfEliminated = new LinkedList<int>();

            for (var i = 0; i < CoreInfos.Count - 1; i++)
            {
                if (eliminated.Contains(i))
                {
                    listOfEliminated.AddLast(i);
                    continue;
                }

                var outOffset = new double[outputLen];
                var c1 = CoreInfos[i];
                for (var j = i + 1; j < CoreInfos.Count; j++)
                {
                    var c2 = CoreInfos[j];
                    var sqid = c1.Core.GetInputSquareDistanceEuclid(c2.Core);
                    c1.Core.GetOutputOffsetSquare(c2.Core, outOffset);
                    var sqod = outOffset.Sum();
                    var sqd = sqid + sqod;

                    if (sqd < DistanceToEliminate)
                    {
                        eliminated.Add(j);
                        c1.Core.Weight++;
                        continue;
                    }

                    var used = false;
                    if (sqid <= c2.MinInputSquareDistance)
                    {
                        if (sqid < c2.MinInputSquareDistance || sqod < c2.MinOutputSquareDistance)
                        {
                            c2.MinInputSquareDistance = sqid;
                            c2.OutputOffsets = outOffset; 
                            c2.MinOutputSquareDistance = sqod;
                            used = true;
                        }
                    }

                    if (sqid <= c1.MinInputSquareDistance)
                    {
                        if (sqid < c1.MinInputSquareDistance || sqod < c1.MinOutputSquareDistance)
                        {
                            c1.MinInputSquareDistance = sqid;
                            c1.OutputOffsets = outOffset;
                            c1.MinOutputSquareDistance = sqod;
                            used = true;
                        }
                    }

                    if (used)
                    {
                        outOffset = new double[outputLen]; // renew
                    }
                }
            }
            foreach (var i in listOfEliminated.Reverse())
            {
                CoreInfos.RemoveAt(i);
            }
        }
        
        #endregion
    }
}
