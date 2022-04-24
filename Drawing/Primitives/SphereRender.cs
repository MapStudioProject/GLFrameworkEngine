using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class SphereRender : RenderMesh<VertexPositionNormal>
    {
        public SphereRender(float radius = 20, float u_segments = 30, float v_segments = 30, PrimitiveType primitiveType = PrimitiveType.TriangleStrip) :
            base(DrawingHelper.GetSphereVertices(radius, u_segments, v_segments), primitiveType)
        {

        }
    }
}
