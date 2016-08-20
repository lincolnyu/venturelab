using System.Collections.Generic;

namespace VentureLab.QbGaussianMethod.Cores
{
    public interface IGaussianCoreFactory : ICoreFactory
    {
        IEnumerable<GaussianRegulatedCoreVariables> GetCoreVariableSets();
    }
}
