using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace GLFrameworkEngine
{
    public class GLTexture3D : GLTexture
    {
        public int Depth { get; set; }

        public GLTexture3D() : base()
        {
            Target = TextureTarget.Texture3D;
        }

        public static GLTexture3D CreateUncompressedTexture(int width, int height, int depth,
            PixelInternalFormat format = PixelInternalFormat.Rgba8,
            PixelFormat pixelFormat = PixelFormat.Rgba,
            PixelType pixelType = PixelType.UnsignedByte)
        {
            GLTexture3D texture = new GLTexture3D();
            texture.PixelFormat = pixelFormat;
            texture.PixelType = pixelType;
            texture.Width = width; 
            texture.Height = height;
            texture.Depth = depth;
            texture.Target = TextureTarget.Texture3D;
            texture.Bind();

            GL.TexImage3D(TextureTarget.Texture3D, 0, format,
                texture.Width, texture.Height, texture.Depth,
                0, pixelFormat, pixelType, IntPtr.Zero);

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public static GLTexture3D FromGeneric(STGenericTexture texture, ImageParameters parameters)
        {
            GLTexture3D glTexture = new GLTexture3D();
            glTexture.Target = TextureTarget.Texture3D;
            glTexture.Width = (int)texture.Width;
            glTexture.Height = (int)texture.Height;
            glTexture.LoadImage(texture, parameters);
            return glTexture;
        }

        public override void SaveDDS(string fileName)
        {
            List<STGenericTexture.Surface> surfaces = new List<STGenericTexture.Surface>();

            Bind();

            for (int i = 0; i < this.Depth; i++)
            {
                var surface = new STGenericTexture.Surface();
                surfaces.Add(surface);

                for (int m = 0; m < this.MipCount; m++)
                {
                    int mipWidth = (int)(Width * Math.Pow(0.5, m));
                    int mipHeight = (int)(Height * Math.Pow(0.5, m));

                    byte[] outputRaw = new byte[mipWidth * mipHeight * 4];

                    GL.GetTextureSubImage(this.ID, m, 0, 0, i, Width, Height, 1,
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
            dds.ArrayCount = (uint)this.Depth;

            dds.SetFlags(TexFormat.RGBA8_UNORM, true, false);

            if (dds.IsDX10)
            {
                if (dds.Dx10Header == null)
                    dds.Dx10Header = new DDS.DX10Header();

                dds.Dx10Header.ResourceDim = 3;
                dds.Dx10Header.ArrayCount = (uint)this.Depth;
            }

            dds.Save(fileName, surfaces);

            Unbind();
        }

        public void Save(string fileName)
        {
            Bind();

            var bmp = ToBitmap();
            bmp.Save(fileName);

            Unbind();
        }

        public override Image<Rgba32> ToBitmap(bool saveAlpha = true)
        {
            Bind();

            byte[] data = new byte[Width * Height * Depth * 4];
            GL.GetTexImage(Target, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data);

            var image = Image.LoadPixelData<Rgba32>(data, Width * Depth, Height);
            image.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));

            Unbind();

            return image;
        }
    }
}
