using System;

namespace VentureVisualization.Samples
{
    public interface IDatedSample : VentureVisualization.ISample
    {
        DateTime Date { get; }
    }
}
