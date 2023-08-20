using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class TextureArrayRender
    {
        public GLTexture Output;

        Framebuffer Framebuffer;

        public GLTexture Render(GLTexture texture, int arrayLevel, int mipLevel, bool flip = false)
        {
            int width = 512;
            int height = 256;

            var shader = GlobalShaders.GetShader("TEXTURE_ARRAY");

            if (Output == null)
            {
                Framebuffer = new Framebuffer(FramebufferTarget.Framebuffer, width, height);

                Output = GLTexture2D.CreateUncompressedTexture(width, height, PixelInternalFormat.Rgba32f);
                Output.MipCount = texture.MipCount;

                texture.Bind();

                Output.Bind();
                Output.GenerateMipmaps();
                Output.Unbind();
            }
            Framebuffer.Bind();

            GL.Disable(EnableCap.Blend);

            shader.Enable();
            shader.SetTexture(texture, "textureArray", 1);

            int mipWidth = (int)(width * Math.Pow(0.5, mipLevel));
            int mipHeight = (int)(height * Math.Pow(0.5, mipLevel));

            if (Framebuffer.Width != mipWidth || Framebuffer.Height != mipHeight)
                Framebuffer.Resize(mipWidth, mipHeight);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                Output.ID, 0);

            shader.SetInt("arrayLevel", arrayLevel);
            shader.SetInt("mipLevel", mipLevel);
            shader.SetBoolToInt("flipY", flip);

            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, mipWidth, mipHeight);

            //Draw the texture onto the framebuffer
            ScreenQuadRender.Draw();

            return Output;
        }
    }
}
