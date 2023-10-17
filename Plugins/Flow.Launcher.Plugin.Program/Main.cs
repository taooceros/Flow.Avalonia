using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin.Program.Programs;
using Flow.Launcher.Plugin.Program.Views;
using Flow.Launcher.Plugin.Program.Views.Models;

namespace Flow.Launcher.Plugin.Program
{
    public class Main : ISettingProvider, IAsyncPlugin, IPluginI18n, IContextMenu, ISavable, IAsyncReloadable,
        IDisposable
    {
        internal static Win32[] _win32s { get; set; }
        internal static UWP.Application[] _uwps { get; set; }
        internal static Settings _settings { get; set; }


        internal static PluginInitContext Context { get; private set; }

        private static readonly List<Result> emptyResults = new();


        static Main()
        {
        }

        public void Save()
        {
        }

        public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
        {
            var resultList = await Task.Run(() =>
                _win32s.Cast<IProgram>()
                    .Concat(_uwps)
                    .AsParallel()
                    .WithCancellation(token)
                    .Where(p => p.Enabled)
                    .Select(p => p.Result(query.Search, Context.API))
                    .Where(r => r?.Score > 0)
                    .ToList());

            resultList = resultList.Any() ? resultList : emptyResults;

            return resultList;
        }

        public async Task InitAsync(PluginInitContext context)
        {
            Context = context;

            _settings = new Settings();

            _ = Task.Run(async () =>
            {
                await IndexProgramsAsync().ConfigureAwait(false);
                WatchProgramUpdate();
            });

            static void WatchProgramUpdate()
            {
                Win32.WatchProgramUpdate(_settings);
                _ = UWP.WatchPackageChange();
            }
        }

        public static void IndexWin32Programs()
        {
            var win32S = Win32.All(_settings);
            _win32s = win32S;
            ResetCache();
            _settings.LastIndexTime = DateTime.Now;
        }

        public static void IndexUwpPrograms()
        {
            var applications = UWP.All(_settings);
            _uwps = applications;
            ResetCache();
            _settings.LastIndexTime = DateTime.Now;
        }

        public static async Task IndexProgramsAsync()
        {
            var a = Task.Run(IndexWin32Programs);

            var b = Task.Run(IndexUwpPrograms);
            await Task.WhenAll(a, b).ConfigureAwait(false);
        }

        internal static void ResetCache()
        {
        }

        public Control CreateSettingPanel()
        {
            return new ProgramSetting(Context, _settings, _win32s, _uwps);
        }

        public string GetTranslatedPluginTitle()
        {
            return Context.API.GetTranslation("flowlauncher_plugin_program_plugin_name");
        }

        public string GetTranslatedPluginDescription()
        {
            return Context.API.GetTranslation("flowlauncher_plugin_program_plugin_description");
        }

        public List<Result> LoadContextMenus(Result selectedResult)
        {
            var menuOptions = new List<Result>();
            var program = selectedResult.ContextData as IProgram;
            if (program != null)
            {
                menuOptions = program.ContextMenus(Context.API);
            }

            menuOptions.Add(
                new Result
                {
                    Title = Context.API.GetTranslation("flowlauncher_plugin_program_disable_program"),
                    Action = c =>
                    {
                        DisableProgram(program);
                        Context.API.ShowMsg(
                            Context.API.GetTranslation("flowlauncher_plugin_program_disable_dlgtitle_success"),
                            Context.API.GetTranslation(
                                "flowlauncher_plugin_program_disable_dlgtitle_success_message"));
                        return false;
                    },
                    IcoPath = "Images/disable.png",
                    Glyph = new GlyphInfo(FontFamily: "/Resources/#Segoe Fluent Icons", Glyph: "\xece4"),
                }
            );

            return menuOptions;
        }

        private static void DisableProgram(IProgram programToDelete)
        {
            if (_settings.DisabledProgramSources.Any(x => x.UniqueIdentifier == programToDelete.UniqueIdentifier))
                return;

            if (_uwps.Any(x => x.UniqueIdentifier == programToDelete.UniqueIdentifier))
            {
                var program = _uwps.First(x => x.UniqueIdentifier == programToDelete.UniqueIdentifier);
                program.Enabled = false;
                _settings.DisabledProgramSources.Add(new ProgramSource(program));
                _ = Task.Run(() => { IndexUwpPrograms(); });
            }
            else if (_win32s.Any(x => x.UniqueIdentifier == programToDelete.UniqueIdentifier))
            {
                var program = _win32s.First(x => x.UniqueIdentifier == programToDelete.UniqueIdentifier);
                program.Enabled = false;
                _settings.DisabledProgramSources.Add(new ProgramSource(program));
                _ = Task.Run(() => { IndexWin32Programs(); });
            }
        }

        public static void StartProcess(Func<ProcessStartInfo, Process> runProcess, ProcessStartInfo info)
        {
            try
            {
                runProcess(info);
            }
            catch (Exception)
            {
                var title = Context.API.GetTranslation("flowlauncher_plugin_program_disable_dlgtitle_error");
                var message = string.Format(Context.API.GetTranslation("flowlauncher_plugin_program_run_failed"),
                    info.FileName);
                Context.API.ShowMsg(title, string.Format(message, info.FileName), string.Empty);
            }
        }

        public async Task ReloadDataAsync()
        {
            await IndexProgramsAsync();
        }

        public void Dispose()
        {
            Win32.Dispose();
        }
    }
}