using System;
using System.Windows.Input;
using VentureClient.Models;

namespace VentureClient.Commands
{
    public class OpenStockFileCommand : ICommand
    {
        public OpenStockFileCommand(Stock stock)
        {
            Stock = stock;
        }

        public Stock Stock { get; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            await Stock.LoadFromFile();
        }
    }
}
