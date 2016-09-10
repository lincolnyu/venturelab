using System;
using VentureClient.Commands;
using VentureClient.Models;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using VentureVisualization.SequencePlotting;
using VentureVisualization.OtherPlotting;
using VentureClient.Interfaces;
using static VentureVisualization.SequencePlotting.CandleChartPlotter;
using VentureVisualization.Samples;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VentureClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IChartNavigator
    {
        public const double ZoomBase = 1.25;
        public const int MinZoomLevel = -5;
        public const int MaxZoomLevel = 5;

        private const double MarginLeft = 50;
        private const double MarginRight = 30;
        private const double MarginY = 30;
        private const double MajorBarRatio = 0.6;
        private const double MajorBarWidth = MarginLeft * MajorBarRatio;
        private const double DateBarRatio = 0.4;
        private const double DateBarHeight = MarginY * DateBarRatio;
        private const double VolumeChartHeightRatio = 0.1;

        private Expert _expert;
        private Stock _stock;

        private CandleChartPlotter _candlePlotter;
        private VolumePlotter _volumePlotter;
        private PriceRuler _priceRuler;
        private TimeRuler _timeRuler;
        private StockAndPredictionSequencer _sequencer;
        private PredictionPlotter _predictPlotter;

        private int _startIndex = 0;
        private int _zoomLevel = 0;

        #region Frame wide variables for plotting

        private int _lastYear = int.MinValue;
        
        #endregion 

        #region Reletaively unchanged drawing styles

        private Brush _blackBrush = new SolidColorBrush(Colors.Black);

        #endregion

        #region Current drawing context

        private double _volumeChartYOffset;
        private double _candleChartYOffset;
        private double _ChartXOffset;
        private double _volumeChartHeight;
        private double _candleChartHeight;
        private YMarginManager _yMarginManager;

        #endregion

        public MainPage()
        {
            InitializeComponent();
            SetupModels();
            SetupCommands();
            DataContext = this;
        }

        public OpenExpertFileCommand OpenExpertFileCommand { get; private set; }
        public OpenStockFileCommand OpenStockFileCommand { get; private set; }
        public GoRightCommand GoRightCommand { get; private set; }
        public GoLeftCommand GoLeftCommand { get; private set; }
        public ZoomInCommand ZoomInCommand { get; private set; }
        public ZoomOutCommand ZoomOutCommand { get; private set; }

        #region IChartNavigator members

        public bool CanGoLeft => _candlePlotter != null && _startIndex > 0;

        public bool CanGoRight => _candlePlotter != null && _startIndex + _sequencer.Length < _stock.Data.Count;

        public bool CanZoomIn => _zoomLevel < MaxZoomLevel;
        public bool CanZoomOut => _zoomLevel > MinZoomLevel;

        public event EventHandler CanGoLeftChanged;
        public event EventHandler CanGoRightChanged;
        public event EventHandler CanZoomInChanged;
        public event EventHandler CanZoomOutChanged;

        #endregion

        #region Methods

        #region IChartNavigator members

        public void GoLeft(int step)
        {
            _startIndex -= step;
            if (_startIndex < 0) _startIndex = 0;

            ReDraw();

            FireCanGoLeftRightChanged();
        }

        public void GoRight(int step)
        {
            _startIndex += step;
            if (_startIndex + _sequencer.Length >= _stock.Data.Count) _startIndex = (int)Math.Ceiling((_stock.Data.Count - _sequencer.Length));

            ReDraw();

            FireCanGoLeftRightChanged();
        }

        public void ZoomIn()
        {
            _zoomLevel++;
            if (_zoomLevel > MaxZoomLevel)
            {
                _zoomLevel = MaxZoomLevel;
            }
            ReDraw();
            FireCanZoomInOutChanged();
            FireCanGoLeftRightChanged();
        }

        public void ZoomOut()
        {
            _zoomLevel--;
            if (_zoomLevel < MinZoomLevel)
            {
                _zoomLevel = MinZoomLevel;
            }
            ReDraw();
            FireCanZoomInOutChanged();
            FireCanGoLeftRightChanged();
        }

        #endregion

        private void SetupModels()
        {
            _expert = new Expert();
            _stock = new Stock();
            _stock.StockUpdated += OnStockUpdated;
        }

        private void OnStockUpdated()
        {
            StockCode.Text = _stock.Code;

            _startIndex = 0;
            _zoomLevel = 0;
            // TODO ...
            _sequencer = new StockAndPredictionSequencer(_stock.Data, new PredictionSample[0]);
            _sequencer.PreDrawDone += SequencerOnPreDrawDone;
            _yMarginManager = new YMarginManager { VertialMode = YMarginManager.VerticalModes.YMargins };
            _candlePlotter = new CandleChartPlotter();
            _candlePlotter.YMarginManager = _yMarginManager;
            _volumePlotter = new VolumePlotter();
            _priceRuler = new PriceRuler(_candlePlotter);
            _timeRuler = new TimeRuler();
            _predictPlotter = new PredictionPlotter(_candlePlotter);
            _candlePlotter.DrawCandle += DrawCandle;
            _candlePlotter.DrawEnd += _priceRuler.Draw;
            _volumePlotter.DrawVolume += DrawVolume;
            _timeRuler.DrawDatePeg += DrawDatePeg;
            _candlePlotter.Subscribe(_sequencer);
            _volumePlotter.Subscribe(_sequencer);
            _timeRuler.Subscribe(_sequencer);
            _predictPlotter.Subscribe(_sequencer);
            _priceRuler.DrawMajor += PriceRulerOnDrawMajor;

            ReDraw();
            FireCanGoLeftRightChanged();            
        }

        private void SequencerOnPreDrawDone()
        {
            _yMarginManager.UpdateVerticalSettings();
        }

        private void DrawDatePeg(double x, DateTime dt)
        {
            var xpeg = MarginLeft + x;
            var ytop = MainCanvas.ActualHeight - MarginY;
            var ybottom = ytop + DateBarHeight;
            var line = new Line
            {
                X1 = xpeg,
                X2 = xpeg,
                Y1 = ytop,
                Y2 = ybottom,
                Stroke = _blackBrush
            };
            MainCanvas.Children.Add(line);

            var year = dt.Date.Year;
            var text = new TextBlock
            {
                Text = string.Format(year > _lastYear ? "{0:dd/MM/yy}" : "{0:dd/MM}", dt.Date)
            };
            _lastYear = year;
            text.SetValue(Canvas.TopProperty, ybottom);
            text.SetValue(Canvas.LeftProperty, xpeg);
            text.LayoutUpdated += (s, e) =>
                text.SetValue(Canvas.LeftProperty, xpeg - text.ActualWidth / 2);
            MainCanvas.Children.Add(text);
        }

        private void PriceRulerOnDrawMajor(double y, double value)
        {
            var line = new Line
            {
                X1 = MarginLeft - MajorBarWidth,
                X2 = MarginLeft,
                Y1 = y,
                Y2 = y,
                Stroke = _blackBrush
            };
            MainCanvas.Children.Add(line);

            var text = new TextBlock
            {
                Text = string.Format("{0:0.000}", value)
            };
            text.SetValue(Canvas.TopProperty, y);
            text.SetValue(Canvas.LeftProperty, 0);
            MainCanvas.Children.Add(text);
        }

        private void MainCanvasOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReDraw();

            FireCanGoLeftRightChanged();
        }

        private void ReDraw()
        {
            if (_candlePlotter == null) return;

            var totalHeight = MainCanvas.ActualHeight - MarginY * 2;
            var width = MainCanvas.ActualWidth - MarginLeft - MarginRight;
            _volumeChartHeight = totalHeight * VolumeChartHeightRatio;
            _candleChartHeight = totalHeight - _volumeChartHeight;
            _ChartXOffset = MarginLeft;
            _candleChartYOffset = MarginY;
            _volumeChartYOffset = _candleChartYOffset + _candleChartHeight;

            var chartWtLRatio = StockSequencer.DefaultChartWidthToLengthRatio * Math.Pow(ZoomBase, _zoomLevel);
            _sequencer.Length = width / chartWtLRatio;
            _candlePlotter.ChartWidth = width;
            _candlePlotter.ChartHeight = _candleChartHeight;

            _volumePlotter.ChartWidth = width;
            _volumePlotter.ChartHeight = _volumeChartHeight;

            _timeRuler.RulerWidth = width;

            var records = _sequencer.GetStocksStarting(_startIndex);
            PrepareDraw();
            _sequencer.Draw(records);
        }

        private void PrepareDraw()
        {
            _lastYear = int.MinValue;
            _yMarginManager.ResetMinMax();
            MainCanvas.Children.Clear();
        }

        private void DrawVolume(VolumePlotter.VolumeShape shape)
        {
            var line = new Line
            {
                X1 = _ChartXOffset + shape.X,
                X2 = _ChartXOffset + shape.X,
                Y1 = _volumeChartYOffset + _volumeChartHeight,
                Y2 = _volumeChartYOffset + shape.Y,
                Stroke = _blackBrush
            };
            MainCanvas.Children.Add(line);
        }

        private void DrawCandle(CandleShape shape)
        {
            var rect = new Rectangle
            {
                Width = shape.XRectMax - shape.XRectMin,
                Height = shape.YRectMax - shape.YRectMin,
                Stroke = _blackBrush
            };
            if (rect.Height < 1) rect.Height = 1;
            rect.SetValue(Canvas.LeftProperty, _ChartXOffset + shape.XRectMin);
            rect.SetValue(Canvas.TopProperty, _candleChartYOffset + shape.YRectMin);
            if (shape.CandleType == CandleTypes.Yin)
            {
                rect.Fill = _blackBrush;
            }
            MainCanvas.Children.Add(rect);

            var lineTop = new Line
            {
                X1 = _ChartXOffset + shape.X,
                X2 = _ChartXOffset + shape.X,
                Y1 = _candleChartYOffset + shape.YMin,
                Y2 = _candleChartYOffset + shape.YRectMin,
                Stroke = _blackBrush
            };

            MainCanvas.Children.Add(lineTop);

            var lineBottom = new Line
            {
                X1 = _ChartXOffset + shape.X,
                X2 = _ChartXOffset + shape.X,
                Y1 = _candleChartYOffset + shape.YRectMax,
                Y2 = _candleChartYOffset + shape.YMax,
                Stroke = _blackBrush
            };

            MainCanvas.Children.Add(lineBottom);
        }

        private void SetupCommands()
        {
            OpenExpertFileCommand = new OpenExpertFileCommand(_expert);
            OpenStockFileCommand = new OpenStockFileCommand(_stock);
            GoLeftCommand = new GoLeftCommand(this);
            GoRightCommand = new GoRightCommand(this);
            ZoomInCommand = new ZoomInCommand(this);
            ZoomOutCommand = new ZoomOutCommand(this);
        }

        private void FireCanGoLeftRightChanged()
        {
            CanGoLeftChanged?.Invoke(this, new EventArgs());
            CanGoRightChanged?.Invoke(this, new EventArgs());
        }

        private void FireCanZoomInOutChanged()
        {
            CanZoomInChanged?.Invoke(this, new EventArgs());
            CanZoomOutChanged?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
