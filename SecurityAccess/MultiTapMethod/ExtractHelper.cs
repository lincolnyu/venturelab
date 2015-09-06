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
        #region Enumerations

        public enum ExportModes
        {
            Text,
            Binary,
            Both
        }

        #endregion

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
            for (j = 0; j < 10; j++)
            {
                sum += data[start - j].Close;
                vsum += data[start - j].Volume;
            }
            sp.P10 = sum / 10;
            sp.V10 = vsum / 10;

            for (; j < 20; j++)
            {
                sum += data[start - j].Close;
                vsum += data[start - j].Volume;
            }
            sp.P20 = sum / 20;
            sp.V20 = vsum / 20;

            for (; j < 65; j++)
            {
                sum += data[start - j].Close;
                vsum += data[start - j].Volume;
            }
            sp.P65 = sum / 65;
            sp.V65 = vsum / 65;

            for (; j < 130; j++)
            {
                sum += data[start - j].Close;
                vsum += data[start - j].Volume;
            }
            sp.P130 = sum / 130;

            for (; j < 260; j++)
            {
                sum += data[start - j].Close;
                vsum += data[start - j].Volume;
            }
            sp.P260 = sum / 260;
            sp.V260 = vsum / 260;

            for (; j < 520; j++)
            {
                sum += data[start - j].Close;
            }
            sp.P520 = sum / 520;

            for (; j < 1300; j++)
            {
                sum += data[start - j].Close;
            }
            sp.P1300 = sum / 1300;

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

            for (; j < 10; j++)
            {
                sum += data[start + j].Close;
            }
            sp.FP10 = sum / 10;

            for (; j < 20; j++)
            {
                sum += data[start + j].Close;
            }
            sp.FP20 = sum / 20;

            for (; j < 65; j++)
            {
                sum += data[start + j].Close;
            }
            sp.FP65 = sum / 65;

            return sp;
        }

        public static void Write(StatisticPoint p, TextWriter tw)
        {
            var a = 1 / p.P1C;
            var b = 1 / p.V1;
            tw.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},"
                + "{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27}",
                p.P1O * a, p.P1H * a, p.P1L * a, p.P2 * a, p.P3 * a, p.P4 * a, p.P5 * a, p.P10 * a,
                p.P20 * a, p.P65 * a, p.P130 * a, p.P260 * a, p.P520 * a, p.P1300 * a,
                p.V2 * b, p.V3 * b, p.V4 * b, p.V5 * b, p.V10 * b, p.V20 * b, p.V65 * b, p.V260 * b,
                p.FP1 * a, p.FP2 * a, p.FP5 * a, p.FP10 * a, p.FP20 * a, p.FP65 * a);
        }

        private static void Write(StatisticPoint p, BinaryWriter bw)
        {
            var a = 1 / p.P1C;
            var b = 1 / p.V1;

            bw.Write(p.P1O * a);
            bw.Write(p.P1H * a);
            bw.Write(p.P1L * a);
            bw.Write(p.P2 * a);
            bw.Write(p.P3 * a);
            bw.Write(p.P4 * a);
            bw.Write(p.P5 * a);
            bw.Write(p.P10 * a);
            bw.Write(p.P20 * a);
            bw.Write(p.P65 * a);
            bw.Write(p.P130 * a);
            bw.Write(p.P260 * a);
            bw.Write(p.P520 * a);
            bw.Write(p.P1300 * a);
            bw.Write(p.V2 * b);
            bw.Write(p.V3 * b);
            bw.Write(p.V4 * b);
            bw.Write(p.V5 * b);
            bw.Write(p.V10 * b);
            bw.Write(p.V20 * b);
            bw.Write(p.V65 * b);
            bw.Write(p.V260 * b);

            bw.Write(p.FP1 * a);
            bw.Write(p.FP2 * a);
            bw.Write(p.FP5 * a);
            bw.Write(p.FP10 * a);
            bw.Write(p.FP20 * a);
            bw.Write(p.FP65 * a);
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
            input[7] = p.P10* a;
            input[8] = p.P20 * a;
            input[9] = p.P65 * a;
            input[10] = p.P130 * a;
            input[11] = p.P260 * a;
            input[12] = p.P520 * a;
            input[13] = p.P1300 * a;
            input[14] = p.V2 * b;
            input[15] = p.V3 * b;
            input[16] = p.V4 * b;
            input[17] = p.V5 * b;
            input[18] = p.V10 * b;
            input[19] = p.V20 * b;
            input[20] = p.V65 * b;
            input[21] = p.V260 * b;
        }

        public static void ProcessFiles(this string srcDir, string dstDir,
            ExportModes mode = ExportModes.Binary, TextWriter logWriter = null)
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

                int count = 0;
                var statisticPoints = GetStatisticPoints(srcFile.FullName, append ? len : 0);
                switch (mode)
                {
                    case ExportModes.Binary:
                        {
                            var dstPath = Path.Combine(dstDir, string.Format("{0}.dat", code));
                            count = statisticPoints.ExportText(dstPath, append);
                            break;
                        }
                    case ExportModes.Text:
                        {
                            var dstPath = Path.Combine(dstDir, string.Format("{0}.txt", code));
                            count = statisticPoints.ExportText(dstPath, append);
                            break;
                        }
                    case ExportModes.Both:
                        {
                            var datPath = Path.Combine(dstDir, string.Format("{0}.dat", code));
                            var txtPath = Path.Combine(dstDir, string.Format("{0}.txt", code));
                            count = statisticPoints.ExportBinaryAndText(datPath, txtPath, append);
                            break;
                        }
                }

                if (logWriter != null)
                {
                    logWriter.WriteLine("{0} Processed (starting {1} counting {2})...", code, len, count);
                }
                lengths[code] = len + count;
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

        private static IEnumerable<StatisticPoint> GetStatisticPoints(string srcPath, int start)
        {
            var entries = srcPath.ReadDailyStockEntries().ToList();
            var statisticPoints = entries.Suck(start, 1);

            return statisticPoints;
        }

        public static int ExportText(this IEnumerable<StatisticPoint> statisticPoints, string dstPath, 
            bool append)
        {
            using (var sw = new StreamWriter(dstPath, append))
            {
                var count = statisticPoints.Write(sw);
                return count;
            }
        }

        public static int ExportBinary(this IEnumerable<StatisticPoint> statisticPoints, string dstPath, bool append)
        {
            using (var fs = new FileStream(dstPath, append ? FileMode.Append : FileMode.Create))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    var count = statisticPoints.Write(bw);
                    return count;
                }
            }
        }

        public static int ExportBinaryAndText(this IEnumerable<StatisticPoint> statisticPoints, 
            string datPath, string txtPath, bool append)
        {
            using (var sw = new StreamWriter(txtPath, append))
            {
                using (var fs = new FileStream(datPath, append ? FileMode.Append : FileMode.Create))
                {
                    using (var bw = new BinaryWriter(fs))
                    {
                        var count = statisticPoints.Write(bw, sw);
                        return count;
                    }
                }
            }
        }

        /// <summary>
        ///  Writes a sequence of points in order to text
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tw"></param>
        /// <returns></returns>
        public static int Write(this IEnumerable<StatisticPoint> points, TextWriter tw)
        {
            var count = 0;
            foreach (var p in points)
            {
                Write(p, tw);
                count++;
            }
            return count;
        }

        public static int Write(this IEnumerable<StatisticPoint> points, BinaryWriter bw)
        {
            var count = 0;
            foreach (var p in points)
            {
                Write(p, bw);
                count++;
            }
            return count;
        }

        public static int Write(this IEnumerable<StatisticPoint> points, BinaryWriter bw, TextWriter tw)
        {
            var count = 0;
            foreach (var p in points)
            {
                Write(p, bw);
                Write(p, tw);
                count++;
            }
            return count;
        }

        #endregion
    }
}
