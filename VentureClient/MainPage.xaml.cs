using VentureClient.Commands;
using VentureClient.Models;
using Windows.UI.Xaml.Controls;
using VentureVisualization;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using static VentureVisualization.CandleChartPlotter;
using VentureClient.Interfaces;
using System;

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

        private Expert _expert;
        private Stock _stock;
        private CandleChartPlotter _candlePlotter;
        private VolumePlotter _volumePlotter;
        private int _startIndex = 0;
        private int _zoomLevel = 0;
        private StockSequencer _sequencer;

        #region Reletaively unchanged drawing styles

        private Brush _blackBrush = new SolidColorBrush(Colors.Black);

        #endregion

        #region Current drawing context

        private double _volumeChartHeight;
        private double _candleChartHeight;

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

        private const double VolumeChartHeightRatio = 0.1;

        private void OnStockUpdated()
        {
            _startIndex = 0;
            _zoomLevel = 0;
            _sequencer = new StockSequencer(_stock.Data);
            _candlePlotter = new CandleChartPlotter(DrawBegin, DrawCandle, DrawEnd);
            _volumePlotter = new VolumePlotter(DummyDrawBeginEnd, DrawVolume, DummyDrawBeginEnd);
            _candlePlotter.Subscribe(_sequencer);
            _volumePlotter.Subscribe(_sequencer);
            ReDraw();
            FireCanGoLeftRightChanged();
            
            StockCode.Text = _stock.Code;
        }

        private void MainCanvasOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReDraw();

            FireCanGoLeftRightChanged();
        }

        private void ReDraw()
        {
            _volumeChartHeight = MainCanvas.ActualHeight * VolumeChartHeightRatio;
            _candleChartHeight = MainCanvas.ActualHeight - _volumeChartHeight;

            var width = MainCanvas.ActualWidth;
            if (_candlePlotter == null) return;
            var chartWtLRatio = StockSequencer.DefaultChartWidthToLengthRatio * Math.Pow(ZoomBase, _zoomLevel);
            _sequencer.Length = width / chartWtLRatio;
            _candlePlotter.ChartWidth = width;
            _candlePlotter.ChartHeight = _candleChartHeight;

            _volumePlotter.ChartWidth = width;
            _volumePlotter.ChartHeight = _volumeChartHeight;

            var records = _sequencer.GetStocksStarting(_startIndex);
            _sequencer.Draw(records);
        }

        private void DrawBegin()
        {
            MainCanvas.Children.Clear();
        }

        private void DrawEnd()
        {
        }

        private void DummyDrawBeginEnd()
        {
        }

        private void DrawVolume(VolumePlotter.VolumeShape shape)
        {
            var line = new Line
            {
                X1 = shape.X,
                X2 = shape.X,
                Y1 = MainCanvas.ActualHeight,
                Y2 = _candleChartHeight + shape.Y,
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
            rect.SetValue(Canvas.LeftProperty, shape.XRectMin);
            rect.SetValue(Canvas.TopProperty, shape.YRectMin);
            if (shape.CandleType == CandleTypes.Yin)
            {
                rect.Fill = _blackBrush;
            }
            MainCanvas.Children.Add(rect);

            var lineTop = new Line
            {
                X1 = shape.X,
                X2 = shape.X,
                Y1 = shape.YMin,
                Y2 = shape.YRectMin,
                Stroke = _blackBrush
            };

            MainCanvas.Children.Add(lineTop);

            var lineBottom = new Line
            {
                X1 = shape.X,
                X2 = shape.X,
                Y1 = shape.YRectMax,
                Y2 = shape.YMax,
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
