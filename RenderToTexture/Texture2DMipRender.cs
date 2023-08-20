using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class Texture2DMipRender
    {
        public GLTexture Output;

        Framebuffer Framebuffer;

        public GLTexture Render(GLTexture2D texture, int mipLevel, bool flipY = false)
        {
            int width = 400;
            int height = 300;

            var shader = GlobalShaders.GetShader("TEXTURE");

            if (Output == null || Framebuffer == null)
            {
                Output = GLTexture2D.CreateUncompressedTexture(width, height, PixelInternalFormat.Rgba32f);
                Framebuffer = new Framebuffer(FramebufferTarget.Framebuffer, width, height);
            }

            Framebuffer.Bind();

            GL.Disable(EnableCap.Blend);

            shader.Enable();

            texture.Bind();
            GL.TexParameter(texture.Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            texture.Unbind();

            shader.SetTexture(texture, "textureValue", 1);
            shader.SetBoolToInt("flipY", flipY);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                Output.ID, 0);

            shader.SetInt("mipLevel", mipLevel);

            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, width, height);

            //Draw the texture onto the framebuffer
            ScreenQuadRender.Draw();

            texture.Bind();
            GL.TexParameter(texture.Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            texture.Unbind();


            return Output;
        }
    }
}
