using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents a framebuffer for displaying projected shadows onto.
    /// </summary>
    public class ShadowFrameBuffer : Framebuffer
    {
        /// <summary>
        /// The depth texture of the framebuffer depth attachement for projected shadows.
        /// </summary>
        public DepthTexture GetShadowTexture() => (DepthTexture)Attachments[0];

        public ShadowFrameBuffer(int width, int height) : base(FramebufferTarget.Framebuffer)
        {
            this.Width = width;
            this.Height = height;

            Bind();
            this.PixelInternalFormat = PixelInternalFormat.DepthComponent24;
            this.SetDrawBuffers(DrawBuffersEnum.None);
            this.SetReadBuffer(ReadBufferMode.None);

            GLTexture shadowTexture = new DepthTexture(width, height, PixelInternalFormat.DepthComponent24);
            AddAttachment(FramebufferAttachment.DepthAttachment, shadowTexture);

            shadowTexture.Bind();
            shadowTexture.MagFilter = TextureMagFilter.Linear;
            shadowTexture.MinFilter = TextureMinFilter.Linear;
            shadowTexture.WrapS = TextureWrapMode.ClampToBorder;
            shadowTexture.WrapT = TextureWrapMode.ClampToBorder;
            shadowTexture.UpdateParameters();

            float[] borderColor = new float[4] { 1, 1, 1, 1 };
            GL.TexParameter(shadowTexture.Target, TextureParameterName.TextureBorderColor, borderColor);
            GL.TexParameter(shadowTexture.Target, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
            GL.TexParameter(shadowTexture.Target, TextureParameterName.TextureCompareFunc, (int)All.Lequal);
            GL.TexParameter(shadowTexture.Target, TextureParameterName.DepthTextureMode, (int)All.Intensity);

            shadowTexture.Unbind();

            Unbind();
        }
    }
}
