using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SecurityAccess.Asx
{
    public class FileReorganiser
    {
        #region Enumerations

        public enum InteroplationMode
        {
            None,
            Linear,
            Extend,
        }

        #endregion

        #region Nested classes

        public class OutputFile
        {
            public StreamWriter Writer { get; set; }

            public int MissingDays { get; set; }
            public DailyStockEntry LastAvailable { get; set; }

            public int TotalCount { get; set; }

            public void WriteDailyStockEntry(DailyStockEntry dse)
            {
                TotalCount++;
                Writer.WriteDailyStockEntry(dse);
            }

            public void WriteDailyStockEntry(string code, DateTime date, 
                double open, double high, double low, double close, double volume)
            {
                TotalCount++;
                Writer.WriteDailyStockEntry(code, date, open, high, low, close, volume);
            }
        }

        #endregion

        #region Constructors

        public FileReorganiser(TextWriter logWriter = null)
        {
            LogWriter = logWriter;
        }

        #endregion

        #region Properties

        public Dictionary<string, OutputFile> OutputFiles { get; }
            = new Dictionary<string, OutputFile>();

        public TextWriter LogWriter {get;}

        #endregion

        #region Methods

        public void Reorganise(string dirStr, string outDir, bool append=false, 
            InteroplationMode mode = InteroplationMode.None)
        {
            if (LogWriter != null)
            {
                LogWriter.WriteLine("Reorganising...");
            }

            var dir = new DirectoryInfo(dirStr);
            var files = dir.GetFiles();

            // only used for interpolation
            var dates = new List<DateTime>();

            foreach (var file in files.OrderBy(x => x.Name))
            {
                if (!file.Name.IsAsxHistoryFile())
                {
                    continue;
                }

                if (LogWriter != null)
                {
                    LogWriter.WriteLine("Processing {0}...", file.Name);
                }

                if (mode != InteroplationMode.None)
                {
                    var dt = file.Name.GetDateOfFile();
                    dates.Add(dt);
                }

                var dsds = file.FullName.ReadDailyStockEntries();
                foreach (var dsd in dsds)
                {
                    OutputFile outFile;
                    if (!OutputFiles.TryGetValue(dsd.Code, out outFile))
                    {
                        var outFilePath = outDir.GetOutputFileName(dsd.Code);
                        outFile = new OutputFile
                        {
                            Writer = new StreamWriter(outFilePath, append)
                        };
                        OutputFiles[dsd.Code] = outFile;
                    }

                    if (mode != InteroplationMode.None && outFile.MissingDays > 0)
                    {
                        // TODO
                        Interpolate(mode, dates, outFile, dsd);
                    }

                    outFile.MissingDays = -1;
                    outFile.LastAvailable = dsd;
                    outFile.WriteDailyStockEntry(dsd);
                }

                if (mode != InteroplationMode.None)
                {
                    foreach (var val in OutputFiles.Values)
                    {
                        if (val.MissingDays != -1)
                        {
                            val.MissingDays++;
                        }
                        else
                        {
                            val.MissingDays = 0;
                        }
                    }
                }
            }

            foreach (var outFile in OutputFiles.Values)
            {
                if (mode != InteroplationMode.None && outFile.MissingDays > 0)
                {
                    Interpolate(mode, dates, outFile, null);
                    outFile.MissingDays = 0;
                }
                outFile.Writer.Close();
            }

            if (LogWriter != null)
            {
                LogWriter.WriteLine("Reorganisation done");
            }
        }

        private void Interpolate(InteroplationMode mode, IList<DateTime> dates, OutputFile outFile, DailyStockEntry dsd)
        {
            switch (mode)
            {
                case InteroplationMode.Linear:
                    {
                        for (var i = 0; i < outFile.MissingDays; i++)
                        {
                            var r2 = (i + 1) / (outFile.MissingDays + 1);
                            var r1 = 1 - r2;
                            var close = outFile.LastAvailable.Close * r1 + dsd.Open * r2;
                            var open = close;
                            var low = close;
                            var high = close;
                            outFile.WriteDailyStockEntry(dsd.Code, dates[dates.Count - outFile.MissingDays + i - 1],
                                open, high, low, close, 0.0);
                        }
                        break;
                    }
                case InteroplationMode.Extend:
                    {
                        for (var i = 0; i < outFile.MissingDays; i++)
                        {
                            var close = outFile.LastAvailable.Close;
                            var open = close;
                            var low = close;
                            var high = close;
                            outFile.WriteDailyStockEntry(dsd.Code, dates[dates.Count - outFile.MissingDays + i - 1],
                                open, high, low, close, 0.0);
                        }
                        break;
                    }
            }
        }

        #endregion
    }
}
