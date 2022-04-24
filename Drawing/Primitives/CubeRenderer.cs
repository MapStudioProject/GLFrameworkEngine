using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class CubeRenderer : RenderMesh<VertexPositionNormal>
    {
        public CubeRenderer(float size = 1.0f, PrimitiveType primitiveType = PrimitiveType.Triangles) :
            base(DrawingHelper.GetCubeVertices(size), Indices, primitiveType)
        {

        }

        public static int[] Indices = new int[]
        {
            // front face
            0, 1, 2, 2, 3, 0,
            // top face
            3, 2, 6, 6, 7, 3,
            // back face
            7, 6, 5, 5, 4, 7,
            // left face
            4, 0, 3, 3, 7, 4,
            // bottom face
            0, 1, 5, 5, 4, 0,
            // right face
            1, 5, 6, 6, 2, 1,
        };
    }
}
