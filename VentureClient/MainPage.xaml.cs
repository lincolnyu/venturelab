﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.Foundation;
using VentureVisualization.SequencePlotting;
using VentureVisualization.OtherPlotting;
using VentureClient.Commands;
using VentureClient.Models;
using VentureClient.Interfaces;
using VentureVisualization.Samples;
using static VentureVisualization.SequencePlotting.CandleChartPlotter;
using static VentureVisualization.SequencePlotting.TimeRuler;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VentureClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IChartNavigator, INotifyPropertyChanged
    {
        #region Constants about zooming and stepping

        public const double ZoomBase = 1.5;
        public const int MinZoomLevel = -20;
        public const int MaxZoomLevel = 5;

        public const double StepLengthRatio = 0.25;

        #endregion

        #region Constants about chart

        private const double MarginLeft = 50;
        private const double MarginRight = 30;
        private const double MarginY = 30;
        private const double MajorBarRatio = 0.6;
        private const double MajorBarWidth = MarginLeft * MajorBarRatio;
        private const double DateBarRatio = 0.4;
        private const double DateBarHeight = MarginY * DateBarRatio;
        private const double VolumeChartHeightRatio = 0.1;
        private const double RightmostAllowance = 10.0;

        #endregion

        #region Constants about candle display

        public const double CandleWidthThr = 2;

        #endregion

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
        private Point? _prevCandleDegradedPoint;

        #endregion

        #region Drawing styles

        private Brush _greenBrush = new SolidColorBrush(Colors.Green);
        private Brush _blueBrush = new SolidColorBrush(Colors.Blue);
        private Brush _blackBrush = new SolidColorBrush(Colors.Black);
        private Brush _grayBrush = new SolidColorBrush(Colors.Gray);
        private Brush _redBrush = new SolidColorBrush(Colors.Red);

        #endregion

        #region Current drawing context

        private double _volumeChartYOffset;
        private double _candleChartYOffset;
        private double _chartXOffset;
        private double _volumeChartHeight;
        private double _candleChartHeight;

        #endregion

        #region Fixed drawing elements

        private Line _crossHori = new Line();
        private Line _crossVert = new Line();
        private TextBlock _labelTime = new TextBlock();
        private TextBlock _labelPrice = new TextBlock();

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
        public ZoomResetCommand ZoomResetCommand { get; private set; }

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

        public bool CanGoRight => _sequencer != null && _startIndex < IndexRightmost;

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

        #region Handy properties

        private int IndexRightmost => 
            (int)Math.Ceiling(Math.Max(0, _sequencer.TotalDataLength - _sequencer.Length + RightmostAllowance));

        #endregion

        #region Methods

        #region IChartNavigator members

        public void GoLeft()
        {
            PrepareChartSizeForRedraw();

            _startIndex -= (int)Math.Ceiling(_sequencer.Length * StepLengthRatio);
            if (_startIndex < 0) _startIndex = 0;

            ReDraw();

            FireCanGoLeftRightChanged();
        }

        public void GoRight()
        {
            PrepareChartSizeForRedraw();

            _startIndex += (int)Math.Ceiling(_sequencer.Length * StepLengthRatio);
            WorryAboutRight();

            ReDraw();

            FireCanGoLeftRightChanged();
        }

        public void GoLeftmost()
        {
            PrepareChartSizeForRedraw();

            _startIndex = 0;

            ReDraw();

            FireCanGoLeftRightChanged();
        }

        public void GoRightmost()
        {
            PrepareChartSizeForRedraw();

            _startIndex = IndexRightmost;

            ReDraw();

            FireCanGoLeftRightChanged();
        }

        public void ZoomIn()
        {
            _zoomLevel++;

            PrepareChartSizeForRedraw();

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

            PrepareChartSizeForRedraw();

            WorryAboutRight();

            ReDraw();

            FireCanZoomInOutChanged();
            FireCanGoLeftRightChanged();
        }

        public void ZoomReset()
        {
            _zoomLevel = 0;

            PrepareChartSizeForRedraw();

            WorryAboutRight();

            ReDraw();

            FireCanZoomInOutChanged();
            FireCanGoLeftRightChanged();
        }

        private void WorryAboutRight()
        {
            if (_startIndex > IndexRightmost)
            {
                _startIndex = IndexRightmost;
            }
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
            ZoomResetCommand = new ZoomResetCommand(this);
        }

        private void SetupFixedElements()
        {
            MainCanvas.Children.Add(_crossHori);
            MainCanvas.Children.Add(_crossVert);
            MainCanvas.Children.Add(_labelTime);
            MainCanvas.Children.Add(_labelPrice);
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

            PrepareChartSizeForRedraw();
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
            PrepareChartSizeForRedraw();
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

        private void PrepareChartSizeForRedraw()
        {
            if (_sequencer == null) return;

            var totalHeight = MainCanvas.ActualHeight - MarginY * 2;
            var width = MainCanvas.ActualWidth - MarginLeft - MarginRight;
            _volumeChartHeight = totalHeight * VolumeChartHeightRatio;
            _candleChartHeight = totalHeight - _volumeChartHeight;
            _chartXOffset = MarginLeft;
            _candleChartYOffset = MarginY;
            _volumeChartYOffset = _candleChartYOffset + _candleChartHeight;

            var chartWtLRatio = StockSequencer.DefaultChartWidthToLengthRatio * Math.Pow(ZoomBase, _zoomLevel);
            _sequencer.Length = width / chartWtLRatio;
            _candlePlotter.ChartWidth = width;
            _candlePlotter.ChartHeight = _candleChartHeight;

            _volumePlotter.ChartWidth = width;
            _volumePlotter.ChartHeight = _volumeChartHeight;

            _timeRuler.RulerWidth = width;
        }

        private void ReDraw()
        {
            if (_sequencer == null) return;

            var records = _sequencer.GetStocksStarting(_startIndex);
            PrepareDraw();
            _sequencer.Draw(records);
        }

        private void PrepareDraw()
        {
            _lastYear = int.MinValue;
            _prevCandleDegradedPoint = null;
            _yMarginManager.ResetMinMax();
            MainCanvas.Children.Clear();
            SetupFixedElements();
        }

        #region Plotter event handlers

        private void DrawVolume(VolumePlotter.VolumeShape shape)
        {
            var line = new Line
            {
                X1 = _chartXOffset + shape.X,
                X2 = _chartXOffset + shape.X,
                Y1 = _volumeChartYOffset + _volumeChartHeight,
                Y2 = _volumeChartYOffset + shape.Y,
                Stroke = _blackBrush
            };
            MainCanvas.Children.Add(line);
        }

        private void DrawCandle(CandleShape shape)
        {
            var rectWidth = shape.XRectMax - shape.XRectMin;
            var x = _chartXOffset + shape.X;
            if (rectWidth > CandleWidthThr)
            {
                var rect = new Rectangle
                {
                    Width = rectWidth,
                    Height = shape.YRectMax - shape.YRectMin,
                    Stroke = _blackBrush
                };
                if (rect.Height < 1) rect.Height = 1;
                rect.SetValue(Canvas.LeftProperty, _chartXOffset + shape.XRectMin);
                rect.SetValue(Canvas.TopProperty, _candleChartYOffset + shape.YRectMin);
                if (shape.CandleType == CandleTypes.Yin)
                {
                    rect.Fill = _blackBrush;
                }
                MainCanvas.Children.Add(rect);

                var lineTop = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = _candleChartYOffset + shape.YMin,
                    Y2 = _candleChartYOffset + shape.YRectMin,
                    Stroke = _blackBrush
                };

                MainCanvas.Children.Add(lineTop);

                var lineBottom = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = _candleChartYOffset + shape.YRectMax,
                    Y2 = _candleChartYOffset + shape.YMax,
                    Stroke = _blackBrush
                };

                MainCanvas.Children.Add(lineBottom);
            }
            else
            {
                var degradedCandle = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = _candleChartYOffset + shape.YMin,
                    Y2 = _candleChartYOffset + shape.YMax,
                    Stroke = _grayBrush
                };
                MainCanvas.Children.Add(degradedCandle);

                var close = shape.CandleType == CandleTypes.Yang ? shape.YRectMin : shape.YRectMax;
                close += _candleChartYOffset;
                var lastX = _prevCandleDegradedPoint?.X ?? x;
                var lastY = _prevCandleDegradedPoint?.Y ?? close;
                var midline = new Line
                {
                    X1 = lastX,
                    X2 = x,
                    Y1 = lastY,
                    Y2 = close,
                    Stroke = _blueBrush
                };
                MainCanvas.Children.Add(midline);

                _prevCandleDegradedPoint = new Point(x, close);
            }
        }

        private void PredictPlotterOnDrawPrediction(PredictionPlotter.PredictionShape shape)
        {
            if (shape.PreviousShape == null)
            {
                return;
            }
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
                line.X1 = _chartXOffset + shape.PreviousShape.X;
                line.X2 = _chartXOffset + shape.X;
                MainCanvas.Children.Add(line);
            }
        }

        private void DrawDatePeg(TimeShape ts)
        {
            var x = ts.X;
            var ds = ts as PastDateShape;
            if (ds != null)
            {
                var year = ds.Date.Year;
                var text = year > _lastYear ? $"{ds.Date:dd/MM/yy}" : $"{ds.Date:dd/MM}";
                DrawDatePeg(x, text, year > _lastYear ? _redBrush : _blackBrush);
                _lastYear = year;
                return;
            }
            var fds = ts as FutureDateShape;
            if(fds != null)
            {
                var text = fds.Days.ToString() + "d";
                DrawDatePeg(x, text, _blueBrush);
            }
        }
        
        private void DrawDatePeg(double x, string text, Brush textColor)
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

            var tb = new TextBlock
            {
                Text = text,
                Foreground = textColor
            };

            tb.SetValue(Canvas.TopProperty, ybottom);
            tb.SetValue(Canvas.LeftProperty, xpeg);
            tb.LayoutUpdated += (s, e) =>
                tb.SetValue(Canvas.LeftProperty, xpeg - tb.ActualWidth / 2);
            MainCanvas.Children.Add(tb);
        }

        private void PriceRulerOnDrawMajor(double y, double value)
        {
            y += _candleChartYOffset;
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
                Text = $"{value:0.000}"
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
                _labelTime.Visibility = Visibility.Collapsed;
                _labelPrice.Visibility = Visibility.Collapsed;
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
            else
            {
                _crossHori.Visibility = Visibility.Collapsed;
                _crossVert.Visibility = Visibility.Collapsed;
            }

            string timeText = null;
            if (_timeRuler != null)
            {
                var x = _lastPointPos.X - _chartXOffset;
                var ts = _timeRuler.FromXToTimeShape(x);
                timeText = ts?.ToString();
            }
            if (timeText != null)
            {
                _labelTime.Text = timeText;
                _labelTime.SetValue(Canvas.TopProperty, 0);
                if (_lastPointPos.X + _labelTime.ActualWidth < MainCanvas.ActualWidth || _lastPointPos.X < _labelTime.ActualWidth)
                {
                    _labelTime.SetValue(Canvas.LeftProperty, _lastPointPos.X);
                }
                else
                {
                    _labelTime.SetValue(Canvas.LeftProperty, _lastPointPos.X - _labelTime.ActualWidth);
                }
                _labelTime.Visibility = Visibility.Visible;
            }
            else
            {
                _labelTime.Visibility = Visibility.Collapsed;
            }

            var y = _lastPointPos.Y - _candleChartYOffset;
            if (_candlePlotter != null && y < _candleChartHeight)
            {
                var py = _candlePlotter.ViewYToY(y, _yMarginManager);
                _labelPrice.Text = $"{py:0.000}";

                _labelPrice.SetValue(Canvas.TopProperty, _lastPointPos.Y);
                _labelPrice.SetValue(Canvas.LeftProperty, MainCanvas.ActualWidth - _labelPrice.ActualWidth);
                _labelPrice.Visibility = Visibility.Visible;
            }
            else
            {
                _labelPrice.Visibility = Visibility.Collapsed;
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
