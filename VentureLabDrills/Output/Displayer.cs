using QLogger.Logging;
using System;

namespace VentureLabDrills.Output
{
    public class Displayer : FileWriter
    {
        public Displayer(MyLogger logger) : base(logger, Console.Out)
        {
        }
    }
}
