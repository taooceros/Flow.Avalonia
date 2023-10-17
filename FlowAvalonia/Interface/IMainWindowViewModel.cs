using System.ComponentModel;
using FlowAvalonia.ViewModels;

namespace FlowAvalonia.PublicAPI;

public interface IMainWindowViewModel : INotifyPropertyChanged
{
    public string QueryText { get; set; }
    public ResultItemBoxViewModel ResultItemBoxViewModel { get; init; }
    public void TriggerQueryUpdate(string? value);
    public void MoveResultDown();

    public void MoveResultUp();
}