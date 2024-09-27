using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Toolbox.Core.IO;
using Toolbox.Core.Imaging;
using GLFrameworkEngine.ImageSharp;

namespace GLFrameworkEngine
{
    public class GLTexture2D : GLTexture
    {
        public GLTexture2D() : base()
        {
            Target = TextureTarget.Texture2D;
        }

        public static GLTexture2D CreateUncompressedTextureWithMipmaps(int width, int height, int mipCount,
         PixelInternalFormat format = PixelInternalFormat.Rgba8,
         PixelFormat pixelFormat = PixelFormat.Rgba,
         PixelType pixelType = PixelType.UnsignedByte)
        {
            GLTexture2D texture = new GLTexture2D();
            texture.PixelInternalFormat = format;
            texture.PixelFormat = pixelFormat;
            texture.PixelType = pixelType;
            texture.Width = width; texture.Height = height;
            texture.Target = TextureTarget.Texture2D;
            texture.MagFilter = TextureMagFilter.Linear;
            texture.MinFilter = TextureMinFilter.LinearMipmapLinear;
            texture.MipCount = mipCount;
            texture.Bind();

            GL.TexParameter(texture.Target, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(texture.Target, TextureParameterName.TextureMaxLevel, texture.MipCount);

            GL.TexParameter(texture.Target, TextureParameterName.TextureLodBias, 0);
            GL.TexParameter(texture.Target, TextureParameterName.TextureMinLod, 0);
            GL.TexParameter(texture.Target, TextureParameterName.TextureMaxLod, texture.MipCount);

            texture.AllocateMipmaps();

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public static GLTexture2D CreateUncompressedTexture(int width, int height,
            PixelInternalFormat format = PixelInternalFormat.Rgba8,
            PixelFormat pixelFormat = PixelFormat.Rgba,
            PixelType pixelType = PixelType.UnsignedByte)
        {
            GLTexture2D texture = new GLTexture2D();
            texture.PixelInternalFormat = format;
            texture.PixelFormat = pixelFormat;
            texture.PixelType = pixelType;
            texture.Width = width; texture.Height = height;
            texture.Target = TextureTarget.Texture2D;
            texture.Bind();

            GL.TexImage2D(TextureTarget.Texture2D, 0, format,
                texture.Width, texture.Height,
                0, pixelFormat, pixelType, IntPtr.Zero);

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;

            Bind();

            GL.TexImage2D(Target, 0, PixelInternalFormat,
                      Width, Height,
                      0, PixelFormat, PixelType, IntPtr.Zero);

            if (MipCount > 1)
                AllocateMipmaps();

            Unbind();
        }

        private void AllocateMipmaps()
        {
            if (MipCount == 1)
                return;

            for (int i = 0; i < MipCount; i++)
            {
                int mwidth = Math.Max(1, (int)Width >> i);
                int mheight = Math.Max(1, (int)Height >> i);

                GL.TexImage2D(Target, i, PixelInternalFormat,
                    mwidth, mheight, 0, PixelFormat, PixelType, IntPtr.Zero);
            }
            GenerateMipmaps();
        }

        public static GLTexture2D CreateBlackTexture(int width = 4, int height = 4)
        {
            return CreateUncompressedTexture(width, height);
        }

        public static GLTexture2D CreateWhiteTexture(int width = 4, int height = 4)
        {
            GLTexture2D texture = new GLTexture2D();
            texture.PixelInternalFormat = PixelInternalFormat.Rgba8;
            texture.PixelFormat = PixelFormat.Rgba;
            texture.PixelType = PixelType.UnsignedByte;
            texture.Width = width; texture.Height = height;
            texture.Target = TextureTarget.Texture2D;
            texture.Bind();

            byte[] buffer = new byte[width * height * 4];
            for (int i = 0; i < width * height * 4; i++)
                buffer[i] = 255;

            GL.TexImage2D(TextureTarget.Texture2D, 0, texture.PixelInternalFormat,
                texture.Width, texture.Height,
                0, texture.PixelFormat, texture.PixelType, buffer);

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public static GLTexture2D CreateConstantColorTexture(int width, int height, byte R, byte G, byte B, byte A)
        {
            GLTexture2D texture = new GLTexture2D();
            texture.PixelInternalFormat = PixelInternalFormat.Rgba8;
            texture.PixelFormat = PixelFormat.Rgba;
            texture.PixelType = PixelType.UnsignedByte;
            texture.Width = width; texture.Height = height;
            texture.Target = TextureTarget.Texture2D;
            texture.Bind();

            byte[] buffer = new byte[width * height * 4];
            int offset = 0;
            for (int i = 0; i < width * height; i++)
            {
                buffer[offset] = R;
                buffer[offset + 1] = G;
                buffer[offset + 2] = B;
                buffer[offset + 3] = A;
                offset += 4;
            }

            GL.TexImage2D(TextureTarget.Texture2D, 0, texture.PixelInternalFormat,
                texture.Width, texture.Height,
                0, texture.PixelFormat, texture.PixelType, buffer);

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public static GLTexture2D CreateRGBATexture(byte[] buffer, int width, int height)
        {
            GLTexture2D texture = new GLTexture2D();
            texture.PixelInternalFormat = PixelInternalFormat.Rgba;
            texture.PixelFormat = PixelFormat.Rgba;
            texture.PixelType = PixelType.UnsignedByte;
            texture.Width = width; texture.Height = height;
            texture.Target = TextureTarget.Texture2D;
            texture.Bind();

            GL.TexImage2D(TextureTarget.Texture2D, 0, texture.PixelInternalFormat,
                texture.Width, texture.Height,
                0, texture.PixelFormat, texture.PixelType, buffer);

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public static GLTexture2D CreateFloatRGBA32Texture(int width, int height, float[] buffer)
        {
            GLTexture2D texture = new GLTexture2D();
            texture.PixelInternalFormat = PixelInternalFormat.Rgba32f;
            texture.PixelFormat = PixelFormat.Rgba;
            texture.PixelType = PixelType.Float;
            texture.Width = width; texture.Height = height;
            texture.Target = TextureTarget.Texture2D;
            texture.Bind();

            GL.TexImage2D(TextureTarget.Texture2D, 0, texture.PixelInternalFormat,
                texture.Width, texture.Height,
                0, texture.PixelFormat, texture.PixelType, buffer);

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public static GLTexture2D CreateFloat32Texture(int width, int height, float[] buffer)
        {
            GLTexture2D texture = new GLTexture2D();
            texture.PixelInternalFormat = PixelInternalFormat.R32f;
            texture.PixelFormat = PixelFormat.Red;
            texture.PixelType = PixelType.Float;
            texture.Width = width; texture.Height = height;
            texture.Target = TextureTarget.Texture2D;
            texture.Bind();

            GL.TexImage2D(TextureTarget.Texture2D, 0, texture.PixelInternalFormat,
                texture.Width, texture.Height,
                0, texture.PixelFormat, texture.PixelType, buffer);

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public static GLTexture2D FromGeneric(STGenericTexture texture, ImageParameters parameters = null)
        {
            if (parameters == null) parameters = new ImageParameters();

            GLTexture2D glTexture = new GLTexture2D();
            glTexture.Target = TextureTarget.Texture2D;
            glTexture.Width = (int)texture.Width;
            glTexture.Height = (int)texture.Height;
            glTexture.LoadImage(texture, parameters);
            return glTexture;
        }

        public static GLTexture2D FromBitmap(string imageFile)
        {
            var image = Image.Load<Rgba32>(imageFile);
            return FromBitmap(image);
        }

        public static GLTexture2D FromBitmap(byte[] imageFile, int width, int height)
        {
            var image = Image.Load<Rgba32>(imageFile);
            image.Mutate(x => x.Resize(width, height));
            return FromBitmap(image);
        }

        public static GLTexture2D FromBitmap(byte[] imageFile, bool isLinear = true)
        {
            var image = Image.Load<Rgba32>(imageFile);


            GLTexture2D texture = new GLTexture2D();
            texture.Target = TextureTarget.Texture2D;
            texture.Width = image.Width; texture.Height = image.Height;

            texture.Bind();
            if (isLinear)
            {
                texture.MagFilter = TextureMagFilter.Linear;
                texture.MinFilter = TextureMinFilter.Linear;
            }
            else
            {
                texture.MagFilter = TextureMagFilter.Nearest;
                texture.MinFilter = TextureMinFilter.Nearest;
            }

            texture.UpdateParameters();
            texture.LoadImage(image);
            return texture;
        }

        public static GLTexture2D FromBitmap(Image<Rgba32> image)
        {
            GLTexture2D texture = new GLTexture2D();
            texture.Target = TextureTarget.Texture2D;
            texture.Width = image.Width; texture.Height = image.Height;
            texture.LoadImage(image);
            return texture;
        }

        public void LoadImage(Image<Rgba32> image)
        {
            Bind();

            var rgba = image.GetSourceInBytes();

            GL.TexImage2D(Target, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, rgba);

            image.Dispose();

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            Unbind();
        }

        public void LoadImage(byte[] image)
        {
            Bind();

            GL.TexImage2D(Target, 0, PixelInternalFormat.Rgba, Width, Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, image);

            Unbind();
        }

        public void SetData<T>(T[] data) where T : struct
        {
            Bind();

            GL.TexImage2D(Target, 0, this.PixelInternalFormat, Width, Height, 0,
                this.PixelFormat, this.PixelType, data);

            Unbind();
        }

        public void Reload<T>(int width, int height, T[] data) where T : struct
        {
            Bind();

            GL.TexImage2D(Target, 0, this.PixelInternalFormat, width, height, 0,
                this.PixelFormat, this.PixelType, data);

            Unbind();
        }

        public System.IO.Stream ToStream(bool saveAlpha = false)
        {
            var stream = new System.IO.MemoryStream();
            var bmp = ToBitmap(saveAlpha);
            bmp.SaveAsPng(stream);
            return stream;
        }

        public override void SaveDDS(string fileName)
        {
            List<STGenericTexture.Surface> surfaces = new List<STGenericTexture.Surface>();

            Bind();

            var surface = new STGenericTexture.Surface();
            surfaces.Add(surface);

            for (int m = 0; m < this.MipCount; m++)
            {
                int mipWidth = (int)(Width * Math.Pow(0.5, m));
                int mipHeight = (int)(Height * Math.Pow(0.5, m));

                byte[] outputRaw = new byte[mipWidth * mipHeight * 4];
                GL.GetTexImage(this.Target, m,
                  PixelFormat.Bgra, PixelType.UnsignedByte, outputRaw);

                surface.mipmaps.Add(outputRaw);
            }

            var dds = new DDS();
            dds.MainHeader.Width = (uint)this.Width;
            dds.MainHeader.Height = (uint)this.Height;
            dds.MainHeader.Depth = 1;
            dds.MainHeader.MipCount = (uint)this.MipCount;
            dds.MainHeader.PitchOrLinearSize = (uint)surfaces[0].mipmaps[0].Length;

            dds.SetFlags(TexFormat.RGBA8_UNORM, false, false);

            if (dds.IsDX10)
            {
                if (dds.Dx10Header == null)
                    dds.Dx10Header = new DDS.DX10Header();

                dds.Dx10Header.ResourceDim = 3;
                dds.Dx10Header.ArrayCount = 1;
            }

            dds.Save(fileName, surfaces);

            Unbind();
        }

        public void Save(string fileName, bool saveAlpha = false)
        {
            Bind();

            var bmp = ToBitmap(saveAlpha);
            bmp.Save(fileName);

            Unbind();
        }

        public override Image<Rgba32> ToBitmap(bool saveAlpha = true)
        {
            Bind();

            byte[] data = GetBytes(0);

            var image = Image.LoadPixelData<Rgba32>(data, Width, Height);

            Unbind();

            return image;
        }
    }
}
