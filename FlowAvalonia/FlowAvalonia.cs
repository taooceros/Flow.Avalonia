using FlowAvalonia.Views;

namespace FlowAvalonia;

public class FlowLauncher
{
    private readonly MainWindow _window;

    public FlowLauncher(MainWindow window)
    {
        _window = window;
    }

    public MainWindow GetMainWindow()
    {
        return _window;
    }
}