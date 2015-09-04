using System.IO;

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
            tw.WriteLine("FP1:  {0:###.0}% / {1:###.0}%", FP1 * 100, FP1Sv * 100 / FP1);
            tw.WriteLine("FP2:  {0:###.0}% / {1:###.0}%", FP2 * 100, FP2Sv * 100 / FP2);
            tw.WriteLine("FP5:  {0:###.0}% / {1:###.0}%", FP5 * 100, FP5Sv * 100 / FP5);
            tw.WriteLine("FP15: {0:###.0}% / {1:###.0}%", FP15 * 100, FP15Sv * 100 / FP15);
            tw.WriteLine("FP30: {0:###.0}% / {1:###.0}%", FP30 * 100, FP30Sv * 100 / FP30);
            tw.WriteLine("FP90: {0:###.0}% / {1:###.0}%", FP90 * 100, FP90Sv * 100 / FP90);
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
