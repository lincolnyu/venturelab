namespace SecurityAccess.MultiTapMethod
{
    public class StatisticPoint
    {
        #region Fields

        /// <summary>
        ///  First day that is late enough to make a statistic point
        /// </summary>
        public const int FirstCentralDay = 1299;

        /// <summary>
        ///  the number of days after the above days the data point predicts up to
        /// </summary>
        public const int MinDistToEnd = 65;

        #endregion

        #region Properties

        #region Inputs

        /// <summary>
        ///  Opening price one business/trading day (same below) before (the FP1 day)
        /// </summary>
        public double P1O { get; set; }
        /// <summary>
        ///  Highest price one day before
        /// </summary>
        public double P1H { get; set; }
        /// <summary>
        ///  Lowest price one day before
        /// </summary>
        public double P1L { get; set; }
        /// <summary>
        ///  Closing price one day before
        /// </summary>
        public double P1C { get; set; }

        /// <summary>
        ///  Price (closing) two days before
        /// </summary>
        public double P2 { get; set; } // close and same as below before P5

        /// <summary>
        ///  Price 3 days before
        /// </summary>
        public double P3 { get; set; }

        /// <summary>
        ///  Price 4 days before
        /// </summary>
        public double P4 { get; set; }

        /// <summary>
        ///  Price 5 days before (1 week)
        /// </summary>
        public double P5 { get; set; } 

        /// <summary>
        ///  Price 10 days before
        /// </summary>
        public double P10 { get; set; } // avg and same as below

        /// <summary>
        ///  Price 20 days before
        /// </summary>
        public double P20 { get; set; }

        /// <summary>
        ///  Price 65 days before (~3 months)
        /// </summary>
        public double P65 { get; set; }

        /// <summary>
        ///  Price 130 days before (~1/2 year)
        /// </summary>
        public double P130 { get; set; }

        /// <summary>
        ///  Price 260 days before (~1 year)
        /// </summary>
        public double P260 { get; set; }

        /// <summary>
        ///  Price 520 days before (~2 years)
        /// </summary>
        public double P520 { get; set; }

        /// <summary>
        ///  Price 1300 days before (~5 years)
        /// </summary>
        public double P1300 { get; set; }

        /// <summary>
        ///  Volume 1 day before
        /// </summary>
        public double V1 { get; set; }

        /// <summary>
        ///  Volume 2 days before
        /// </summary>
        public double V2 { get; set; }

        /// <summary>
        ///  Volume 3 days before
        /// </summary>
        public double V3 { get; set; }

        /// <summary>
        ///  Volume 4 days before
        /// </summary>
        public double V4 { get; set; }

        /// <summary>
        ///  Volume 5 days before (1 week)
        /// </summary>
        public double V5 { get; set; } 

        /// <summary>
        ///  Volume 10 days before (2 weeks)
        /// </summary>
        public double V10 { get; set; } // avg and same as below

        /// <summary>
        ///  Volume 20 days before (~1 month)
        /// </summary>
        public double V20 { get; set; }

        /// <summary>
        ///  Volume 65 days before (~3 months)
        /// </summary>
        public double V65 { get; set; }

        /// <summary>
        ///  Volume 260 days before (~1 year)
        /// </summary>
        public double V260 { get; set; }

        #endregion

        #region Outputs

        /// <summary>
        ///  Price 1 day after
        /// </summary>
        public double FP1 { get; set; }  // close

        /// <summary>
        ///  Price 2 days after
        /// </summary>
        public double FP2 { get; set; }  // close

        /// <summary>
        ///  Price 5 days after (1 week)
        /// </summary>
        public double FP5 { get; set; }  // avg of close and same as below

        /// <summary>
        ///  Price 10 days after (2 weeks)
        /// </summary>
        public double FP10 { get; set; }

        /// <summary>
        ///  Price 20 days after (~1 month)
        /// </summary>
        public double FP20 { get; set; }

        /// <summary>
        ///  Price 65 days after (~3 months)
        /// </summary>
        public double FP65 { get; set; }

        #endregion

        #endregion
    }
}
