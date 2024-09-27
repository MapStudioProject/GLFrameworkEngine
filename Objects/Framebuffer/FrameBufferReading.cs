using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Toolbox.Core.Imaging;
using GLFrameworkEngine.ImageSharp;

namespace GLFrameworkEngine
{
    public partial class Framebuffer
    {
        public byte[] ReadImageData(bool saveAlpha = false, ReadBufferMode bufferMode = ReadBufferMode.ColorAttachment0)
        {
            int imageSize = Width * Height * 4;

            Bind();
            byte[] pixels = ReadPixels(Width, Height, imageSize, bufferMode, saveAlpha);
            var bitmap = GetBitmap(Width, Height, pixels);

            var data = bitmap.GetSourceInBytes();
            bitmap.Dispose();

            return data;
        }

        public Image<Rgba32> ReadImagePixels(bool saveAlpha = false, ReadBufferMode bufferMode = ReadBufferMode.ColorAttachment0)
        {
            int imageSize = Width * Height * 4;

            Bind();
            byte[] pixels = ReadPixels(Width, Height, imageSize, bufferMode, saveAlpha);
            var bitmap = GetBitmap(Width, Height, pixels);

            // Adjust for differences in the origin point.
            bitmap.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));

            return bitmap;
        }

        private static byte[] ReadPixels(int width, int height, int imageSizeInBytes, ReadBufferMode bufferMode, bool saveAlpha)
        {
            byte[] pixels = new byte[imageSizeInBytes];

            // Read the pixels from the framebuffer. PNG uses the BGRA format. 
            GL.ReadBuffer(bufferMode);
            GL.ReadPixels(0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            if (!saveAlpha)
                SetAlphaToWhite(width, height, 4, pixels);

            return pixels;
        }

        private static void SetAlphaToWhite(int width, int height, int pixelSizeInBytes, byte[] pixels)
        {
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    int pixelIndex = w + (h * width);
                    pixels[pixelIndex * pixelSizeInBytes + 3] = 255;
                }
            }
        }

        public static Image<Rgba32> GetBitmap(int width, int height, byte[] imageData)
        {
            BitmapExtension.ConvertBgraToRgba(imageData);
            return Image.LoadPixelData<Rgba32>(imageData, width, height);
        }
    }
}