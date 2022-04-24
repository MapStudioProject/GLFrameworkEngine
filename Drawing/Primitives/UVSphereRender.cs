using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class UVSphereRender : RenderMesh<VertexPositionNormalTexCoord>
    {
        public UVSphereRender(float radius = 20, float u_segments = 30, float v_segments = 30) :
            base(DrawingHelper.GetUVSphereVertices(radius, u_segments, v_segments), PrimitiveType.TriangleStrip)
        {

        }
    }
}
