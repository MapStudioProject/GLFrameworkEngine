using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class LUTRender
    {
        public GLTexture2D Output => (GLTexture2D)Framebuffer.Attachments[0];

        private Framebuffer Framebuffer;

        public LUTRender()
        {
        }

        public void CreateTextureRender(GLTexture texture, int width, int height)
        {
            if (Framebuffer  == null)
            {
                Framebuffer = new Framebuffer(FramebufferTarget.Framebuffer, width, height);

                Output.Bind();
                Output.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
                Output.Unbind();
            }

            Framebuffer.Bind();

            var shader = GlobalShaders.GetShader("LUT_DISPLAY");

            GL.Disable(EnableCap.Blend);

            shader.Enable();
            shader.SetTexture(texture, "dynamic_texture_array", 1);

            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, width, height);

            //Draw the texture onto the framebuffer
            ScreenQuadRender.Draw();

            //Disable shader and textures
            GL.UseProgram(0);
            GL.BindTexture(TextureTarget.Texture3D, 0);
        }
    }
}
