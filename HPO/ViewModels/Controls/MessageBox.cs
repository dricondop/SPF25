using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace HeatProductionOptimization.Controls
{
    public partial class MessageBox : Window
    {
        public MessageBox()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public MessageBox(string title, string message) : this()
        {
            this.Title = title;
            var messageBlock = this.FindControl<TextBlock>("MessageText");
            if (messageBlock != null)
            {
                messageBlock.Text = message;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}