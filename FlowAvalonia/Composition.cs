using Flow.Launcher.Plugin;
using FlowAvalonia.Component;
using FlowAvalonia.PublicAPI;
using FlowAvalonia.ViewModels;
using FlowAvalonia.Views;
using Pure.DI;
using static Pure.DI.Lifetime;

namespace FlowAvalonia;

public partial class Composition
{
    private static void Setup() => DI.Setup(nameof(Composition))
        .DefaultLifetime(Singleton)
        .Bind<InnerAPI>().To<InnerAPI>()
        .Bind<IImageLoader>().To<ImageLoader>()
        .Bind<IPublicAPI>().To<PublicAPIInstance>()
        .Bind<ResultItemBoxViewModel>().To<ResultItemBoxViewModel>().Root<ResultItemBoxViewModel>("ResultsBoxViewModel")
        .Bind<MainWindow>().To<MainWindow>()
        .Bind<IMainWindowViewModel>().To<MainWindowWindowViewModel>().Root<IMainWindowViewModel>("MainViewModel")
        .Root<FlowLauncher>("Root");
}