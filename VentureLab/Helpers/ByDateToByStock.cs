using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VentureLab.Helpers
{
    public class ByDateToByStock
    {
        #region Delegates

        public delegate void ProgressUpdatedEventHandler(string inputFileName);

        #endregion

        #region Nested types

        private class CodeBuffer
        {
            private StringBuilder _bufferedLines = new StringBuilder();

            public CodeBuffer(bool append)
            {
                Append = append;
            }

            public int LineCount { get; private set; }
            public bool Append { get; set; }

            public void AddLine(string line)
            {
                _bufferedLines.AppendLine(line);
                LineCount++;
            }

            public string Flush()
            {
                var s = _bufferedLines.ToString();
                _bufferedLines.Clear();
                LineCount = 0;
                return s;
            }
        }

        #endregion

        #region Fields

        public const int DefaultMaxBuffered = 1024 * 1024;

        public static readonly HashSet<string> SystemFiles = new HashSet<string>
        {
            "AUX", "PRN"
        };

        private Dictionary<string, CodeBuffer> _codes = new Dictionary<string, CodeBuffer>();

        private int _totalBufferedLines = 0;

        #endregion

        #region Constructors

        public ByDateToByStock(IEnumerable<FileInfo> byDateInputs, string outputDir, bool append = true, int maxBuffered = DefaultMaxBuffered)
        {
            ByDateInputs = byDateInputs;
            OutputDir = outputDir;
            Append = append;
            MaxBufferedLines = maxBuffered;
        }

        public ByDateToByStock(IEnumerable<FileInfo> byDateInputs, string outputDir, ProgressUpdatedEventHandler progressCb, bool append = true, int maxBuffered = DefaultMaxBuffered) : this(byDateInputs, outputDir, append, maxBuffered)
        {
            ProgressUpdated += progressCb;
        }

        #endregion

        #region Properties

        #region Configs

        public IEnumerable<FileInfo> ByDateInputs { get; }
        public string OutputDir { get; }
        public bool Append { get; }
        public int MaxBufferedLines { get; }

        #endregion

        #region Results

        public int LinesSuccessful { get; private set; }
        public int LinesFailed { get; private set; }

        #endregion

        #endregion

        #region Events

        public event ProgressUpdatedEventHandler ProgressUpdated;

        #endregion

        #region Methods

        public ByDateToByStock Run()
        {
            foreach (var input in ByDateInputs)
            {
                using (var sr = input.OpenText())
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        if (line == null) break;
                        var code = AsxFileHelper.GetCodeOfLine(line);
                        if (code != null)
                        {
                            CodeBuffer cb;
                            if (!_codes.TryGetValue(code, out cb))
                            {
                                _codes.Add(code, cb = new CodeBuffer(Append));
                            }
                            cb.AddLine(line);
                            _totalBufferedLines++;
                            if (_totalBufferedLines > MaxBufferedLines)
                            {
                                Flush();
                            }
                        }
                        else
                        {
                            LinesFailed++;
                        }
                    }
                }
                ProgressUpdated?.Invoke(input.FullName);
            }
            if (_totalBufferedLines > 0)
            {
                Flush();
            }
            return this;
        }

        private void Flush()
        {
            foreach (var kvp in _codes)
            {
                var code = kvp.Key;
                var cb = kvp.Value;
                using (var codeFile = GetCodeFile(OutputDir, code, cb.Append))
                {
                    LinesSuccessful += cb.LineCount;
                    codeFile.Write(cb.Flush());
                }
                cb.Append = true;
            }
            _totalBufferedLines = 0;
        }

        private static StreamWriter GetCodeFile(string outputDir, string code, bool append)
        {
            if (SystemFiles.Contains(code.ToUpper()))
            {
                code += "_";
            }
            var file = Path.Combine(outputDir, code + ".txt");
            try
            {
                if (append) TrimFile(file);
                return new StreamWriter(file, append);
            }
            catch (Exception)
            {
                Console.WriteLine($"error opening file {file}");
                throw;
            }
        }

        private static void TrimFile(string file)
        {
            if (!File.Exists(file)) return;
            long trimPos = 0;
            using (var fs = new FileStream(file, FileMode.Open))
            {
                fs.Seek(0, SeekOrigin.End);
                for (; fs.Position >= 0; fs.Position-=2)
                {
                    var c = (char)fs.ReadByte();
                    if (!char.IsWhiteSpace(c))
                    {
                        break;
                    }
                }
                fs.SetLength(trimPos);
            }
            if (trimPos > 0)
            {
                using (var sw = new StreamWriter(file, true))
                {
                    sw.WriteLine();
                }
            }
        }

        #endregion
    }
}
