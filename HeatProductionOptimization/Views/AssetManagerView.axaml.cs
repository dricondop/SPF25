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
    }

    private void DoubleTapped(object sender, RoutedEventArgs e)
    {
        if (sender is Border border)
        {
            var panel = border.Child as Panel;
            if (panel == null)
                return;

            var textBlock = panel.FindDescendantOfType<TextBlock>();
            var textBox = panel.FindDescendantOfType<TextBox>();

            if (textBlock != null && textBox != null)
            {
                textBlock.IsVisible = false;
                textBox.IsVisible = true;

                textBox.Focus();
                textBox.SelectAll();
            }
        }
    }

    private void LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var panel = textBox.Parent as Panel;
            if (panel == null)
                return;

            var textBlock = panel.Children[0] as TextBlock;
            if (textBlock != null)
            {
                textBlock.Text = textBox.Text;
                textBox.IsVisible = false;
                textBlock.IsVisible = true;
            }
        }
    }

    private void KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (sender is TextBox textBox)
            {
                var panel = textBox.Parent as Panel;
                if (panel == null) 
                    return;

                var textBlock = panel.Children[0] as TextBlock;
                if (textBlock != null)
                {
                    textBlock.Text = textBox.Text;
                    textBox.IsVisible = false;
                    textBlock.IsVisible = true;

                    e.Handled = true;
                }
            }
        }
    }

    private void AddUnit_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AssetManagerViewModel viewModel)
        {
            viewModel.AddNewUnit();
        }
    }
    
    private void SaveChanges_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AssetManagerViewModel viewModel)
        {
            viewModel.SaveChanges();
        }
    }
}