using System;

namespace VentureClient.Interfaces
{
    public delegate void CanGoChangedEventHandler();

    public interface IChartNavigator
    {
        bool CanGoLeft { get; }
        bool CanGoRight { get; }
        bool CanZoomIn { get; }
        bool CanZoomOut { get; }

        event EventHandler CanGoLeftChanged;
        event EventHandler CanGoRightChanged;
        event EventHandler CanZoomInChanged;
        event EventHandler CanZoomOutChanged;

        void GoLeft(int step);
        void GoRight(int step);
        void ZoomIn();
        void ZoomOut();
    }
}
