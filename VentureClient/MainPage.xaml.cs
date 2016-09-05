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
        public const int MinZoomLevel = -5;
        public const int MaxZoomLevel = 5;

        private Expert _expert;
        private Stock _stock;
        private CandleChartPlotter _candlePlotter;
        private int _startIndex = 0;
        private int _zoomLevel = 0;

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

        public bool CanGoRight => _candlePlotter != null && _startIndex + _candlePlotter.Length < _stock.Data.Count;

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
            if (_startIndex + _candlePlotter.Length >= _stock.Data.Count) _startIndex = (int)(_stock.Data.Count - _candlePlotter.Length);

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
            _startIndex = 0;
            _zoomLevel = 0;
            _candlePlotter = new CandleChartPlotter(_stock.Data, MainCanvas.ActualWidth, MainCanvas.ActualHeight, DrawBegin, DrawCandle, DrawEnd);
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
            var width = MainCanvas.ActualWidth;
            var height = MainCanvas.ActualHeight;
            if (_candlePlotter == null) return;
            var chartWtLRatio = DefaultChartWidthToLengthRatio * Math.Pow(2, _zoomLevel);
            _candlePlotter.Length = width / chartWtLRatio;
            _candlePlotter.ChartWidth = width;
            _candlePlotter.ChartHeight = height;
            _candlePlotter.Draw(_startIndex);
        }

        private void DrawBegin()
        {
            MainCanvas.Children.Clear();
        }

        private void DrawEnd()
        {
        }

        private void DrawCandle(CandleShape shape)
        {
            var brush = new SolidColorBrush(Colors.Black);
            var rect = new Rectangle
            {
                Width = shape.XRectMax - shape.XRectMin,
                Height = shape.YRectMax - shape.YRectMin,
                Stroke = brush
            };
            if (rect.Height < 1) rect.Height = 1;
            rect.SetValue(Canvas.LeftProperty, shape.XRectMin);
            rect.SetValue(Canvas.TopProperty, shape.YRectMin);
            if (shape.CandleType == CandleTypes.Yin)
            {
                rect.Fill = brush;
            }
            MainCanvas.Children.Add(rect);

            var lineTop = new Line
            {
                X1 = shape.X,
                X2 = shape.X,
                Y1 = shape.YMin,
                Y2 = shape.YRectMin,
                Stroke = brush
            };

            MainCanvas.Children.Add(lineTop);

            var lineBottom = new Line
            {
                X1 = shape.X,
                X2 = shape.X,
                Y1 = shape.YRectMax,
                Y2 = shape.YMax,
                Stroke = brush
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
