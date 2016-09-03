using System;
using System.Windows.Input;
using VentureClient.Models;

namespace VentureClient.Commands
{
    public class OpenFileCommand : ICommand
    {
        public OpenFileCommand(Expert expert)
        {
            Expert = expert;
        }

        public event EventHandler CanExecuteChanged;

        public Expert Expert { get; }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            await Expert.PickFile();
        }
    }
}
