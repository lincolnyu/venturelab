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
using System.Collections.Generic;
using Windows.UI.Xaml.Input;
using System.ComponentModel;
using Windows.Foundation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VentureClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IChartNavigator, INotifyPropertyChanged
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

        public const double StepLengthRatio = 0.25;

        #region Data

        private Expert _expert;
        private Stock _stock;

        #endregion

        #region Plotting facilities

        private StockAndPredictionSequencer _sequencer;

        private YMarginManager _yMarginManager;

        private CandleChartPlotter _candlePlotter;
        private VolumePlotter _volumePlotter;
        private PriceRuler _priceRuler;
        private TimeRuler _timeRuler;
        private PredictionPlotter _predictPlotter;

        #endregion

        #region Current plotting contextual parameters

        private int _startIndex = 0;
        private int _zoomLevel = 0;

        #endregion

        #region Frame-wide variables for plotting

        private int _lastYear = int.MinValue;

        #endregion

        #region Drawing styles

        private Brush _greenBrush = new SolidColorBrush(Colors.Green);
        private Brush _blueBrush = new SolidColorBrush(Colors.Blue);
        private Brush _blackBrush = new SolidColorBrush(Colors.Black);
        private Brush _redBrush = new SolidColorBrush(Colors.Red);

        #endregion

        #region Current drawing context

        private double _volumeChartYOffset;
        private double _candleChartYOffset;
        private double _ChartXOffset;
        private double _volumeChartHeight;
        private double _candleChartHeight;

        #endregion

        #region Fixed drawing elements

        private Line _crossHori = new Line();
        private Line _crossVert = new Line();

        private Point _lastPointPos;

        #endregion

        #region Backing fields

        private bool _isShowingCross;

        #endregion

        public MainPage()
        {
            InitializeComponent();
            SetupModels();
            SetupCommands();
            SetupFixedElements();
            SetupInitUI();
            DataContext = this;
        }

        #region Commands

        public OpenExpertFileCommand OpenExpertFileCommand { get; private set; }
        public OpenStockFileCommand OpenStockFileCommand { get; private set; }
        public GoRightCommand GoRightCommand { get; private set; }
        public GoLeftCommand GoLeftCommand { get; private set; }
        public GoRightmostCommand GoRightmostCommand { get; private set; }
        public GoLeftmostCommand GoLeftmostCommand { get; private set; }
        public ZoomInCommand ZoomInCommand { get; private set; }
        public ZoomOutCommand ZoomOutCommand { get; private set; }

        #endregion

        #region Data bindings

        public bool IsShowingCross
        {
            get
            {
                return _isShowingCross;
            }

            set
            {
                if (_isShowingCross != value)
                {
                    _isShowingCross = value;
                    UpdateShowingCross();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsShowingCross"));
                }
            }
        }

        #endregion

        #region IChartNavigator members

        public bool CanGoLeft => _sequencer != null && _startIndex > 0;

        public bool CanGoRight => _sequencer != null && _startIndex + _sequencer.Length < _sequencer.TotalDataLength;

        public bool CanZoomIn => _zoomLevel < MaxZoomLevel;
        public bool CanZoomOut => _zoomLevel > MinZoomLevel;

        public event EventHandler CanGoLeftChanged;
        public event EventHandler CanGoRightChanged;
        public event EventHandler CanZoomInChanged;
        public event EventHandler CanZoomOutChanged;

        #endregion

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        #region IChartNavigator members

        public void GoLeft()
        {
            _startIndex -= (int)Math.Ceiling(_sequencer.Length * StepLengthRatio);
            if (_startIndex < 0) _startIndex = 0;

            ReDraw();

            FireCanGoLeftRightChanged();
        }

        public void GoRight()
        {
            _startIndex += (int)Math.Ceiling(_sequencer.Length * StepLengthRatio);
            if (_startIndex + _sequencer.Length >= _sequencer.TotalDataLength) _startIndex = (int)Math.Ceiling((_sequencer.TotalDataLength - _sequencer.Length));

            ReDraw();

            FireCanGoLeftRightChanged();
        }

        public void GoLeftmost()
        {
            _startIndex = 0;

            ReDraw();

            FireCanGoLeftRightChanged();
        }

        public void GoRightmost()
        {
            _startIndex = (int)Math.Ceiling((_sequencer.TotalDataLength - _sequencer.Length));

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

        #region Setup

        private void SetupModels()
        {
            _expert = new Expert();
            _stock = new Stock();
            _stock.StockUpdated += OnDataUpdated;
            _expert.ExpertUpdated += OnDataUpdated;
        }


        private void SetupCommands()
        {
            OpenExpertFileCommand = new OpenExpertFileCommand(_expert);
            OpenStockFileCommand = new OpenStockFileCommand(_stock);
            GoLeftCommand = new GoLeftCommand(this);
            GoRightCommand = new GoRightCommand(this);
            GoLeftmostCommand = new GoLeftmostCommand(this);
            GoRightmostCommand = new GoRightmostCommand(this);
            ZoomInCommand = new ZoomInCommand(this);
            ZoomOutCommand = new ZoomOutCommand(this);
        }

        private void SetupFixedElements()
        {
            MainCanvas.Children.Add(_crossHori);
            MainCanvas.Children.Add(_crossVert);
        }

        private void SetupInitUI()
        {
            IsShowingCross = true;
        }

        #endregion

        private void OnDataUpdated()
        {
            if (_stock.Code == null) return;

            StockCode.Text = _stock.Code;

            _startIndex = 0;
            _zoomLevel = 0;

            List<PredictionSample> predictSamples;
            if (!_expert.Data.TryGetValue(_stock.Code, out predictSamples))
            {
                predictSamples = new List<PredictionSample>();
            }
            _sequencer = new StockAndPredictionSequencer(_stock.Data, predictSamples);
            _sequencer.PreDrawDone += SequencerOnPreDrawDone;

            _yMarginManager = new YMarginManager { VertialMode = YMarginManager.VerticalModes.YMargins };

            _candlePlotter = new CandleChartPlotter(_yMarginManager);
            _priceRuler = new PriceRuler(_candlePlotter);
            _priceRuler.DrawMajor += PriceRulerOnDrawMajor;
            _candlePlotter.DrawCandle += DrawCandle;
            _candlePlotter.DrawEnd += _priceRuler.Draw;
            _candlePlotter.Subscribe(_sequencer);

            _timeRuler = new TimeRuler();
            _timeRuler.DrawDatePeg += DrawDatePeg;
            _timeRuler.Subscribe(_sequencer);

            _volumePlotter = new VolumePlotter();
            _volumePlotter.DrawVolume += DrawVolume;
            _volumePlotter.Subscribe(_sequencer);

            _predictPlotter = new PredictionPlotter(_candlePlotter);
            _predictPlotter.DrawPrediction += PredictPlotterOnDrawPrediction;
            _predictPlotter.Subscribe(_sequencer);

            ReDraw();
            FireCanGoLeftRightChanged();
            FireCanZoomInOutChanged();
        }

        private void SequencerOnPreDrawDone()
        {
            _yMarginManager.UpdateVerticalSettings();
        }

        private void MainCanvasOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReDraw();
            UpdateCross();

            FireCanGoLeftRightChanged();
        }

        private void MainCanvasOnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (IsShowingCross)
            {
                var p = e.GetCurrentPoint(MainCanvas);
                _lastPointPos = p.Position;
                UpdateCross();
            }
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
            SetupFixedElements();
        }

        #region Plotter event handlers

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

        private void PredictPlotterOnDrawPrediction(PredictionPlotter.PredictionShape shape)
        {
            var lines = new Line[]
            {
                new Line
                {
                    Y1 = _candleChartYOffset + shape.PreviousShape.Y,
                    Y2 = _candleChartYOffset + shape.Y,
                    Stroke = _blueBrush
                },
                new Line
                {
                    Y1 = _candleChartYOffset + shape.PreviousShape.YLower,
                    Y2 = _candleChartYOffset + shape.YLower,
                    Stroke = _redBrush
                },
                new Line
                {
                    Y1 = _candleChartYOffset + shape.PreviousShape.YUpper,
                    Y2 = _candleChartYOffset + shape.YUpper,
                    Stroke = _greenBrush
                }
            };
            foreach (var line in lines)
            {
                line.X1 = _ChartXOffset + shape.PreviousShape.X;
                line.X2 = _ChartXOffset + shape.X;
                MainCanvas.Children.Add(line);
            }
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
                Text = string.Format(year > _lastYear ? "{0:dd/MM/yy}" : "{0:dd/MM}", dt.Date),
                Foreground = year > _lastYear ? _redBrush : _blackBrush
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

        #endregion

        #region UI handler

        private void UpdateShowingCross()
        {
            if (!IsShowingCross)
            {
                _crossHori.Visibility = Visibility.Collapsed;
                _crossVert.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        private void UpdateCross()
        {
            if (IsShowingCross && _lastPointPos != null)
            {
                _crossHori.X1 = 0;
                _crossHori.X2 = MainCanvas.ActualWidth;
                _crossHori.Y1 = _crossHori.Y2 = _lastPointPos.Y;
                _crossHori.Stroke = _blackBrush;
                _crossVert.Y1 = 0;
                _crossVert.Y2 = MainCanvas.ActualHeight;
                _crossVert.X1 = _crossVert.X2 = _lastPointPos.X;
                _crossVert.Stroke = _blackBrush;
                _crossHori.Visibility = Visibility.Visible;
                _crossVert.Visibility = Visibility.Visible;
            }
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
