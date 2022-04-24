using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace GLFrameworkEngine
{
    public class OrientationCubeDrawer : UVCubeRenderer, IDisposable
    {
        StandardMaterial Material = new StandardMaterial();

        public OrientationCubeDrawer(float size = 10) : base(size)
        {

        }

        public void DrawModel(GLContext context, Matrix4 matrix, bool selected)
        {
            Material.DiffuseTextureID = RenderTools.TexturedCubeTex.ID;
            Material.DisplaySelection = selected;
            Material.HalfLambertShading = false;
            Material.ModelMatrix = matrix;
            Material.Render(context);

            DrawWithSelection(context, selected);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
