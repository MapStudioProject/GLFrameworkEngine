using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class CylinderRenderer : RenderMesh<VertexPositionNormal>
    {
        public CylinderRenderer(float radius, float height)
            : base(DrawingHelper.GetCylinderVertices(radius, height, 32),
                  PrimitiveType.Triangles)
        {

        }
    }
}
