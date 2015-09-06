namespace SecurityAccess.MultiTapMethod
{
    public class StatisticPoint
    {
        #region Fields

        /// <summary>
        ///  First day that is late enough to make a statistic point
        /// </summary>
        public const int FirstCentralDay = 1299;

        public const int MinDistToEnd = 65;

        #endregion

        #region Properties

        #region Inputs

        public double P1O { get; set; }
        public double P1H { get; set; }
        public double P1L { get; set; }
        public double P1C { get; set; }

        public double P2 { get; set; } // close and same as below before P5
        public double P3 { get; set; }
        public double P4 { get; set; }
        public double P5 { get; set; } 
        public double P10 { get; set; } // avg and same as below
        public double P20 { get; set; }
        public double P65 { get; set; }
        public double P130 { get; set; }
        public double P260 { get; set; }
        public double P520 { get; set; }
        public double P1300 { get; set; }

        public double V1 { get; set; }
        public double V2 { get; set; }
        public double V3 { get; set; }
        public double V4 { get; set; }
        public double V5 { get; set; } 
        public double V10 { get; set; } // avg and same as below
        public double V20 { get; set; }
        public double V65 { get; set; }
        public double V260 { get; set; }

        #endregion

        #region Outputs

        public double FP1 { get; set; }  // close
        public double FP2 { get; set; }  // close
        public double FP5 { get; set; }  // avg of close and same as below
        public double FP10 { get; set; }
        public double FP20 { get; set; }
        public double FP65 { get; set; }

        #endregion

        #endregion
    }
}
