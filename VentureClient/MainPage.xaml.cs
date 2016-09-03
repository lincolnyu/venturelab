using VentureClient.Commands;
using VentureClient.Models;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VentureClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Expert _expert;

        public MainPage()
        {
            InitializeComponent();
            SetupExpert();
            SetupCommands();
            DataContext = this;
        }

        public OpenFileCommand OpenFile
        {
            get; private set;
        }

        private void SetupExpert()
        {
            _expert = new Expert();
        }

        private void SetupCommands()
        {
            OpenFile = new OpenFileCommand(_expert);
        }
    }
}
