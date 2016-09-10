using System;

namespace VentureVisualization
{
    public interface IDatedSample : ISample
    {
        DateTime Date { get; }
    }
}
