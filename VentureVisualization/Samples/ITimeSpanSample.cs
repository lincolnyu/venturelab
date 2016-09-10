using System;

namespace VentureVisualization.Samples
{
    public interface ITimeSpanSample : ISample
    {
        TimeSpan TimeSpan { get; }
    }
}