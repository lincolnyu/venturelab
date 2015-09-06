using System.IO;
using System.Text;

namespace SecurityAccess.MultiTapMethod
{
    public class Prediction
    {
        #region Properties

        // expected price vs last
        public double FP1 { get; set; }  // close
        public double FP2 { get; set; }  // close
        public double FP5 { get; set; }  // avg of close and same as below
        public double FP15 { get; set; }
        public double FP30 { get; set; }
        public double FP90 { get; set; }

        // standard variance
        public double FP1Sv { get; set; }
        public double FP2Sv { get; set; }
        public double FP5Sv { get; set; }
        public double FP15Sv { get; set; }
        public double FP30Sv { get; set; }
        public double FP90Sv { get; set; }

        #endregion

        #region Methods

        public void Export(TextWriter tw)
        {
            tw.WriteLine("FP1:  {0}", FormatResult(FP1, FP1Sv));
            tw.WriteLine("FP2:  {0}", FormatResult(FP2, FP2Sv));
            tw.WriteLine("FP5:  {0}", FormatResult(FP5, FP5Sv));
            tw.WriteLine("FP15: {0}", FormatResult(FP15, FP15Sv));
            tw.WriteLine("FP30: {0}", FormatResult(FP30, FP30Sv));
            tw.WriteLine("FP90: {0}", FormatResult(FP90, FP90Sv));
        }

        private string FormatResult(double mean, double sv)
        {
            string s;

            if (mean > 1)
            {
                s = string.Format("+{0:  0.0} (+-{1:0.0}) %", (mean - 1) * 100, sv * 100);
            }
            else if (mean < 1)
            {
                s = string.Format("-{0:  0.0} (+-{1:0.0}) %", (1 - mean) * 100, sv * 100);
            }
            else
            {
                s = string.Format("   0.0 (+-{0:0.0}) %", sv);
            }

            return s;
        } 

        public void Export(string fn, string desc, bool append=false)
        {
            using (var sw = new StreamWriter(fn))
            {
                Export(sw);
            }
        }

        #endregion
    }
}
