using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class CylinderRenderer : RenderMesh<VertexPositionNormal>
    {
        public CylinderRenderer(float radius, float startHeight, float height, float div = 32, PrimitiveType type = PrimitiveType.Triangles)
            : base(DrawingHelper.GetCylinderVertices(radius, startHeight, height, div),
                  type)
        {

        }
    }
}
