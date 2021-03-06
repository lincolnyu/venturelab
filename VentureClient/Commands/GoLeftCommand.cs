﻿using System;
using System.Windows.Input;
using VentureClient.Interfaces;

namespace VentureClient.Commands
{
    public class GoLeftCommand : ICommand
    {
        private IChartNavigator _navigator;

        public GoLeftCommand(IChartNavigator navigator)
        {
            _navigator = navigator;
        }

        public event EventHandler CanExecuteChanged
        {
            add { _navigator.CanGoLeftChanged += value; }
            remove { _navigator.CanGoLeftChanged -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _navigator.CanGoLeft;
        }

        public void Execute(object parameter)
        {
            _navigator.GoLeft();
        }
    }
}
