using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using System.Linq;
using System.Runtime.InteropServices;

namespace GLFrameworkEngine.ImageSharp
{
    public static class ImageSharpExtension
    {
        public static byte[] GetSourceInBytes(this Image<Rgba32> image)
        {
            var _IMemoryGroup = image.GetPixelMemoryGroup();
            var data = _IMemoryGroup.SelectMany(row => MemoryMarshal.AsBytes(row.Span).ToArray())
                       .ToArray();

            return data;
        }
    }
}
