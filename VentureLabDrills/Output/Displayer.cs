using QLogger.Logging;
using System;

namespace VentureLabDrills.Output
{
    public class Displayer : Logger.WriterWrapper
    {
        public Displayer(MyLogger logger) : base(Console.Out)
        {
            Logger = logger;
        }

        public MyLogger Logger { get; }

        public MyLogger.Levels DisplayLevel { get; set; }

        public bool IsEnabled { get; set; }

        public override bool IsActive => Logger.CurrentLevel <= DisplayLevel && IsEnabled;
    }
}
