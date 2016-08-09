using QLogger.ConsoleHelpers;
using QLogger.Logging;

namespace VentureLabDrills.Output
{
    public class InplaceDisplayer : Logger.WriterWrapper
    {
        public InplaceDisplayer(MyLogger logger) : base(InplaceWriter.Instance)
        {
            Logger = logger;
        }

        public MyLogger Logger { get; }

        public MyLogger.Levels DisplayLevel { get; set; }

        public bool IsEnabled { get; set; }

        public override bool IsActive => Logger.CurrentLevel <= DisplayLevel && IsEnabled;
    }
}
