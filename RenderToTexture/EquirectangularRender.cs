using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class EquirectangularRender
    {
        public GLTexture Texture => (GLTexture)Framebuffer.Attachments[0];
        public Framebuffer Framebuffer;

        public void Render(GLTexture texture, int arrayLevel, int mipLevel, bool hdrEncode = false)
        {
            int width = 512;
            int height = 256;

            if (Framebuffer == null)
                Framebuffer = new Framebuffer(FramebufferTarget.Framebuffer, width, height, PixelInternalFormat.Rgba, 1, false);

            var shader = GlobalShaders.GetShader("EQUIRECTANGULAR");
            Framebuffer.Bind();

            GL.Disable(EnableCap.Blend);

            shader.Enable();
            shader.SetBoolToInt("is_array", texture is GLTextureCubeArray);

            if (texture is GLTextureCubeArray)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                texture.Bind();
                shader.SetInt("dynamic_texture_array", 1);
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture2);
                texture.Bind();
                shader.SetInt("dynamic_texture", 2);
            }

            int mipWidth = (int)(width * Math.Pow(0.5, mipLevel));
            int mipHeight = (int)(height * Math.Pow(0.5, mipLevel));

            shader.SetInt("arrayLevel", arrayLevel);
            shader.SetInt("mipLevel", mipLevel);

            shader.SetBoolToInt("hdr_decode", hdrEncode);
            shader.SetFloat("scale", 4);
            shader.SetFloat("range", 1024.0f);
            shader.SetFloat("gamma", 2.2f);

            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, width, height);

            //Draw the texture onto the framebuffer
            ScreenQuadRender.Draw();
        }
    }
}
