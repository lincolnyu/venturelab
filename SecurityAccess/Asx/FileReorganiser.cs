using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SecurityAccess.Asx
{
    public class FileReorganiser
    {
        #region Enumerations

        public enum InterpolationModes
        {
            None,
            Linear,
            Extend,
        }

        #endregion

        #region Nested classes

        public class OutputFile
        {
            public OutputFile(FileReorganiser owner, string code)
            {
                Owner = owner;
                Code = code;
                Written = false;
            }

            public bool Written { get; private set; }

            public FileReorganiser Owner { get; }

            public string Code { get; }

            public int MissingDays { get; set; }
            public DailyStockEntry LastAvailable { get; set; }

            public int TotalCount { get; set; }

            public void WriteDailyStockEntry(DailyStockEntry dse)
            {
                var writer = Owner.GetWriter(this, Written);
                Written = true;
                TotalCount++;
                writer.WriteDailyStockEntry(dse);
            }

            public void WriteDailyStockEntry(string code, DateTime date, 
                double open, double high, double low, double close, double volume)
            {
                var writer = Owner.GetWriter(this, Written);
                Written = true;
                TotalCount++;
                writer.WriteDailyStockEntry(code, date, open, high, low, close, volume);
            }
        }

        public class OutputFileInfo
        {
            public StreamWriter Writer { get; set; }
            public int Ageing { get; set; }
        }

        #endregion

        #region Constructors

        public FileReorganiser(string inputDir, string outputDir, 
            TextWriter logWriter = null, bool append=false, 
            InterpolationModes mode = InterpolationModes.None)
        {
            InputDir = inputDir;
            OutputDir = outputDir;
            LogWriter = logWriter;
            Append = false;
            Mode = mode;
        }

        #endregion

        #region Properties

        public string InputDir { get; }

        public string OutputDir { get; }

        public bool Append { get; }

        public InterpolationModes Mode { get; }  

        public int MaxAgeing { get; set; } = 10;

        /// <summary>
        ///  Map code to output file 
        /// </summary>
        public Dictionary<string, OutputFile> OutputFiles { get; }
            = new Dictionary<string, OutputFile>();

        public Dictionary<OutputFile, OutputFileInfo> WriterPool { get; }
            = new Dictionary<OutputFile, OutputFileInfo>();

        public TextWriter LogWriter {get;}

        #endregion

        #region Methods

        public static bool IsCodeFile(string fn, out string code)
        {
            var ext = Path.GetExtension(fn);
            if (!ext.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                code = null;
                return false;
            }
            code = Path.GetFileNameWithoutExtension(fn);
            return (code.Length == 3);
        }

        public void Reorganise()
        {
            if (LogWriter != null)
            {
                LogWriter.WriteLine("Reorganising...");
            }

            var dir = new DirectoryInfo(InputDir);
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

                if (Mode != InterpolationModes.None)
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
                        var outFilePath = OutputDir.GetOutputFileName(dsd.Code);
                        outFile = new OutputFile(this, dsd.Code);
                        OutputFiles[dsd.Code] = outFile;
                    }
                    
                    if (Mode != InterpolationModes.None && outFile.MissingDays > 0)
                    {
                        // TODO
                        Interpolate(Mode, dates, outFile, dsd);
                    }

                    outFile.MissingDays = -1;
                    outFile.LastAvailable = dsd;
                    outFile.WriteDailyStockEntry(dsd);
                }

                if (Mode != InterpolationModes.None)
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

                AgeWriterPool();
            }

            if (LogWriter != null)
            {
                LogWriter.WriteLine("Finalising files...");
            }

            foreach (var outFile in OutputFiles.Values)
            {
                if (Mode != InterpolationModes.None && outFile.MissingDays > 0)
                {
                    Interpolate(Mode, dates, outFile, null);
                    outFile.MissingDays = 0;
                }
            }

            CloseWriters();

            if (LogWriter != null)
            {
                LogWriter.WriteLine("Reorganisation done.");
            }
        }

        private void AgeWriterPool()
        {
            var listToDel = new List<KeyValuePair<OutputFile, OutputFileInfo>>();

            foreach (var outfp in WriterPool)
            {
                outfp.Value.Ageing++;

                var val = outfp.Value;
                if (val.Ageing > MaxAgeing)
                {
                    listToDel.Add(outfp);
                }
            }

            foreach (var todel in listToDel)
            {
                todel.Value.Writer.Close();
                WriterPool.Remove(todel.Key);
            }
        }

        private StreamWriter GetWriter(OutputFile outFile, bool written)
        {
            // age all
            OutputFileInfo ofi = null;
            if (WriterPool.TryGetValue(outFile, out ofi))
            {
                ofi.Ageing = 0;
                return ofi.Writer;
            }
            
            var fn = OutputDir.GetOutputFileName(outFile.Code);
            //if (LogWriter != null)
            //{
            //    LogWriter.WriteLine("(Debug Info) Opeing '{0}'", outFile.Code);
            //}
            ofi = new OutputFileInfo
            {
                Writer = new StreamWriter(fn, written || Append)
            };
            WriterPool[outFile] = ofi;
            return ofi.Writer;
        }

        private void CloseWriters()
        {
            foreach (var wpp in WriterPool)
            {
                //if (LogWriter != null)
                //{
                //    LogWriter.WriteLine("(Debug Info) Closing '{0}'", wpp.Key.Code);
                //}
                wpp.Value.Writer.Close();
            }
        }

        private void Interpolate(InterpolationModes mode, IList<DateTime> dates, OutputFile outFile, DailyStockEntry dsd)
        {
            switch (mode)
            {
                case InterpolationModes.Linear:
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
                case InterpolationModes.Extend:
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
