using System.Collections.Generic;

namespace VentureLab.QbGaussianMethod.Cores
{
    public class Point : IPoint
    {
        public Point(int inputLen, int outputLen)
        {
            Input = new double[inputLen];
            Output = new double[outputLen];
        }

        public IList<double> Input { get; private set; }
        public IList<double> Output { get; private set; }

        public int InputLength => Input.Count;
        public int OutputLength => Output.Count;
    }
}
