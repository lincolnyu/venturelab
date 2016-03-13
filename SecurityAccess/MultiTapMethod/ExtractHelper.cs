using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GaussianCore;
using GaussianCore.Generic;
using SecurityAccess.Asx;

namespace SecurityAccess.MultiTapMethod
{
    /// <summary>
    ///  extracts statistic points from raw stock data.
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
        ///  Converts raw sequences to statistic model model (points)
        /// </summary>
        /// <param name="dir">The directory the files are in</param>
        /// <param name="data">Time series data in chronical order (oldest first)</param>
        /// <param name="start">number of days from the first one that is late enough to make 
        /// statistic point to the day to start the processing with; 0 means starts from that first possible day</param>
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

        public static IEnumerable<ICore> LoadCores(BinaryReader br, int count, int start = 0, int step = 1)
        {
            const int inputLen = 22;
            const int outputLen = 6;
            const int chunkSize = sizeof(double) * (inputLen + outputLen);
            var offset = start * chunkSize;
            var stride = (step - 1) * chunkSize;
            br.BaseStream.Position = offset;
            for (var i = 0; i < count && br.BaseStream.Position  < br.BaseStream.Length; i++)
            {
                var core = new GaussianConfinedCore(22, 6);
                for (var j = 0; j < 22; j++)
                {
                    core.CentersInput[j] = br.ReadDouble();
                }
                for (var j = 0; j < 6; j++)
                {
                    core.CentersOutput[j] = br.ReadDouble();
                }
                yield return core;
                if (step > 1)
                {
                    br.BaseStream.Position += stride;
                }
            }
        }
        
        /// <summary>
        ///  Sucks only the input part
        /// </summary>
        /// <param name="data">Time series data in chronical order (oldest first)</param>
        /// <param name="start">The starting point relative to the start of the time series</param>
        /// <returns>The static point (which has only the input part updated by this method)</returns>
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

        /// <summary>
        ///  Process one data point
        /// </summary>
        /// <param name="data">The time series</param>
        /// <param name="start">The offset to the natural start of the time series</param>
        /// <returns></returns>
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

        /// <summary>
        ///  returns the logarithm of the input relative to the reverence value:
        /// </summary>
        /// <param name="v">The input</param>
        /// <param name="refval">The reference value</param>
        /// <returns>The logarithmic result</returns>
        public static double GetLogarithm(double v, double refval)
        {
            var res = (Math.Log(v) - Math.Log(refval)) / Math.Log(2);
            return res;
        }

        /// <summary>
        ///  Writes the specified static point to the textual file
        /// </summary>
        /// <param name="p">The static point to write</param>
        /// <param name="tw">The textual stream to write to</param>
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

        /// <summary>
        ///  Writes a static point to binarya file
        /// </summary>
        /// <param name="p">The point</param>
        /// <param name="bw">The binary stream</param>
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

        /// <summary>
        ///  generates the logarithmic input from data point (which contains original levels) for prediction
        /// </summary>
        /// <param name="p">The data point</param>
        /// <param name="input">The logarithmic input</param>
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

        public static IDictionary<string, int> GetLengths(string statisticsDir)
        {
            // The info file that contains the descriptions of existing data point files
            // mainly the number of existing data points of each data point file
            var infoFile = Path.Combine(statisticsDir, "_info.txt");
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
            return lengths;
        }

        public static void UpdateLengthFile(string statisticsDir, IDictionary<string, int> lengths)
        {
            var infoFile = Path.Combine(statisticsDir, "_info.txt");
            using (var sw = new StreamWriter(infoFile))
            {
                foreach (var kvp in lengths)
                {
                    sw.WriteLine("{0}:{1}", kvp.Key, kvp.Value);
                }
            }
        }


        /// <summary>
        ///  main entry that starts the process of converting the raw time-series data to data points
        /// </summary>
        /// <param name="srcDir">The source directory where all the time-series data is kept</param>
        /// <param name="dstDir">The target directory where data point files are kept</param>
        /// <param name="mode">binary, textual or both</param>
        /// <param name="logWriter">The writer that writes log file if needed</param>
        public static void ProcessFiles(this string srcDir, string dstDir,
            ExportModes mode = ExportModes.Binary, TextWriter logWriter = null)
        {
            var srcDirInfo = new DirectoryInfo(srcDir);
            var srcFiles = srcDirInfo.GetFiles();

            var lengths = GetLengths(dstDir);

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

            UpdateLengthFile(dstDir, lengths);

            logWriter.WriteLine("All processed.");
        }

        /// <summary>
        ///  gets the static points from the file startign from the specified offset (specified number
        ///  of possible data points)
        /// </summary>
        /// <param name="srcPath">The file that contains time-series data</param>
        /// <param name="start">The starting data point</param>
        /// <returns>The points</returns>
        private static IEnumerable<StatisticPoint> GetStatisticPoints(string srcPath, int start)
        {
            var entries = srcPath.ReadDailyStockEntries().ToList();
            var statisticPoints = entries.Suck(start, 1);

            return statisticPoints;
        }

        /// <summary>
        ///  writes the data points to textual file
        /// </summary>
        /// <param name="statisticPoints">The data points</param>
        /// <param name="dstPath">The path to the textual file</param>
        /// <param name="append">If it is to append to the file</param>
        /// <returns>The number of points written</returns>
        public static int ExportText(this IEnumerable<StatisticPoint> statisticPoints, string dstPath, 
            bool append)
        {
            using (var sw = new StreamWriter(dstPath, append))
            {
                var count = statisticPoints.Write(sw);
                return count;
            }
        }

        /// <summary>
        ///  Writes the data points to binary file
        /// </summary>
        /// <param name="statisticPoints">The data points</param>
        /// <param name="dstPath">The path to the textual file</param>
        /// <param name="append">If it is to append to the file</param>
        /// <returns>The number of points written</returns>
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

        /// <summary>
        ///  Exports the data points to both binary and text files
        /// </summary>
        /// <param name="statisticPoints">The data points</param>
        /// <param name="datPath">The binary file</param>
        /// <param name="txtPath">The text file</param>
        /// <param name="append">whether to append</param>
        /// <returns>The number of data points written</returns>
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
        ///  Writes data points to text file
        /// </summary>
        /// <param name="points">The data points</param>
        /// <param name="tw">The text to write to</param>
        /// <returns>The number of points written</returns>
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

        /// <summary>
        /// Writes data points to binary file
        /// </summary>
        /// <param name="points">The data points</param>
        /// <param name="bw">The binary file to write to</param>
        /// <returns>The number of points written</returns>
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

        /// <summary>
        ///  Writes data points to both text and binary file
        /// </summary>
        /// <param name="points">The data points</param>
        /// <param name="bw">The binary file to write to</param>
        /// <param name="tw">The text to write to</param>
        /// <returns>The number of points written</returns>
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
