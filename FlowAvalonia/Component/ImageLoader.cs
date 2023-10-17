using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Web.Http;
using Avalonia.Media.Imaging;
using FlowAvalonia.PublicAPI;
using SkiaSharp;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace FlowAvalonia.Component
{
    public class ImageLoader : IImageLoader

    {
        private static readonly ConcurrentDictionary<(string, bool), Bitmap> ImageCache = new();
        private static readonly ConcurrentDictionary<string, string> GuidToKey = new();
        private static readonly bool EnableImageHash = true;
        public const int SmallIconSize = 64;
        public const int FullIconSize = 256;


        private static readonly string[] ImageExtensions =
        {
            ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tiff", ".ico"
        };

        private class ImageResult
        {
            public ImageResult(Bitmap imageSource, ImageType imageType)
            {
                ImageSource = imageSource;
                ImageType = imageType;
            }

            public ImageType ImageType { get; }
            public Bitmap ImageSource { get; }
        }

        private enum ImageType
        {
            File,
            Folder,
            Data,
            ImageFile,
            FullImageFile,
            Error,
            Cache
        }

        private static async ValueTask<ImageResult> LoadInternalAsync(string path, bool loadFullImage = false)
        {
            ImageResult imageResult;

            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return new ImageResult(null, ImageType.Error);
                }

                if (ImageCache.ContainsKey((path, loadFullImage)))
                {
                    return new ImageResult(ImageCache[(path, loadFullImage)], ImageType.Cache);
                }

                if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out var uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    var image = await LoadRemoteImageAsync(loadFullImage, uriResult);
                    ImageCache[(path, loadFullImage)] = image;
                    return new ImageResult(image, ImageType.ImageFile);
                }

                imageResult = await Task.Run(() => GetThumbnailResult(ref path, loadFullImage));
            }
            catch (System.Exception e)
            {
                try
                {
                    // Get thumbnail may fail for certain images on the first try, retry again has proven to work
                    imageResult = GetThumbnailResult(ref path, loadFullImage);
                }
                catch (System.Exception e2)
                {
                    imageResult = new ImageResult(null, ImageType.Error);
                }
            }

            ImageCache[(path, loadFullImage)] = imageResult.ImageSource;

            return imageResult;
        }

        private static HttpClient Client { get; set; } = new HttpClient();

        private static async Task<Bitmap> LoadRemoteImageAsync(bool loadFullImage, Uri uriResult)
        {
            // Download image from url
            using var resp = await Client.GetInputStreamAsync(uriResult);
            var image = Bitmap.DecodeToWidth(resp.AsStreamForRead(), 32);
            return image;
        }

        private static ImageResult GetThumbnailResult(ref string path, bool loadFullImage = false)
        {
            Bitmap image;
            ImageType type = ImageType.Error;

            if (Directory.Exists(path))
            {
                /* Directories can also have thumbnails instead of shell icons.
                 * Generating thumbnails for a bunch of folder results while scrolling
                 * could have a big impact on performance and Flow.Launcher responsibility.
                 * - Solution: just load the icon
                 */
                type = ImageType.Folder;
                image = GetThumbnail(path, ThumbnailOptions.IconOnly);
            }
            else if (File.Exists(path))
            {
                var extension = Path.GetExtension(path).ToLower();
                if (ImageExtensions.Contains(extension))
                {
                    type = ImageType.ImageFile;
                    if (loadFullImage)
                    {
                        image = LoadFullImage(path);
                        type = ImageType.FullImageFile;
                    }
                    else
                    {
                        /* Although the documentation for GetImage on MSDN indicates that
                         * if a thumbnail is available it will return one, this has proved to not
                         * be the case in many situations while testing.
                         * - Solution: explicitly pass the ThumbnailOnly flag
                         */
                        image = GetThumbnail(path, ThumbnailOptions.ThumbnailOnly);
                    }
                }
                else
                {
                    type = ImageType.File;
                    image = GetThumbnail(path, ThumbnailOptions.ThumbnailOnly, loadFullImage ? FullIconSize : SmallIconSize);
                }
            }
            else
            {
                image = null;
            }


            return new ImageResult(image, type);
        }


        private static Bitmap GetThumbnail(string path, ThumbnailOptions option = ThumbnailOptions.ThumbnailOnly,
            int size = SmallIconSize)
        {
            return WindowsThumbnailProvider.GetThumbnail(
                path,
                size,
                size,
                option);
        }

        public static bool CacheContainImage(string path, bool loadFullImage = false)
        {
            return ImageCache.ContainsKey((path, loadFullImage));
        }

        public static async Task<Bitmap> LoadAsync(string path, bool loadFullImage = false)
        {
            var imageResult = await LoadInternalAsync(path, loadFullImage);

            var img = imageResult.ImageSource;

            return img;
        }

        private static Bitmap LoadFullImage(string path)
        {
            Bitmap image = new Bitmap(path);
            return image;
        }

        public Task<Bitmap> LoadImageAsync(string path)
        {
            return LoadAsync(path);
        }
    }
}