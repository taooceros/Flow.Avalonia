using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using FlowAvalonia.ViewModels;

namespace FlowAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowWindowViewModel mainWindowWindowViewModel)
    {
        DataContext = mainWindowWindowViewModel;
        InitializeComponent();
        InitializePosition();
    }

    private void InitializePosition()
    {
        var screen = Screens.Primary;
        var workingArea = screen.WorkingArea;
        var position = new PixelPoint((workingArea.Width) / 2 - (int)Width, workingArea.Height / 2 - 400);
        Position = position;
    }
}