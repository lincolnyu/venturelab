using VentureClient.Commands;
using VentureClient.Models;
using Windows.UI.Xaml.Controls;
using VentureVisualization;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using static VentureVisualization.CandleChartPlotter;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VentureClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Expert _expert;
        private Stock _stock;
        private CandleChartPlotter _candlePlotter;

        public MainPage()
        {
            InitializeComponent();
            SetupModels();
            SetupCommands();
            DataContext = this;
        }

        public OpenExpertFileCommand OpenExpertFile { get; private set; }

        public OpenStockFileCommand OpenStockFile { get; private set; }

        private void SetupModels()
        {
            _expert = new Expert();
            _stock = new Stock();
            _stock.StockUpdated += OnStockUpdated;
        }

        private void OnStockUpdated()
        {
            _candlePlotter = new CandleChartPlotter(_stock.Data, MainCanvas.ActualWidth, MainCanvas.ActualHeight, DrawBegin, DrawCandle, DrawEnd);
            ReDraw(MainCanvas.ActualWidth, MainCanvas.ActualHeight);

            StockCode.Text = _stock.Code;
        }

        private void MainCanvasOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReDraw(e.NewSize.Width, e.NewSize.Height);
        }

        private void ReDraw(double width, double height)
        {
            if (_candlePlotter == null) return;
            _candlePlotter.Length = width / DefaultChartWidthToLengthRatio;
            _candlePlotter.ChartWidth = width;
            _candlePlotter.ChartHeight = height;
            _candlePlotter.Draw(0);
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
            OpenExpertFile = new OpenExpertFileCommand(_expert);
            OpenStockFile = new OpenStockFileCommand(_stock);
        }
    }
}
