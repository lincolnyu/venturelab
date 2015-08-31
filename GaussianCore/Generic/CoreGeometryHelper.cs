using System.Collections.Generic;

namespace GaussianCore.Generic
{
    public static class CoreGeometryHelper
    {
        #region Methods

        public static double GetInputSquareDistanceEuclid(this GenericCore core1, GenericCore core2)
        {
            var sum = 0.0;
            for (var k = 0; k < core1.InputLength; k++)
            {
                var d = core1.CentersInput[k] - core2.CentersInput[k];
                d *= d;
                sum += d;
            }
            return sum;
        }

        public static void GetOutputOffsetSquare(this GenericCore core1, GenericCore core2, IList<double> result)
        {
            for (var k = 0; k < core1.OutputLength; k++)
            {
                var d = core2.CentersOutput[k] - core1.CentersOutput[k];
                d *= d;
                result[k] = d;
            }
        }


        #endregion
    }
}
