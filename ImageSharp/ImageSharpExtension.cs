using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;

namespace GLFrameworkEngine
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
