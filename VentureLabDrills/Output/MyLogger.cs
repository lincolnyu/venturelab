using QLogger.ConsoleHelpers;
using QLogger.Logging;
using System;

namespace VentureLabDrills.Output
{
    public class MyLogger : Logger
    {
        public enum Levels
        {
            None = 0,
            Error,
            Warning,
            Info,
            Verbose,
        }

        public Levels CurrentLevel { get; private set; }

        public Displayer Displayer { get; }

        public InplaceDisplayer InplaceDisplayer { get; private set; }

        public MyLogger(Levels displayLevel)
        {
            Writers.Add(Displayer = new Displayer(this) { ThresholdLevel = displayLevel, IsEnabled = true });
            SetupInplaceDisplayer();
        }

        private void SetupInplaceDisplayer()
        {
            InplaceDisplayer = new InplaceDisplayer(this) { DisplayLevel = Levels.Info, IsEnabled = false };
            var ipw = (InplaceWriter)InplaceDisplayer.Writer;
            ipw.MinRefreshInterval = TimeSpan.FromSeconds(1);
        }

        public void Write(Levels level, string msg)
        {
            lock(this)
            {
                CurrentLevel = level;
                Write(msg);
            }
        }

        public void Write(Levels level, string fmt, params object[] args)
        {
            lock(this)
            {
                CurrentLevel = level;
                Write(fmt, args);
            }
        }

        public void WriteLine(Levels level, string msg = "")
        {
            lock(this)
            {
                CurrentLevel = level;
                WriteLine(msg);
            }
        }

        public void WriteLine(Levels level, string fmt, params object[] args)
        {
            lock(this)
            {
                CurrentLevel = level;
                WriteLine(fmt, args);
            }
        }

        public void InplaceWrite(Levels level, string fmt, params object[] args)
        {
            var writer = (InplaceWriter)InplaceDisplayer.Writer;
            writer.WriteFormat(true, fmt, args);
        }

        public void LocateInplaceWrite()
        {
            var writer = (InplaceWriter)InplaceDisplayer.Writer;
            writer.RememberCursor();
        }

        public void InplaceWrite(Levels level, string msg)
        {
            var writer = (InplaceWriter)InplaceDisplayer.Writer;
            if (!writer.CanRefreshNow()) return;
            writer.RestoreCursor();
            writer.WriteFormat(true, msg);
            writer.UpdateLastRefreshTime();
        }
    }
}
