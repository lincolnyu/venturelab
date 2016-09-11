namespace VentureVisualization.Samples
{
    public interface IFutureSample : ISample
    {
        /// <summary>
        ///  Number of days from the last record sample
        /// </summary>
        int Days { get; }
    }
}
