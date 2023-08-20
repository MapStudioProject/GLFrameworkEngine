using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class DepthCascadeTexture : GLTexture, IFramebufferAttachment
    {
        public DepthCascadeTexture(int width, int height, int count, PixelInternalFormat pixelInternalFormat) 
            : base()
        {
            Target = TextureTarget.Texture2DArray;
            PixelInternalFormat = pixelInternalFormat;
            PixelFormat = PixelFormat.DepthComponent;
            PixelType = PixelType.Float;
            Width = width;
            Height = height;
            ArrayCount = count;

            // Set texture settings.
            Bind();

            GL.TexImage3D(Target, 0, PixelInternalFormat,
                        Width, Height, ArrayCount, 0,
                          PixelFormat, PixelType, IntPtr.Zero);

            MagFilter = TextureMagFilter.Nearest;
            MinFilter = TextureMinFilter.Nearest;

            // Use white for values outside the depth map's border.
            WrapS = TextureWrapMode.ClampToBorder;
            WrapT = TextureWrapMode.ClampToBorder;
            UpdateParameters();

            GL.TexParameter(Target, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.None);
            GL.TexParameter(Target, TextureParameterName.TextureBorderColor, new float[] { 1, 1, 1, 1 });
        //    GL.TexParameter(Target, TextureParameterName.TextureCompareFunc, (int)All.Lequal);

            Unbind();
        }

        public override void Attach(FramebufferAttachment attachment, Framebuffer target)
        {
            target.Bind();
            GL.FramebufferTextureLayer(target.Target, attachment, ID, 0, 0);
        }

        public void AttachDepth(Framebuffer target, int layer)
        {
            target.Bind();
            GL.FramebufferTextureLayer(target.Target, FramebufferAttachment.DepthAttachment, ID, 0, layer);
        }
    }
}
