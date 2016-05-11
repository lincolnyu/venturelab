using System;
using GaussianCore.Generic;

namespace GaussianCore.Classification
{
    public static class DigioCoreBuilder
    {
        public static void Build(this GenericCoreManager gcm, DigioClassifier dcf, string code)
        {
            var index = dcf.Codes.BinarySearch(code);
            if (index < 0)
            {
                throw new ArgumentException("No data for the specified code");
            }
            var c = dcf.CoreLists[index];
            AddCore(gcm, c, 1);
            for (var i = 0; i < dcf.Codes.Count; i++)
            {
                if (i != index)
                {
                    var w = dcf.GetWeight(index, i);
                    if (w > 0)
                    {
                        AddCore(gcm, dcf.CoreLists[i], w);
                    }
                }
            }
        }

        private static void AddCore(GenericCoreManager gcm, DigioClassifier.CodeCoreList coreList, double weight)
        {
            foreach (var c in coreList.Cores)
            {
                var core = (GenericCore) c.Core;
                // TODO make a clone?
                core.Weight = weight;
                gcm.AddCore(core);
            }
        }
    }
}
