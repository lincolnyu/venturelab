using SecurityAccess.Asx;
using SecurityAccess.MultiTapMethod;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SecurityAccess
{
    /// <summary>
    ///  extracts statistic point from raw stock data.
    /// </summary>
    public static class ExtractHelper
    {
        #region Methods

        /// <summary>
        ///  Sucks data and creates model
        /// </summary>
        /// <param name="dir">The directory the files are in</param>
        /// <param name="data">data in chronical order (oldest first)</param>
        /// <param name="start">days from the first one that is late enough to make 
        /// statistic point to the day to start the processing with</param>
        /// <param name="interval">interval in number of days</param>
        /// <param name="count">total number of statistic point to try to extract</param>
        public static IEnumerable<StatisticPoint> Suck(this IList<DailyStockEntry> data, int start, int interval, 
            int count = int.MaxValue)
        {
            for (var i = StatisticPoint.FirstCentralDay + start; count > 0 
                && i < data.Count - StatisticPoint.MinDistToEnd; i++, count--)
            {
                var sp = data.SuckOne(i);

                yield return sp;
            }
        }

        public static StatisticPoint SuckOnlyInput(this IList<DailyStockEntry> data, int start)
        {
            var sp = new StatisticPoint();

            // history prices and volumes

            var d1 = data[start];
            sp.P1O = d1.Open;
            sp.P1H = d1.High;
            sp.P1L = d1.Low;
            sp.P1C = d1.Close;
            sp.V1 = d1.Volume;

            var d2 = data[start - 1];
            sp.P2 = d2.Close;
            sp.V2 = d2.Volume;

            var d3 = data[start - 2];
            sp.P3 = d3.Close;
            sp.V3 = d3.Volume;

            var d4 = data[start - 3];
            sp.P4 = d4.Close;
            sp.V4 = d4.Volume;

            var d5 = data[start - 4];
            sp.P5 = d5.Close;
            sp.V5 = d5.Volume;

            int j;
            var sum = 0.0;
            var vsum = 0.0;
            for (j = 0; j < 15; j++)
            {
                sum += data[start - j].Close;
                vsum += data[start - j].Volume;
            }
            sp.P15 = sum / 15;
            sp.V15 = vsum / 15;

            for (; j < 30; j++)
            {
                sum += data[start - j].Close;
                vsum += data[start - j].Volume;
            }
            sp.P30 = sum / 30;
            sp.V30 = vsum / 30;

            for (; j < 90; j++)
            {
                sum += data[start - j].Close;
                vsum += data[start - j].Volume;
            }
            sp.P90 = sum / 90;
            sp.V90 = vsum / 90;

            for (; j < 180; j++)
            {
                sum += data[start - j].Close;
                vsum += data[start - j].Volume;
            }
            sp.P180 = sum / 180;

            for (; j < 360; j++)
            {
                sum += data[start - j].Close;
                vsum += data[start - j].Volume;
            }
            sp.P360 = sum / 360;
            sp.V360 = vsum / 360;

            for (; j < 720; j++)
            {
                sum += data[start - j].Close;
            }
            sp.P720 = sum / 720;

            for (; j < 1800; j++)
            {
                sum += data[start - j].Close;
            }
            sp.P1800 = sum / 1800;

            return sp;
        }

        public static StatisticPoint SuckOne(this IList<DailyStockEntry> data, int start)
        {
            var sp = data.SuckOnlyInput(start);   

            // future prices

            var sum = 0.0;
            int j;

            sp.FP1 = (data[start + 1].High + data[start + 1].Low) / 2;
            sp.FP2 = (data[start + 2].High + data[start + 2].Low) / 2;
            for (j = 0; j < 5; j++)
            {
                sum += data[start + j].Close;
            }
            sp.FP5 = sum / 5;

            for (; j < 15; j++)
            {
                sum += data[start + j].Close;
            }
            sp.FP15 = sum / 15;

            for (; j < 30; j++)
            {
                sum += data[start + j].Close;
            }
            sp.FP30 = sum / 30;

            for (; j < 90; j++)
            {
                sum += data[start + j].Close;
            }
            sp.FP90 = sum / 90;

            return sp;
        }

        public static int Export(this IEnumerable<StatisticPoint> points, StreamWriter sw)
        {
            var count = 0;
            foreach (var p in points)
            {
                var a = 1 / p.P1C;
                var b = 1 / p.V1;
                sw.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},"
                    + "{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27}",
                    p.P1O * a, p.P1H * a, p.P1L * a, p.P2 * a, p.P3 * a, p.P4 * a, p.P5 * a, p.P15 * a,
                    p.P30 * a, p.P90 * a, p.P180 * a, p.P360 * a, p.P720 * a, p.P1800 * a,
                    p.V2 * b, p.V3 * b, p.V4 * b, p.V5 * b, p.V15 * b, p.V30 * b, p.V90 * b, p.V360 * b,
                    p.FP1 * a, p.FP2 * a, p.FP5 * a, p.FP15 * a, p.FP30 * a, p.FP90 * a);
                count++;
            }
            return count;
        }

        public static void GenerateInput(this StatisticPoint p, IList<double> input)
        {
            var a = 1 / p.P1C;
            var b = 1 / p.V1;
            input[0] = p.P1O * a;
            input[1] = p.P1H * a;
            input[2] = p.P1L * a;
            input[3] = p.P2 * a;
            input[4] = p.P3 * a;
            input[5] = p.P4 * a;
            input[6] = p.P5 * a;
            input[7] = p.P15* a;
            input[8] = p.P30 * a;
            input[9] = p.P90 * a;
            input[10] = p.P180 * a;
            input[11] = p.P360 * a;
            input[12] = p.P720 * a;
            input[13] = p.P1800 * a;
            input[14] = p.V2 * b;
            input[15] = p.V3 * b;
            input[16] = p.V4 * b;
            input[17] = p.V5 * b;
            input[18] = p.V15 * b;
            input[19] = p.V30 * b;
            input[20] = p.V90 * b;
            input[21] = p.V360 * b;
        }

        public static void ProcessFiles(this string srcDir, string dstDir, TextWriter logWriter = null)
        {
            var srcDirInfo = new DirectoryInfo(srcDir);
            var srcFiles = srcDirInfo.GetFiles();

            var infoFile = Path.Combine(dstDir, "_info.txt");
            var lengths = new Dictionary<string, int>();
            if (File.Exists(infoFile))
            {
                using (var sr = new StreamReader(infoFile))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        var segs = line.Split(':');
                        var key = segs[0];
                        var len = int.Parse(segs[1]);
                        lengths[key] = len;
                    }
                }
            }

            foreach (var srcFile in srcFiles.OrderBy(x => x.Name))
            {
                string code;
                if (!FileReorganiser.IsCodeFile(srcFile.Name, out code))
                {
                    continue;
                }
                int len = 0;
                bool append = false;
                if (lengths.TryGetValue(code, out len))
                {
                    append = true;
                }

                var dstPath = Path.Combine(dstDir, srcFile.Name);
                var count = ProcessFile(srcFile.FullName, dstPath, append ? len : 0, append);
                if (logWriter != null)
                {
                    logWriter.WriteLine("{0} Processed (starting {1} counting {2})...", srcFile.Name, len, count);
                }
                lengths[code] = len+count;
            }

            using (var sw = new StreamWriter(infoFile))
            {
                foreach (var kvp in lengths)
                {
                    sw.WriteLine("{0}:{1}", kvp.Key, kvp.Value);
                }
            }

            logWriter.WriteLine("All processed.");
        }

        private static int ProcessFile(string srcPath, string dstPath, int start=0, bool append=false)
        {
            var entries = srcPath.ReadDailyStockEntries().ToList();
            var statisticPoints = entries.Suck(start, 1);
            using (var sw = new StreamWriter(dstPath, append))
            {
                var count = statisticPoints.Export(sw);
                return count;
            }
        }

        #endregion
    }
}
