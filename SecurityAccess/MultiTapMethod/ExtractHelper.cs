using SecurityAccess.Asx;
using SecurityAccess.MultiTapMethod;
using System;
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

        public static double GetLogarithm(double v, double refval)
        {
            var res = (Math.Log(v) - Math.Log(refval)) / Math.Log(2);
            return res;
        }

        public static void Write(StatisticPoint p, TextWriter tw)
        {
            tw.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},"
                + "{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27}",
                GetLogarithm(p.P1O, p.P1C),
                GetLogarithm(p.P1H, p.P1C),
                GetLogarithm(p.P1L, p.P1C),
                GetLogarithm(p.P2, p.P1C),
                GetLogarithm(p.P3, p.P1C),
                GetLogarithm(p.P4, p.P1C),
                GetLogarithm(p.P5, p.P1C),
                GetLogarithm(p.P10, p.P1C),
                GetLogarithm(p.P20, p.P1C),
                GetLogarithm(p.P65, p.P1C),
                GetLogarithm(p.P130, p.P1C),
                GetLogarithm(p.P260, p.P1C),
                GetLogarithm(p.P520, p.P1C),
                GetLogarithm(p.P1300, p.P1C),
                GetLogarithm(p.V2, p.V1),
                GetLogarithm(p.V3, p.V1),
                GetLogarithm(p.V4, p.V1),
                GetLogarithm(p.V5, p.V1),
                GetLogarithm(p.V10, p.V1),
                GetLogarithm(p.V20, p.V1),
                GetLogarithm(p.V65, p.V1),
                GetLogarithm(p.V260, p.V1),
                GetLogarithm(p.FP1, p.P1C),
                GetLogarithm(p.FP2, p.P1C),
                GetLogarithm(p.FP5, p.P1C),
                GetLogarithm(p.FP10, p.P1C),
                GetLogarithm(p.FP20, p.P1C),
                GetLogarithm(p.FP65, p.P1C));
        }

        private static void Write(StatisticPoint p, BinaryWriter bw)
        {
            bw.Write(GetLogarithm(p.P1O, p.P1C));
            bw.Write(GetLogarithm(p.P1H, p.P1C));
            bw.Write(GetLogarithm(p.P1L, p.P1C));
            bw.Write(GetLogarithm(p.P2, p.P1C));
            bw.Write(GetLogarithm(p.P3, p.P1C));
            bw.Write(GetLogarithm(p.P4, p.P1C));
            bw.Write(GetLogarithm(p.P5, p.P1C));
            bw.Write(GetLogarithm(p.P10, p.P1C));
            bw.Write(GetLogarithm(p.P20, p.P1C));
            bw.Write(GetLogarithm(p.P65, p.P1C));
            bw.Write(GetLogarithm(p.P130, p.P1C));
            bw.Write(GetLogarithm(p.P260, p.P1C));
            bw.Write(GetLogarithm(p.P520, p.P1C));
            bw.Write(GetLogarithm(p.P1300, p.P1C));
            bw.Write(GetLogarithm(p.V2, p.V1));
            bw.Write(GetLogarithm(p.V3, p.V1));
            bw.Write(GetLogarithm(p.V4, p.V1));
            bw.Write(GetLogarithm(p.V5, p.V1));
            bw.Write(GetLogarithm(p.V10, p.V1));
            bw.Write(GetLogarithm(p.V20, p.V1));
            bw.Write(GetLogarithm(p.V65, p.V1));
            bw.Write(GetLogarithm(p.V260, p.V1));

            bw.Write(GetLogarithm(p.FP1, p.P1C));
            bw.Write(GetLogarithm(p.FP2, p.P1C));
            bw.Write(GetLogarithm(p.FP5, p.P1C));
            bw.Write(GetLogarithm(p.FP10, p.P1C));
            bw.Write(GetLogarithm(p.FP20, p.P1C));
            bw.Write(GetLogarithm(p.FP65, p.P1C));
        }

        public static void GenerateInput(this StatisticPoint p, IList<double> input)
        {
            input[0] = GetLogarithm(p.P1O, p.P1C);
            input[1] = GetLogarithm(p.P1H, p.P1C);
            input[2] = GetLogarithm(p.P1L, p.P1C);
            input[3] = GetLogarithm(p.P2, p.P1C);
            input[4] = GetLogarithm(p.P3, p.P1C);
            input[5] = GetLogarithm(p.P4, p.P1C);
            input[6] = GetLogarithm(p.P5, p.P1C);
            input[7] = GetLogarithm(p.P10, p.P1C);
            input[8] = GetLogarithm(p.P20, p.P1C);
            input[9] = GetLogarithm(p.P65, p.P1C);
            input[10] = GetLogarithm(p.P130, p.P1C);
            input[11] = GetLogarithm(p.P260, p.P1C);
            input[12] = GetLogarithm(p.P520, p.P1C);
            input[13] = GetLogarithm(p.P1300, p.P1C);
            input[14] = GetLogarithm(p.V2, p.V1);
            input[15] = GetLogarithm(p.V3, p.V1);
            input[16] = GetLogarithm(p.V4, p.V1);
            input[17] = GetLogarithm(p.V5, p.V1);
            input[18] = GetLogarithm(p.V10, p.V1);
            input[19] = GetLogarithm(p.V20, p.V1);
            input[20] = GetLogarithm(p.V65, p.V1);
            input[21] = GetLogarithm(p.V260, p.V1);
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
                string code, ext;
                 if (!FileReorganiser.IsCodeFile(srcFile.Name, out code, out ext) && ext != ".txt")
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
                            count = statisticPoints.ExportBinary(dstPath, append);
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
            int originalCount = 0;
            if (append)
            {
                using (var fs = new FileStream(dstPath, FileMode.Open))
                {
                    using (var br = new BinaryReader(fs))
                    {
                        br.ReadUInt32(); // skip the flag
                        originalCount = br.ReadInt32();
                    }
                }
            }

            using (var fs = new FileStream(dstPath, append ? FileMode.Append : FileMode.Create))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    // NOTE it's fc - i/o only compatible
                    if (!append)
                    {
                        bw.Write((uint)FixedConfinedBuilder.Flags.InputOutputOnly);
                        // place holder for counter
                        bw.Write(0);
                    }

                    var count = statisticPoints.Write(bw);

                    bw.Seek(sizeof(uint), SeekOrigin.Begin);
                    bw.Write(originalCount + count);

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
