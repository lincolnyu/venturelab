using System;

namespace VentureLab.Helpers
{
    public static class StatisticsHelper
    {
        public static double GetVariance(double x, double xx)
        {
            return xx - x * x; 
        }

        public static double GetStandardVariance(double x, double xx)
        {
            return Math.Sqrt(GetVariance(x, xx));
        }
    }
}
