using Avalonia.Controls;
using Avalonia.Interactivity;

namespace HeatProductionOptimization.Views
{
    public partial class MainWindow : Window
    {
        private ImportButton _importButton;

        public MainWindow()
        {
            InitializeComponent();
            _importButton = new ImportButton(this);
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            _importButton.ImportClicked(sender, e);
        }
    }
}