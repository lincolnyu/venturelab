using System;
using System.Collections.Generic;

namespace VentureLab.Helpers
{
    public static class DistanceHelper
    {
        /// <summary>
        ///  Returns the absolute difference between two sequences
        /// </summary>
        /// <param name="aa">The first vector</param>
        /// <param name="bb">The second vector</param>
        /// <param name="diff">The total absolute difference</param>
        /// <param name="count">The the total number of components in each vector</param>
        public static void GetAbsDiff(IEnumerable<double> aa, IEnumerable<double> bb, out double diff, out int count)
        {
            var eaa = aa.GetEnumerator();
            var ebb = bb.GetEnumerator();
            diff = 0.0;
            count = 0;
            while (eaa.MoveNext() && ebb.MoveNext())
            {
                var a = eaa.Current;
                var b = ebb.Current;
                var d = Math.Abs(a - b);
                diff += d;
                count++;
            }
        }

        /// <summary>
        ///  Returns the square difference between two sequences
        /// </summary>
        /// <param name="aa">The first vector</param>
        /// <param name="bb">The second vector</param>
        /// <param name="diff">The total square difference</param>
        /// <param name="count">The the total number of components in each vector</param>
        public static void GetSqrDiff(IEnumerable<double> aa, IEnumerable<double> bb, out double diff, out int count)
        {
            var eaa = aa.GetEnumerator();
            var ebb = bb.GetEnumerator();
            diff = 0.0;
            count = 0;
            while (eaa.MoveNext() && ebb.MoveNext())
            {
                var a = eaa.Current;
                var b = ebb.Current;
                var d = a - b;
                diff += d * d;
                count++;
            }
        }


        /// <summary>
        ///  Returns the absolute difference between two sequence divided by the sequence length
        /// </summary>
        /// <param name="aa">The first sequence</param>
        /// <param name="bb">The second sequence</param>
        /// <returns>sum from i = 0 to N-1 | a sub i - b sub i | over N</returns>
        public static double GetMeanAbsDiff(IEnumerable<double> aa, IEnumerable<double> bb)
        {
            double diff;
            int count;
            GetAbsDiff(aa, bb, out diff, out count);
            return diff / count;
        }

        /// <summary>
        ///  Returns the square difference between two sequence divided by the sequence length
        /// </summary>
        /// <param name="aa">The first sequence</param>
        /// <param name="bb">The second sequence</param>
        /// <returns>sum from i = 0 to N-1 ( a sub i - b sub i ) ^ 2 over N</returns>
        public static double GetMeanSqrDiff(IEnumerable<double> aa, IEnumerable<double> bb)
        {
            double diff;
            int count;
            GetSqrDiff(aa, bb, out diff, out count);
            return diff / count;
        }
    }
}
