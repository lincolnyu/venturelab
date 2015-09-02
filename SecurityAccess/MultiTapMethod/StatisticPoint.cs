namespace SecurityAccess.MultiTapMethod
{
    public class StatisticPoint
    {
        #region Fields

        /// <summary>
        ///  First day that is late enough to make a statistic point
        /// </summary>
        public const int FirstCentralDay = 1799;

        public const int MinDistToEnd = 360;

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
        public double P15 { get; set; } // avg and same as below
        public double P30 { get; set; }
        public double P90 { get; set; }
        public double P180 { get; set; }
        public double P360 { get; set; }
        public double P720 { get; set; }
        public double P1800 { get; set; }

        public double V1 { get; set; }
        public double V2 { get; set; }
        public double V3 { get; set; }
        public double V4 { get; set; }
        public double V5 { get; set; } 
        public double V15 { get; set; } // avg and same as below
        public double V30 { get; set; }
        public double V90 { get; set; }
        public double V360 { get; set; }

        #endregion

        #region Outputs

        public double FP1 { get; set; }  // close
        public double FP2 { get; set; }  // close
        public double FP5 { get; set; }  // avg of close and same as below
        public double FP15 { get; set; }
        public double FP30 { get; set; }
        public double FP90 { get; set; }

        #endregion

        #endregion
    }
}
