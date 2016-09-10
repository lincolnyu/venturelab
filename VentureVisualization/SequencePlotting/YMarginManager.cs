using System;
using System.Collections.Generic;

namespace VentureVisualization.SequencePlotting
{
    public class YMarginManager
    {
        public delegate void GetOverallMinMaxDelegate(out double min, out double max);

        /// <summary>
        ///  Drawing modes
        /// </summary>
        /// <remarks>
        ///  Specified times:
        ///    
        ///  As-is:
        ///    When start time, count specified, end time is deduced
        ///    When end time, count specified, start time is deduced
        /// </remarks>
        public enum VerticalModes
        {
            YRange,
            YMargins,
            YMarginsFull,
        }

        #region Default values

        public const VerticalModes DefaultVerticalMode = VerticalModes.YMargins;
        public const double DefaultTopMarginRatio = 0.3;
        public const double DefaultBottomMarginRatio = 0.3;

        #endregion

        private bool _yMinMaxValueScanned;
        private double _yMaxValue;
        private double _yMinValue;
        private VerticalModes _verticalMode = DefaultVerticalMode;

        public VerticalModes VertialMode
        {
            get { return _verticalMode; }
            set
            {
                if (_verticalMode == value) return;
                UpdateVerticalSettings();

            }
        }

        /// <summary>
        ///  Value of Y on the upper boundary
        /// </summary>
        /// <remarks>
        ///  It is updated when re-drawn if the vertical mode is not YRange 
        ///  or it is specified by the user
        /// </remarks>
        public double YMax { get; set; }

        /// <summary>
        ///  Value of Y on the lower boundary
        /// </summary>
        /// <remarks>
        ///  It is updated when re-drawn if the vertical mode is not YRange 
        ///  or it is specified by the user
        /// </remarks>
        public double YMin { get; set; }

        /// <summary>
        ///  Top margin vs (max-min)
        /// </summary>
        public double TopMarginRatio { get; set; } = DefaultTopMarginRatio;

        /// <summary>
        ///  Bottom margin vs (max-min)
        /// </summary>
        public double BottomMarginRatio { get; set; } = DefaultBottomMarginRatio;

        /// <summary>
        ///  Overall max
        /// </summary>
        public double YMaxValue
        {
            get
            {
                ScanMinMaxYValues();
                return _yMaxValue;
            }
        }

        /// <summary>
        ///  Overall min
        /// </summary>
        public double YMinValue
        {
            get
            {
                ScanMinMaxYValues();
                return _yMinValue;
            }
        }

        /// <summary>
        ///  Min value of Y on the screen
        /// </summary>
        public double? ScreenMin { get; private set; }

        /// <summary>
        ///  Max value of Y on the screen
        /// </summary>
        public double? ScreenMax { get; private set; }

        public HashSet<GetOverallMinMaxDelegate> GetOverallMinMax { get; } = new HashSet<GetOverallMinMaxDelegate>();

        public void ResetMinMax()
        {
            ScreenMin = null;
            ScreenMax = null;
        }

        public void UpdateMin(double min)
        {
            if (ScreenMin == null || min < ScreenMin) ScreenMin = min;
        }

        public void UpdateMax(double max)
        {
            if (ScreenMax == null || max > ScreenMax) ScreenMax = max;
        }

        private void ScanMinMaxYValues()
        {
            if (_yMinMaxValueScanned) return;

            _yMinValue = double.MaxValue;
            _yMaxValue = double.MinValue;
            foreach (var getMinMax in GetOverallMinMax)
            {
                double low, high;
                getMinMax(out low, out high);
                if (low < _yMinValue) _yMinValue = low;
                if (high > _yMaxValue) _yMaxValue = high;
            }
            
            _yMinMaxValueScanned = true;
        }

        private void GetYMinMaxForMargins(double min, double max, out double newMin, out double newMax)
        {
            newMax = max + (max - min) * TopMarginRatio;
            newMin = min - (max - min) * BottomMarginRatio;
        }

        public void UpdateVerticalSettings()
        {
            double newMin, newMax;
            switch (_verticalMode)
            {
                case VerticalModes.YMargins:
                    GetYMinMaxForMargins(ScreenMin.Value, ScreenMax.Value, out newMin, out newMax);
                    YMin = newMin;
                    YMax = newMax;
                    break;
                case VerticalModes.YMarginsFull:
                    GetYMinMaxForMargins(YMinValue, YMaxValue, out newMin, out newMax);
                    YMin = newMin;
                    YMax = newMax;
                    break;
                default:
                    break;
            }
        }
    }
}
