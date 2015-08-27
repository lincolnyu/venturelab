using System;
using System.Collections.Generic;

namespace GaussianCore
{
    public class Core
    {
        #region Properties

        public const double Attenuation = 0.5;

        public IList<Component> Components { get; } = new List<Component>();

        public double AInput { get; set; }

        /// <summary>
        ///  The output component of A
        /// </summary>
        public double AOutput { get; set; }

        public int Multiple { get; set; } = 1;

        public Core ClosestNeighbour { get; set; }

        public double MinimumOutputSquareDistance { get; set; }

        public double MinimumInputSquareDistance { get; set; }

        public IList<double> OutputOffsets { get; set; }

        #endregion

        #region Methods

        public double GetSquareDistanceEuclid(Core other)
        {
            return GetSquareDistanceEuclid(other, 0, Components.Count);
        }

        public IList<double> GetVector(Core other, int start, int end)
        {
            var result = new List<double>(end-start);
            for (var i = start; i < end; i++)
            {
                var d = other.Components[i].Center - Components[i].Center;
                result.Add(d);
            }
            return result;
        }

        public double GetSquareDistanceEuclid(Core other, int start, int end)
        {
            var sum = 0.0;
            for (var i = start; i < end; i++)
            {
                var c1 = Components[i].Center;
                var c2 = other.Components[i].Center;
                var d = c2 - c1;
                d *= d;
                sum += d;
            }
            return sum;
        }

        public void UpdateCoefficients(int outputStartFrom)
        {
            var invPi = 1/Math.PI;
            var linput = Math.Log(Attenuation) / MinimumInputSquareDistance;
            for (var i = 0; i < outputStartFrom; i++)
            {
                Components[i].L = linput;
            }
            // linput < 0
            AInput = Math.Pow(-linput*invPi, outputStartFrom / 2.0);

            var outputCount = Components.Count - outputStartFrom;
            AOutput = Math.Pow(invPi, outputCount / 2.0);
            for (var i = outputStartFrom; i < Components.Count; i++)
            {
                var sqoo = OutputOffsets[i - outputStartFrom];
                sqoo *= sqoo;
                var loutput = Math.Log(Attenuation) / sqoo;
                Components[i].L = loutput;
                AOutput *= Math.Sqrt(-loutput) ;
            }
        }

        public double GetLeadingIntensity(IList<double> inputs)
        {
            var result = Multiple * AInput;
            for (var i = 0; i < inputs.Count; i++)
            {
                var dx = inputs[i] - Components[i].Center;
                var sqdx = dx * dx;
                var e = Math.Exp(Components[i].L * sqdx);
                result *= e;
            }
            return result;
        }

        public double GetIntensity(IList<double> inoutputs)
        {
            var result = Multiple * AInput * AOutput;
            for (var i = 0; i < inoutputs.Count; i++)
            {
                var dx = inoutputs[i] - Components[i].Center;
                var sqdx = dx * dx;
                var e = Math.Exp(Components[i].L * sqdx);
                result *= e;
            }
            return result;
        }

        #endregion
    }
}
