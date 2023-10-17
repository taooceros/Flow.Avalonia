using Avalonia.Media.Imaging;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Text;
using System.Collections.Generic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Launcher.Plugin;
using FlowAvalonia.PublicAPI;

namespace FlowAvalonia.ViewModels;

public partial class ResultItemViewModel : ViewModelBase
{
    private readonly IImageLoader _imageLoader;
    private static PrivateFontCollection fontCollection = new();
    private static Dictionary<string, string> fonts = new();

    public ResultItemViewModel(Result result, IImageLoader imageLoader)
    {
        _imageLoader = imageLoader;
        ArgumentNullException.ThrowIfNull(result);

        Result = result;

        if (Result.Glyph is not { FontFamily: not null } glyph) return;

        // Checks if it's a system installed font, which does not require path to be provided. 
        if (glyph.FontFamily.EndsWith(".ttf") || glyph.FontFamily.EndsWith(".otf"))
        {
            string fontFamilyPath = glyph.FontFamily;

            if (!Path.IsPathRooted(fontFamilyPath))
            {
                fontFamilyPath = Path.Combine(Result.PluginDirectory, fontFamilyPath);
            }

            if (fonts.TryGetValue(fontFamilyPath, out var font))
            {
                Glyph = glyph with
                {
                    FontFamily = font
                };
            }
            else
            {
                fontCollection.AddFontFile(fontFamilyPath);
                fonts[fontFamilyPath] =
                    $"{Path.GetDirectoryName(fontFamilyPath)}/#{fontCollection.Families[^1].Name}";
                Glyph = glyph with
                {
                    FontFamily = fonts[fontFamilyPath]
                };
            }
        }
        else
        {
            Glyph = glyph;
        }
    }

    public double IconRadius
    {
        get
        {
            if (Result.RoundedIcon)
            {
                return IconXY / 2;
            }

            return IconXY;
        }
    }


    private bool GlyphAvailable => Glyph is not null;

    private bool ImgIconAvailable => !string.IsNullOrEmpty(Result.IcoPath) || Result.Icon is not null;

    private bool PreviewImageAvailable => !string.IsNullOrEmpty(Result.Preview.PreviewImagePath) ||
                                          Result.Preview.PreviewDelegate != null;


    public string ShowTitleToolTip => string.IsNullOrEmpty(Result.TitleToolTip)
        ? Result.Title
        : Result.TitleToolTip;

    public string ShowSubTitleToolTip => string.IsNullOrEmpty(Result.SubTitleToolTip)
        ? Result.SubTitle
        : Result.SubTitleToolTip;

    public Task<Bitmap> Image => _imageLoader.LoadImageAsync(Result.IcoPath);

    [ObservableProperty, NotifyPropertyChangedFor(nameof(BackgroundColor))]
    private bool _isSelected;

    public IBrush BackgroundColor => IsSelected ? Brushes.Azure : Brushes.Transparent;

    /// <summary>
    /// Determines if to use the full width of the preview panel
    /// </summary>
    public bool UseBigThumbnail => Result.Preview.IsMedia;

    public GlyphInfo? Glyph { get; set; }


    public Result Result { get; }

    public int ResultProgress => Result.ProgressBar ?? 0;

    public string? QuerySuggestionText { get; set; }

    public double IconXY { get; set; } = 32;

    public override bool Equals(object obj)
    {
        return obj is ResultItemViewModel r && Result.Equals(r.Result);
    }

    public override int GetHashCode()
    {
        return Result.GetHashCode();
    }

    public override string ToString()
    {
        return Result.ToString();
    }
}