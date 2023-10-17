using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Launcher.Plugin;
using Flow.Launcher.Plugin.Program;
using FlowAvalonia.PublicAPI;
using Pure.DI;

namespace FlowAvalonia.ViewModels;

public partial class MainWindowWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    [ObservableProperty] public string? queryText;
    [ObservableProperty] public string? ctxQueryText;

    public required ResultItemBoxViewModel ResultItemBoxViewModel { get; init; }

    private CancellationTokenSource _cancellationTokenSource = new();

    public Main ProgramPlugin { get; set; } = new Main();

    partial void OnQueryTextChanged(string? value)
    {
        TriggerQueryUpdate(value);
    }

    public void TriggerQueryUpdate(string? value)
    {
        Task.Run(async () =>
        {
            var results = await ProgramPlugin.QueryAsync(new Query()
            {
                Search = value
            }, default);

            ResultItemBoxViewModel.ResultsChannel.Writer.TryWrite(results);
        });
    }

    public MainWindowWindowViewModel(InnerAPI api)
    {
        api.QueryTextChanged += (_, args) =>
        {
            var (newQuery, reQuery) = args;

            QueryText = newQuery;
        };

        ProgramPlugin.InitAsync(new PluginInitContext(new PluginMetadata(), new PublicAPIInstance(new InnerAPI())));
    }

    public void MoveResultDown() => ResultItemBoxViewModel.MoveDown();

    public void MoveResultUp() => ResultItemBoxViewModel.MoveUp();
}