using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using HeatProductionOptimization.ViewModels;

namespace HeatProductionOptimization.Views;

public partial class AssetManagerView : UserControl
{
    public AssetManagerView()
    {
        InitializeComponent();
        
        // Make sure DataContext is set if not already done elsewhere
        if (Design.IsDesignMode)
            DataContext = new AssetManagerViewModel();
    }

    private void EditableValue_DoubleTapped(object sender, RoutedEventArgs e)
    {
        if (sender is Border border)
        {
            // Find the display TextBlock and edit TextBox within the Panel
            var panel = border.Child as Panel;
            if (panel == null) return;

            var textBlock = panel.FindDescendantOfType<TextBlock>();
            var textBox = panel.FindDescendantOfType<TextBox>();

            if (textBlock != null && textBox != null)
            {
                // Hide the display and show the editor
                textBlock.IsVisible = false;
                textBox.IsVisible = true;
                
                // Focus the editor and select all text
                textBox.Focus();
                textBox.SelectAll();
            }
        }
    }

    private void EditableValue_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            // Find the parent panel and the display TextBlock
            var panel = textBox.Parent as Panel;
            if (panel == null) return;

            var textBlock = panel.Children[0] as TextBlock;
            if (textBlock != null)
            {
                // Hide the editor and show the display again
                textBox.IsVisible = false;
                textBlock.IsVisible = true;
            }
        }
    }
}