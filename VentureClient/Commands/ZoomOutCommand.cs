using System;
using System.Windows.Input;
using VentureClient.Interfaces;

namespace VentureClient.Commands
{
    public class ZoomOutCommand : ICommand
    {
        private IChartNavigator _navigator;

        public ZoomOutCommand(IChartNavigator navigator)
        {
            _navigator = navigator;
        }

        public event EventHandler CanExecuteChanged
        {
            add { _navigator.CanZoomOutChanged += value; }
            remove { _navigator.CanZoomOutChanged -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _navigator.CanZoomOut;
        }

        public void Execute(object parameter)
        {
            _navigator.ZoomOut();
        }
    }
}
