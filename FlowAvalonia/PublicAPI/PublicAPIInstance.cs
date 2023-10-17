using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Flow.Launcher.Plugin;
using Flow.Launcher.Plugin.SharedModels;
using FlowAvalonia.Component;
using FlowAvalonia.ViewModels;

namespace FlowAvalonia.PublicAPI;

public class PublicAPIInstance : IPublicAPI
{
    public InnerAPI InnerAPI { get; }

    public PublicAPIInstance(InnerAPI api)
    {
        InnerAPI = api;
    }


    public void ChangeQuery(string query, bool reQuery = false)
    {
        InnerAPI.ChangeQueryText(query, reQuery);
    }

    public void RestartApp()
    {
        throw new NotImplementedException();
    }

    public void ShellRun(string cmd, string filename = "cmd.exe")
    {
        throw new NotImplementedException();
    }

    public void CopyToClipboard(string text, bool directCopy = false, bool showDefaultNotification = true)
    {
        throw new NotImplementedException();
    }

    public void SaveAppAllSettings()
    {
        throw new NotImplementedException();
    }

    public void SavePluginSettings()
    {
        throw new NotImplementedException();
    }

    public Task ReloadAllPluginData()
    {
        throw new NotImplementedException();
    }

    public void CheckForNewUpdate()
    {
        throw new NotImplementedException();
    }

    public void ShowMsgError(string title, string subTitle = "")
    {
        throw new NotImplementedException();
    }

    public void ShowMainWindow()
    {
        throw new NotImplementedException();
    }

    public void HideMainWindow()
    {
        throw new NotImplementedException();
    }

    public bool IsMainWindowVisible()
    {
        throw new NotImplementedException();
    }

    public event VisibilityChangedEventHandler? VisibilityChanged;

    public void ShowMsg(string title, string subTitle = "", string iconPath = "")
    {
        throw new NotImplementedException();
    }

    public void ShowMsg(string title, string subTitle, string iconPath, bool useMainWindowAsOwner = true)
    {
        throw new NotImplementedException();
    }

    public void OpenSettingDialog()
    {
        throw new NotImplementedException();
    }

    public string GetTranslation(string key)
    {
        throw new NotImplementedException();
    }

    public List<PluginPair> GetAllPlugins()
    {
        throw new NotImplementedException();
    }

    public void RegisterGlobalKeyboardCallback(Func<int, int, SpecialKeyState, bool> callback)
    {
        throw new NotImplementedException();
    }

    public void RemoveGlobalKeyboardCallback(Func<int, int, SpecialKeyState, bool> callback)
    {
        throw new NotImplementedException();
    }

    public MatchResult FuzzySearch(string query, string stringToCompare)
    {
        return StringMatcher.FuzzySearch(query, stringToCompare);
    }

    public Task<string> HttpGetStringAsync(string url, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> HttpGetStreamAsync(string url, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task HttpDownloadAsync(string url, string filePath, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public void AddActionKeyword(string pluginId, string newActionKeyword)
    {
        throw new NotImplementedException();
    }

    public void RemoveActionKeyword(string pluginId, string oldActionKeyword)
    {
        throw new NotImplementedException();
    }

    public bool ActionKeywordAssigned(string actionKeyword)
    {
        throw new NotImplementedException();
    }

    public void LogDebug(string className, string message, string methodName = "")
    {
        throw new NotImplementedException();
    }

    public void LogInfo(string className, string message, string methodName = "")
    {
        throw new NotImplementedException();
    }

    public void LogWarn(string className, string message, string methodName = "")
    {
        throw new NotImplementedException();
    }

    public void LogException(string className, string message, Exception e, string methodName = "")
    {
        throw new NotImplementedException();
    }

    public T LoadSettingJsonStorage<T>() where T : new()
    {
        throw new NotImplementedException();
    }

    public void SaveSettingJsonStorage<T>() where T : new()
    {
        throw new NotImplementedException();
    }

    public void OpenDirectory(string DirectoryPath, string FileNameOrFilePath = null)
    {
        throw new NotImplementedException();
    }

    public void OpenUrl(Uri url, bool? inPrivate = null)
    {
        throw new NotImplementedException();
    }

    public void OpenUrl(string url, bool? inPrivate = null)
    {
        throw new NotImplementedException();
    }

    public void OpenAppUri(Uri appUri)
    {
        throw new NotImplementedException();
    }

    public void OpenAppUri(string appUri)
    {
        throw new NotImplementedException();
    }
}