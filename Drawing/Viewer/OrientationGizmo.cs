using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core.ViewModels;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents an orientation view cube for displaying the camera orientation.
    /// This cube is automatically drawn to the bottom left corner of the current viewport.
    /// </summary>
    public class OrientationGizmo
    {
        UVCubeRenderer CubeRenderer = null;
        StandardMaterial Material = new StandardMaterial();

        GLTexture DisplayCubeTexture = null;

        public void Draw(GLContext context)
        {
            Prepare();

            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            //Smaller view space (15% of the screen)
            var width = (int)(context.Width * 0.15f);
            var height = (int)(context.Height * 0.15f);
            //Get the center
            var centerX = width / 2.0f;
            var centerY = height / 2.0f;
            //Place the object to the bottom left side. Shift it to fit based on the cube size.
            Matrix4 translationMatrix = Matrix4.CreateTranslation(-centerX + 6, -centerY + 6, 0);
            Matrix4 scaleMatrix = Matrix4.CreateScale(0.3f);
            //Use only the rotation in the view matrix.
            Matrix4 viewMatrix = context.Camera.ViewMatrix.ClearTranslation() * translationMatrix;
            //Use an ortho view covering the entire view space.
            Matrix4 projMatrix = Matrix4.CreateOrthographic(width, height, -200f, 200f);

            Material.DiffuseTextureID = DisplayCubeTexture.ID;
            Material.CameraMatrix = viewMatrix * projMatrix;
            Material.ModelMatrix = scaleMatrix;
            Material.Render(context);

            CubeRenderer.Draw(context);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void Prepare()
        {
            if (CubeRenderer == null || CubeRenderer.IsDisposed)
                CubeRenderer = new UVCubeRenderer(10);
            if (DisplayCubeTexture == null)
                DisplayCubeTexture = GLTexture2D.FromBitmap(Properties.Resources.TexturedCube);
        }

        public void Dispose()
        {
            CubeRenderer?.Dispose();
            DisplayCubeTexture?.Dispose();
        }
    }
}
