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

        public double SquareInputDistance(IPoint other)
        {
            var sum = 0.0;
            for (var i = 0; i < Input.Count; i++)
            {
                var d = Input[i] - other.Input[i];
                var dd = d * d;
                sum += dd;
            }
            return sum;
        }

        public double SquareOutputDistance(IPoint other)
        {
            var sum = 0.0;
            for (var i = 0; i < Output.Count; i++)
            {
                var d = Output[i] - other.Output[i];
                var dd = d * d;
                sum += dd;
            }
            return sum;
        }

        public double SquareDistance(IPoint other)
        {
            return SquareInputDistance(other) + SquareOutputDistance(other);
        }

        public static double SquareInputDistance(IPoint point1, IPoint point2)
        {
            return point1.SquareInputDistance(point2);
        }

        public static double SquareOutputDistance(IPoint point1, IPoint point2)
        {
            return point1.SquareOutputDistance(point2);
        }

        public static double SquareDistance(IPoint point1, IPoint point2)
        {
            return point1.SquareDistance(point2);
        }
    }
}
