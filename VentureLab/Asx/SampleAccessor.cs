using System;
using VentureLab.QbClustering;

namespace VentureLab.Asx
{
    public class SampleAccessor
    {
        public const int InputCount = 22;
        public const int OutputCount = 6;

        public const int DaysBefore = 1299;
        public const int DaysAfter = 65;

        public const double MinValue = 0.0001;

        public SampleAccessor(IStrainPoint sp)
        {
            StrainPoint = sp;
        }
        
        public IStrainPoint StrainPoint { get; }

        public double P1O { get; set; }
        public double P1H { get; set; }
        public double P1L { get; set; }
        public double P1C { get; set; }
        public double V1 { get; set; }

        public double P2 { get; set; }
        public double V2 { get; set; }

        public double P3 { get; set; }
        public double V3 { get; set; }

        public double P4 { get; set; }
        public double V4 { get; set; }

        public double P5 { get; set; }
        public double V5 { get; set; }

        public double P10 { get; set; }
        public double V10 { get; set; }

        public double P20 { get; set; }
        public double V20 { get; set; }

        public double P65 { get; set; }
        public double V65 { get; set; }

        public double P130 { get; set; }

        public double P260 { get; set; }
        public double V260 { get; set; }

        public double P520 { get; set; }
        public double P1300 { get; set; }

        public double FP1 { get; set; }
        public double FP2 { get; set; }
        public double FP5 { get; set; }
        public double FP10 { get; set; }
        public double FP20 { get; set; }
        public double FP65 { get; set; }

        public void UpdateInput()
        {
            StrainPoint.Input[0] = GetLogarithm(P1O, P1C);
            StrainPoint.Input[1] = GetLogarithm(P1H, P1C);
            StrainPoint.Input[2] = GetLogarithm(P1L, P1C);
            StrainPoint.Input[3] = GetLogarithm(P2, P1C);
            StrainPoint.Input[4] = GetLogarithm(P3, P1C);
            StrainPoint.Input[5] = GetLogarithm(P4, P1C);
            StrainPoint.Input[6] = GetLogarithm(P5, P1C);
            StrainPoint.Input[7] = GetLogarithm(P10, P1C);
            StrainPoint.Input[8] = GetLogarithm(P20, P1C);
            StrainPoint.Input[9] = GetLogarithm(P65, P1C);
            StrainPoint.Input[10] = GetLogarithm(P130, P1C);
            StrainPoint.Input[11] = GetLogarithm(P260, P1C);
            StrainPoint.Input[12] = GetLogarithm(P520, P1C);
            StrainPoint.Input[13] = GetLogarithm(P1300, P1C);
            StrainPoint.Input[14] = GetLogarithm(V2, V1);
            StrainPoint.Input[15] = GetLogarithm(V3, V1);
            StrainPoint.Input[16] = GetLogarithm(V4, V1);
            StrainPoint.Input[17] = GetLogarithm(V5, V1);
            StrainPoint.Input[18] = GetLogarithm(V10, V1);
            StrainPoint.Input[19] = GetLogarithm(V20, V1);
            StrainPoint.Input[20] = GetLogarithm(V65, V1);
            StrainPoint.Input[21] = GetLogarithm(V260, V1);
        }

        public void UpdateOutput()
        {
            StrainPoint.Output[0] = GetLogarithm(FP1, P1C);
            StrainPoint.Output[1] = GetLogarithm(FP2, P1C);
            StrainPoint.Output[2] = GetLogarithm(FP5, P1C);
            StrainPoint.Output[3] = GetLogarithm(FP10, P1C);
            StrainPoint.Output[4] = GetLogarithm(FP20, P1C);
            StrainPoint.Output[5] = GetLogarithm(FP65, P1C);
        }

        public static double GetTwoToPower(double b, double index)
        {
            return b * Math.Pow(2, index);
        }

        /// <summary>
        ///  returns the logarithm of the input relative to the reverence value:
        /// </summary>
        /// <param name="v">The input</param>
        /// <param name="refval">The reference value</param>
        /// <returns>The logarithmic result</returns>
        public static double GetLogarithm(double v, double refval)
        {
            if (v < MinValue) v = MinValue;
            if (refval < MinValue) refval = MinValue;
            var res = (Math.Log(v) - Math.Log(refval)) / Math.Log(2);
            return res;
        }
    }
}
