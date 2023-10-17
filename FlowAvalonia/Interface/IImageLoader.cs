using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace FlowAvalonia.PublicAPI;

public interface IImageLoader
{
    public Task<Bitmap> LoadImageAsync(string path);
}