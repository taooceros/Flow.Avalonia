using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using Avalonia.ReactiveUI;
using Flow.Launcher.Plugin;
using FlowAvalonia.PublicAPI;

namespace FlowAvalonia.ViewModels;

public partial class ResultItemBoxViewModel : ViewModelBase
{
    public IImageLoader ImageLoader { get; }

    private readonly ReadOnlyObservableCollection<ResultItemViewModel> resultItems;

    [ObservableProperty] private ReadOnlyObservableCollection<ResultItemViewModel> items;

    public ReadOnlyObservableCollection<ResultItemViewModel> contextItems { get; set; }

    private SourceList<Result> ResultList { get; } = new();

    [ObservableProperty] private int _selectedIndex;

    public Channel<IEnumerable<Result>> ResultsChannel = Channel.CreateUnbounded<IEnumerable<Result>>(
        new UnboundedChannelOptions()
        {
            SingleReader = true,
        });

    public void MoveIndex(int diff)
    {
        if (Items.Count == 0)
        {
            SelectedIndex = -1;
            return;
        }

        SelectedIndex = Mod(SelectedIndex + diff, Items.Count);
        return;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int Mod(int x, int m) => (x % m + m) % m;
    }

    public void ToContextMenu()
    {
        Items = contextItems;
    }
    
    public void Escape()
    {
        Items = resultItems;
    }

    public void MoveDown() => MoveIndex(1);

    public void MoveUp() => MoveIndex(-1);

    private async void UpdateResultsAsync()
    {
        var reader = ResultsChannel.Reader;

        while (await reader.WaitToReadAsync().ConfigureAwait(false))
        {
            IEnumerable<Result> current = null!;
            while (reader.TryRead(out var next))
            {
                current = next;
                await Task.Delay(50).ConfigureAwait(false);
            }

            ResultList.EditDiff(current);
        }
    }

    public ResultItemBoxViewModel(IImageLoader imageLoader)
    {
        ImageLoader = imageLoader;
        
        ResultList.Connect()
            .Sort(SortExpressionComparer<Result>.Descending(r => r.Score))
            .Transform(result => new ResultItemViewModel(result, imageLoader))
            .ObserveOn(AvaloniaScheduler.Instance)
            .Bind(out resultItems)
            .Do(x => MoveIndex(-SelectedIndex))
            .Subscribe();

        _ = Task.Run(UpdateResultsAsync);

        Items = resultItems;
        contextItems = new ReadOnlyObservableCollection<ResultItemViewModel>(
            new ObservableCollection<ResultItemViewModel>(
                Enumerable.Range(1, 500).Select(i =>
                    new ResultItemViewModel
                    (new Result()
                        {
                            Title = $"test {i}"
                        },
                        ImageLoader
                    ))));
    }
}