using System;
using System.Windows.Input;
using VentureClient.Interfaces;

namespace VentureClient.Commands
{
    public class GoRightCommand : ICommand
    {
        private IChartNavigator _navigator;

        public GoRightCommand(IChartNavigator navigator)
        {
            _navigator = navigator;
        }
        
        public event EventHandler CanExecuteChanged
        {
            add { _navigator.CanGoRightChanged += value; }
            remove { _navigator.CanGoRightChanged -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _navigator.CanGoRight;
        }

        public void Execute(object parameter)
        {
            _navigator.GoRight();
        }
    }
}
