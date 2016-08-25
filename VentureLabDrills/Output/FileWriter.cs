using QLogger.Logging;
using System;
using System.IO;

namespace VentureLabDrills.Output
{
    public class FileWriter : Logger.WriterWrapper, IDisposable
    {
        public FileWriter(MyLogger logger, TextWriter tw) : base(tw)
        {
            Logger = logger;
        }

        public MyLogger Logger { get; }

        public MyLogger.Levels ThresholdLevel { get; set; }

        public bool IsEnabled { get; set; }

        public override bool IsActive => Logger.CurrentLevel <= ThresholdLevel && IsEnabled;

        public void Dispose()
        {
            ((TextWriter)Writer).Dispose();
        }
    }
}
