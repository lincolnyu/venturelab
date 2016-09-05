using System;
using System.Windows.Input;
using VentureClient.Interfaces;

namespace VentureClient.Commands
{
    public class ZoomInCommand : ICommand
    {
        private IChartNavigator _navigator;

        public ZoomInCommand(IChartNavigator navigator)
        {
            _navigator = navigator;
        }

        public event EventHandler CanExecuteChanged
        {
            add { _navigator.CanZoomInChanged += value; }
            remove { _navigator.CanZoomInChanged -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _navigator.CanZoomIn;
        }

        public void Execute(object parameter)
        {
            _navigator.ZoomIn();
        }
    }
}
