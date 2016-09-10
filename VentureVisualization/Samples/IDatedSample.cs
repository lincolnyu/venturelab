using System;

namespace VentureVisualization.Samples
{
    public interface IDatedSample : ISample
    {
        DateTime Date { get; }
    }
}
