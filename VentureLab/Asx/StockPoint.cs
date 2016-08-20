using System;
using System.Collections.Generic;
using System.Linq;
using VentureLab.Prediction;
using VentureLab.QbClustering;
using VentureLab.QbGaussianMethod.Cores;
using VentureLab.QbGaussianMethod.Predictors;

namespace VentureLab.Asx
{
    public class StockPoint : Point, IStrainPoint
    {
        public class Manager : ConfinedGaussianPredictor, IPointManager, IGaussianCoreFactory
        {
            public GaussianRegulatedCoreConstants SharedConstants;
            public GaussianRegulatedCoreVariables SharedVariables;

            #region IPointFactory members

            public IPoint Create() => new StockPoint(SampleAccessor.InputCount, SampleAccessor.OutputCount);

            #endregion

            #region IGaussianCoreFactory members

            #region ICoreFactory members

            public IEnumerable<ICore> CreateCores(IEnumerable<IPoint> point) =>
                point.Select(p => new GaussianRegulatedCore(p, SharedConstants, SharedVariables));

            #endregion

            public IEnumerable<GaussianRegulatedCoreVariables> GetCoreVariableSets()
            {
                yield return SharedVariables;
            }

            #endregion

            /// <summary>
            ///  Sets up the shared constants for the subsequent core creation
            ///  Note this is not thread safe
            /// </summary>
            /// <param name="m">M (major) argument</param>
            /// <param name="n">N (minor) argument</param>
            public void PrepareCoreCreation(double m, double n)
            {
                SharedConstants = new GaussianRegulatedCoreConstants(SampleAccessor.OutputCount, m, n);
                SharedVariables = new GaussianRegulatedCoreVariables(SampleAccessor.InputCount, SampleAccessor.OutputCount);
            }
        }

        public enum Status
        {
            Available,
            Engaged,
            Copied
        }

        public StockPoint(int inputLen, int outputLen) : base(inputLen, outputLen)
        {
        }

        public double Indicator { get; set; }

        public int CompareTo(IStrainPoint other) => Indicator.CompareTo(other.Indicator);
    }
}
