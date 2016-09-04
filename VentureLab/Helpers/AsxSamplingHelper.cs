using System.Collections.Generic;
using VentureCommon;
using VentureLab.Asx;
using VentureLab.QbClustering;
using VentureLab.QbGaussianMethod.Cores;

namespace VentureLab.Helpers
{
    public static class AsxSamplingHelper
    {
        public static SampleAccessor CreateSampleAccessor(this IPointFactory pointFactory)
        {
            var point = (IStrainPoint)pointFactory.Create();
            var sa = new SampleAccessor(point);
            return sa;
        }

        public static IEnumerable<IStrainPoint> Sample(this IPointFactory pointFactory, IList<StockRecord> data, int start, int end, int interval)
        {
            for (var i = start; i < end; i += interval)
            {
                var sa = pointFactory.CreateSampleAccessor();
                sa.SampleOne(data, i);
                yield return sa.StrainPoint;
            }
        }

        /// <summary>
        ///  Returns the range of a list of the specified length that can be sampled
        /// </summary>
        /// <param name="len">The length of the list</param>
        /// <param name="start">The start index (inclusive)</param>
        /// <param name="end">The end index (exclusive)</param>
        public static void GetStartAndEnd(int len, out int start, out int end)
        {
            start = SampleAccessor.DaysBefore;
            end = len - SampleAccessor.DaysAfter;
        }
        
        public static void SampleOne(this SampleAccessor sa, IList<StockRecord> data, int day0)
        {
            sa.SampleOneInput(data, day0);
            sa.SampleOneOutput(data, day0);
        }

        public static void SampleOneInput(this SampleAccessor sa, IList<StockRecord> data, int day0)
        {
            // history prices and volumes
            var d1 = data[day0];
            sa.P1O = d1.Open;
            sa.P1H = d1.High;
            sa.P1L = d1.Low;
            sa.P1C = d1.Close;
            sa.V1 = d1.Volume;

            var d2 = data[day0 - 1];
            sa.P2 = d2.Close;
            sa.V2 = d2.Volume;

            var d3 = data[day0 - 2];
            sa.P3 = d3.Close;
            sa.V3 = d3.Volume;

            var d4 = data[day0 - 3];
            sa.P4 = d4.Close;
            sa.V4 = d4.Volume;

            var d5 = data[day0 - 4];
            sa.P5 = d5.Close;
            sa.V5 = d5.Volume;

            double pmean = 0;
            double vmean = 0;

            int j;
            var sum = 0.0;
            var vsum = 0.0;
            for (j = 0; j < 10; j++)
            {
                sum += GetClose(data, day0, j, ref pmean);
                vsum += GetVolume(data, day0, j, ref vmean);
            }
            sa.P10 = sum / 10;
            sa.V10 = vsum / 10;

            for (; j < 20; j++)
            {
                sum += GetClose(data, day0, j, ref pmean);
                vsum += GetVolume(data, day0, j, ref vmean);
            }
            sa.P20 = sum / 20;
            sa.V20 = vsum / 20;

            for (; j < 65; j++)
            {
                sum += GetClose(data, day0, j, ref pmean);
                vsum += GetVolume(data, day0, j, ref vmean);
            }
            sa.P65 = sum / 65;
            sa.V65 = vsum / 65;

            for (; j < 130; j++)
            {
                sum += GetClose(data, day0, j, ref pmean);
                vsum += GetVolume(data, day0, j, ref vmean);
            }
            sa.P130 = sum / 130;

            for (; j < 260; j++)
            {
                sum += GetClose(data, day0, j, ref pmean);
                vsum += GetVolume(data, day0, j, ref vmean);
            }
            sa.P260 = sum / 260;
            sa.V260 = vsum / 260;

            for (; j < 520; j++)
            {
                sum += GetClose(data, day0, j, ref pmean);
            }
            sa.P520 = sum / 520;

            for (; j < 1300; j++)
            {
                sum += GetClose(data, day0, j, ref pmean);
            }
            sa.P1300 = sum / 1300;

            sa.UpdateInput();
        }

        private static double GetClose(IList<StockRecord> entry, int day0,
            int j, ref double sum)
        {
            if (day0 >= j)
            {
                var result = entry[day0 - j].Close;
                sum += result;
                return result;
            }

            if (day0 == j-1)
            {
                sum /= j;
            }
            return sum;
        }

        private static double GetVolume(IList<StockRecord> entry, int day0,
            int j, ref double sum)
        {
            if (day0 >= j)
            {
                var result = entry[day0 - j].Volume;
                sum += result;
                return result;
            }

            if (day0 == j - 1)
            {
                sum /= j;
            }
            return sum;
        }

        public static void SampleOneOutput(this SampleAccessor sa, IList<StockRecord> data, int day0)
        {
            var sum = 0.0;
            int j;
            sa.FP1 = (data[day0 + 1].High + data[day0 + 1].Low) / 2;
            sa.FP2 = (data[day0 + 2].High + data[day0 + 2].Low) / 2;
            for (j = 0; j < 5; j++)
            {
                sum += data[day0 + j].Close;
            }
            sa.FP5 = sum / 5;

            for (; j < 10; j++)
            {
                sum += data[day0 + j].Close;
            }
            sa.FP10 = sum / 10;

            for (; j < 20; j++)
            {
                sum += data[day0 + j].Close;
            }
            sa.FP20 = sum / 20;

            for (; j < 65; j++)
            {
                sum += data[day0 + j].Close;
            }
            sa.FP65 = sum / 65;

            sa.UpdateOutput();
        }
    }
}
