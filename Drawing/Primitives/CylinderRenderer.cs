using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class CylinderRenderer : RenderMesh<VertexPositionNormal>
    {
        public CylinderRenderer(float radius, float startHeight, float height)
            : base(DrawingHelper.GetCylinderVertices(radius, startHeight, height, 32),
                  PrimitiveType.Triangles)
        {

        }
    }
}
