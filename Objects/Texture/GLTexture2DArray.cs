using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GLFrameworkEngine
{
    public class GLTexture2DArray : GLTexture
    {
        public GLTexture2DArray() : base()
        {
            Target = TextureTarget.Texture2DArray;
        }

        public static GLTexture2DArray From2DTexture(GLTexture2D tex)
        {
            GLTexture2DArray texture = new GLTexture2DArray();
            texture.PixelFormat = tex.PixelFormat;
            texture.PixelType = tex.PixelType;
            texture.PixelInternalFormat = tex.PixelInternalFormat;
            texture.Width = tex.Width; texture.Height = tex.Height;
            texture.Target = TextureTarget.Texture2DArray;
            texture.MinFilter = TextureMinFilter.Linear;
            texture.MagFilter = TextureMagFilter.Linear;
            texture.MipCount = tex.MipCount;
            texture.ArrayCount = 1;
            texture.Bind();

            //Allocate mip data
            if (texture.MipCount > 1)
            {
                texture.MinFilter = TextureMinFilter.LinearMipmapLinear;
                texture.GenerateMipmaps();
            }

            GL.CopyTexImage2D(tex.Target, 0, (InternalFormat)tex.PixelInternalFormat, 0, 0, tex.Width, tex.Height, 0);

           /* for (int array = 0; array < texture.ArrayCount; array++)
            {
                for (int mip = 0; mip < texture.MipCount; mip++)
                {
                    int mipWidth = (int)(texture.Width * Math.Pow(0.5, mip));
                    int mipHeight = (int)(texture.Height * Math.Pow(0.5, mip));

                    GL.TexImage3D(texture.Target, array, texture.PixelInternalFormat,
                        mipWidth, mipHeight, texture.ArrayCount, mip,
                          texture.PixelFormat, texture.PixelType, IntPtr.Zero);
                }
            }*/

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public static GLTexture2DArray CreateUncompressedTexture(int width, int height, int arrayCount, int mipCount,
            PixelInternalFormat pixelInternalFormat = PixelInternalFormat.Rgba8,
            PixelFormat pixelFormat = PixelFormat.Rgba,
            PixelType pixelType = PixelType.UnsignedByte)
        {
            GLTexture2DArray texture = new GLTexture2DArray();
            texture.PixelFormat = pixelFormat;
            texture.PixelType = pixelType;
            texture.PixelInternalFormat = pixelInternalFormat;
            texture.Width = width; texture.Height = height;
            texture.Target = TextureTarget.Texture2DArray;
            texture.MinFilter = TextureMinFilter.Linear;
            texture.MagFilter = TextureMagFilter.Linear;
            texture.MipCount = mipCount;
            texture.ArrayCount = arrayCount;
            texture.Bind();

            GL.TexImage3D(texture.Target, 0, texture.PixelInternalFormat,
                         texture.Width, texture.Height, texture.ArrayCount, 0,
                           texture.PixelFormat, texture.PixelType, IntPtr.Zero);

            //Allocate mip data
            if (texture.MipCount > 1)
            {
                texture.MinFilter = TextureMinFilter.LinearMipmapLinear;
                texture.GenerateMipmaps();
            }

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public static GLTexture2DArray CreateConstantColorTexture(int width, int height, int count, byte R, byte G, byte B, byte A)
        {
            GLTexture2DArray texture = new GLTexture2DArray();
            texture.PixelInternalFormat = PixelInternalFormat.Rgba8;
            texture.PixelFormat = PixelFormat.Rgba;
            texture.PixelType = PixelType.UnsignedByte;
            texture.Width = width; texture.Height = height;
            texture.Target = TextureTarget.Texture2DArray;
            texture.Bind();

            byte[] buffer = new byte[width * height * count * 4];
            int offset = 0;
            for (int i = 0; i < width * height * count; i++)
            {
                buffer[offset] = R;
                buffer[offset + 1] = G;
                buffer[offset + 2] = B;
                buffer[offset + 3] = A;
                offset += 4;
            }

            GL.TexImage3D(texture.Target, 0, texture.PixelInternalFormat,
                texture.Width, texture.Height, count,
                0, texture.PixelFormat, texture.PixelType, buffer);

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public static GLTexture2DArray FromGeneric(STGenericTexture texture, ImageParameters parameters)
        {
            GLTexture2DArray glTexture = new GLTexture2DArray();
            glTexture.Width = (int)texture.Width;
            glTexture.Height = (int)texture.Height;
            glTexture.ArrayCount = (int)texture.ArrayCount;
            glTexture.LoadImage(texture, parameters);
            return glTexture;
        }

        public static GLTexture2DArray FromDDS(DDS dds)
        {
            GLTexture2DArray texture = new GLTexture2DArray();
            texture.Width = (int)dds.Width; texture.Height = (int)dds.Height;
            texture.Bind();

            GL.TexParameter(texture.Target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(texture.Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(texture.Target, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(texture.Target, TextureParameterName.TextureMaxLevel, 13);
            GL.TexParameter(texture.Target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(texture.Target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(texture.Target, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            InternalFormat format = InternalFormat.Rgba8;

            for (int j = 0; j < dds.MipCount; j++)
            {
                int mipWidth = CalculateMipDimension(texture.Width, j);
                int mipHeight = CalculateMipDimension(texture.Height, j);
                int imageSize = GLFormatHelper.CalculateImageSize(mipWidth, mipHeight, format) * (int)dds.ArrayCount;

                List<byte[]> levels = new List<byte[]>();
                for (int i = 0; i < dds.ArrayCount; i++)
                {
                    var data = dds.GetDecodedSurface(i, j);
                    if (i == 0 || i == 1)
                        data = FlipHorizontal(mipWidth, mipHeight, data);

                    levels.Add(data);
                }

                var surface = ByteUtils.CombineArray(levels.ToArray());
                if (format == InternalFormat.Rgba8)
                {
                    GL.TexImage3D(TextureTarget.Texture2DArray, j,
                        PixelInternalFormat.Rgba,
                        mipWidth, mipHeight, (int)dds.ArrayCount, 0, PixelFormat.Bgra, PixelType.UnsignedByte,
                        surface);
                }
                else
                {
                    GL.CompressedTexImage3D(TextureTarget.Texture2DArray, j,
                        format,
                        mipWidth, mipHeight, (int)dds.ArrayCount,
                        0, imageSize, surface);
                }
            }

            if (dds.MipCount > 0)
                GL.GenerateMipmap((GenerateMipmapTarget)texture.Target);

            texture.Unbind();
            return texture;
        }

        private static byte[] FlipHorizontal(int Width, int Height, byte[] Input)
        {
            byte[] FlippedOutput = new byte[Width * Height * 4];

            for (int Y = 0; Y < Height; Y++)
            {
                for (int X = 0; X < Width; X++)
                {
                    int IOffs = (Y * Width + X) * 4;
                    int OOffs = (Y * Width + Width - 1 - X) * 4; 

                    FlippedOutput[OOffs + 0] = Input[IOffs + 0];
                    FlippedOutput[OOffs + 1] = Input[IOffs + 1];
                    FlippedOutput[OOffs + 2] = Input[IOffs + 2];
                    FlippedOutput[OOffs + 3] = Input[IOffs + 3];
                }
            }
            return FlippedOutput;
        }

        private static byte[] FlipVertical(int Width, int Height, byte[] Input)
        {
            byte[] FlippedOutput = new byte[Width * Height * 4];

            int Stride = Width * 4;
            for (int Y = 0; Y < Height; Y++)
            {
                int IOffs = Stride * Y;
                int OOffs = Stride * (Height - 1 - Y);

                for (int X = 0; X < Width; X++)
                {
                    FlippedOutput[OOffs + 0] = Input[IOffs + 0];
                    FlippedOutput[OOffs + 1] = Input[IOffs + 1];
                    FlippedOutput[OOffs + 2] = Input[IOffs + 2];
                    FlippedOutput[OOffs + 3] = Input[IOffs + 3];

                    IOffs += 4;
                    OOffs += 4;
                }
            }
            return FlippedOutput;
        }

        public static GLTexture2DArray FromDDS(DDS[] dds, bool flipY = false)
        {
            GLTexture2DArray texture = new GLTexture2DArray();
            texture.Width = (int)dds[0].Width; texture.Height = (int)dds[0].Height;
            texture.Bind();

            GL.TexParameter(texture.Target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(texture.Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(texture.Target, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(texture.Target, TextureParameterName.TextureMaxLevel, 1);
            GL.TexParameter(texture.Target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(texture.Target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(texture.Target, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            InternalFormat format = InternalFormat.Rgba8;
            for (int j = 0; j < dds.Length; j++)
            {
                int mipWidth = CalculateMipDimension(texture.Width, j);
                int mipHeight = CalculateMipDimension(texture.Height, j);
                int imageSize = GLFormatHelper.CalculateImageSize(mipWidth, mipHeight, format) * (int)dds[0].ArrayCount;

                List<byte[]> levels = new List<byte[]>();
                for (int i = 0; i < dds[0].ArrayCount; i++)
                {
                    var data = dds[j].GetDecodedSurface(i, 0);
                    if (i == 0 || i == 1)
                        data = FlipHorizontal(mipWidth, mipHeight, data);

                    levels.Add(data);
                }

                var surface = ByteUtils.CombineArray(levels.ToArray());
                if (format == InternalFormat.Rgba8)
                {
                    GL.TexImage3D(TextureTarget.Texture2DArray, j,
                        PixelInternalFormat.Rgba,
                        mipWidth, mipHeight, (int)dds[0].ArrayCount, 0, PixelFormat.Bgra, PixelType.UnsignedByte,
                        surface);
                }
                else
                {
                    GL.CompressedTexImage3D(TextureTarget.Texture2DArray, j,
                        format,
                        mipWidth, mipHeight, (int)dds[0].ArrayCount,
                        0, imageSize, surface);
                }
            }

            texture.Unbind();
            return texture;
        }

        public static GLTexture2DArray FromBitmap(Image<Rgba32> image)
        {
            GLTexture2DArray texture = new GLTexture2DArray();
            texture.Width = image.Width; texture.Height = image.Height;
            texture.LoadImage(image);
            return texture;
        }

        public static GLTexture2DArray FromRawData(int width, int height, TexFormat format, byte[] data)
        {
            GLTexture2DArray texture = new GLTexture2DArray();
            texture.Width = width; texture.Height = height;
            texture.LoadImage(width, height, format, data);
            return texture;
        }

        public void LoadImage(int width, int height, TexFormat format, byte[] data)
        {
            if (TextureFormatHelper.IsBCNCompressed(format))
            {
                var internalFormat = GLFormatHelper.ConvertCompressedFormat(format, true);
                int imageSize = GLFormatHelper.CalculateImageSize(width, height, internalFormat);

                GL.CompressedTexImage3D(TextureTarget.Texture2DArray, 0,
                internalFormat, width, height, 1, 0, imageSize, data);
            }
            else
            {
                var formatInfo = GLFormatHelper.ConvertPixelFormat(format);

                GL.TexImage3D(Target, 0, formatInfo.InternalFormat, width, height, 1, 0,
                      formatInfo.Format, formatInfo.Type, data);
            }
        }

        public void LoadImage(Image<Rgba32> image, int level = 0, int count = 1)
        {
            Bind();

            var rgba = image.GetSourceInBytes();

            GL.TexImage3D(Target, level, PixelInternalFormat.Rgba, image.Width, image.Height, count, 0,
                  OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, rgba);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

            Unbind();
        }

        public void LoadImage(byte[] image)
        {
            Bind();

            GL.TexImage3D(Target, 0, PixelInternalFormat.Rgba, Width, Height, 1, 0,
           OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, image);

            Unbind();
        }

        public System.IO.Stream ToStream(bool saveAlpha = false)
        {
            var stream = new System.IO.MemoryStream();
            var bmp = ToBitmap(saveAlpha);
            bmp.SaveAsPng(stream);
            return stream;
        }

        public void Save(string fileName, bool saveAlpha = false)
        {
            Bind();
            for (int i = 0; i < 6; i++)
            {
                byte[] output = new byte[Width * Height * 4];
                GL.GetTextureSubImage(this.ID, 0, 0, 0, i, Width, Height, 1,
                    PixelFormat.Bgra, PixelType.UnsignedByte, output.Length, output);

                var bitmap = Image.LoadPixelData<Rgba32>(output, Width, Height);
                bitmap.Save(fileName + $"_{i}.png");
            }
            Unbind();
        }

        public override void SaveDDS(string fileName)
        {
            List<STGenericTexture.Surface> surfaces = new List<STGenericTexture.Surface>();

            Bind();

            for (int i = 0; i < this.ArrayCount; i++)
            {
                var surface = new STGenericTexture.Surface();
                surfaces.Add(surface);

                for (int m = 0; m < this.MipCount; m++)
                {
                    int mipW = (int)(this.Width * Math.Pow(0.5, m));
                    int mipH = (int)(this.Height * Math.Pow(0.5, m));

                    byte[] outputRaw = new byte[mipW * mipH * 4];
                    GL.GetTextureSubImage(this.ID, m, 0, 0, i, mipW, mipH, 1,
                     PixelFormat.Rgba, PixelType.UnsignedByte, outputRaw.Length, outputRaw);

                    surface.mipmaps.Add(outputRaw);
                }
            }

            var dds = new DDS();
            dds.MainHeader.Width = (uint)this.Width;
            dds.MainHeader.Height = (uint)this.Height;
            dds.MainHeader.Depth = 1;
            dds.MainHeader.MipCount = (uint)this.MipCount;
            dds.MainHeader.PitchOrLinearSize = (uint)surfaces[0].mipmaps[0].Length;

            dds.SetFlags(TexFormat.RGBA8_UNORM, false, true);

            if (dds.IsDX10)
            {
                if (dds.Dx10Header == null)
                    dds.Dx10Header = new DDS.DX10Header();

                dds.Dx10Header.ResourceDim = 3;
                dds.Dx10Header.ArrayCount = (uint)surfaces.Count;
            }

            dds.Save(fileName, surfaces);

            Unbind();
        }
    }
}
