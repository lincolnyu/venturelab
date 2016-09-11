namespace VentureVisualization
{
    public interface ISample
    {
        double Step { get; }

        /// <summary>
        ///  Used as step from base for the first sample in the view 
        /// </summary>
        double Offset { get; }
    }
}
