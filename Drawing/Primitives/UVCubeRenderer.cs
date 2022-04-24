using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class UVCubeRenderer : RenderMesh<VertexPositionNormalTexCoord>
    {
        public UVCubeRenderer(float size = 1.0f, PrimitiveType primitiveType = PrimitiveType.Triangles) :
            base(GetVertices(size), Indices, primitiveType)
        {

        }

        static VertexPositionNormalTexCoord[] GetVertices(float size)
        {
            VertexPositionNormalTexCoord[] vertices = Vertices.ToArray();
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Position *= size;
            return vertices;
        }

        static VertexPositionNormalTexCoord[] Vertices = new VertexPositionNormalTexCoord[]
         {
            new VertexPositionNormalTexCoord(new Vector3(-1f, 1f, 1f), new Vector3(-1f, 0f, 0f), new Vector2(0.250018f, 0.99999f)),
            new VertexPositionNormalTexCoord(new Vector3(-1f, -1f, -1f), new Vector3(-1f, 0f, 0f), new Vector2(0.00016f, 0.500273f)),
            new VertexPositionNormalTexCoord(new Vector3(-1f, -1f, 1f), new Vector3(-1f, 0f, 0f), new Vector2(0.250018f, 0.500273f)),
            new VertexPositionNormalTexCoord(new Vector3(-1f, 1f, -1f), new Vector3(0f, 0f, -1f), new Vector2(0.500048f, 1f)),
            new VertexPositionNormalTexCoord(new Vector3(1f, -1f, -1f), new Vector3(0f, 0f, -1f), new Vector2(0.250073f, 0.500051f)),
            new VertexPositionNormalTexCoord(new Vector3(-1f, -1f, -1f), new Vector3(0f, 0f, -1f), new Vector2(0.500048f, 0.500051f)),
            new VertexPositionNormalTexCoord(new Vector3(1f, 1f, -1f), new Vector3(1f, 0f, 0f), new Vector2(0.749886f, 0.999726f)),
            new VertexPositionNormalTexCoord(new Vector3(1f, -1f, 1f), new Vector3(1f, 0f, 0f), new Vector2(0.500027f, 0.500009f)),
            new VertexPositionNormalTexCoord(new Vector3(1f, -1f, -1f), new Vector3(1f, 0f, 0f), new Vector2(0.749886f, 0.500009f)),
            new VertexPositionNormalTexCoord(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 1f), new Vector2(0.999994f, 1f)),
            new VertexPositionNormalTexCoord(new Vector3(-1f, -1f, 1f), new Vector3(0f, 0f, 1f), new Vector2(0.750019f, 0.500051f)),
            new VertexPositionNormalTexCoord(new Vector3(1f, -1f, 1f), new Vector3(0f, 0f, 1f), new Vector2(0.999994f, 0.500051f)),
            new VertexPositionNormalTexCoord(new Vector3(1f, -1f, -1f), new Vector3(0f, -1f, 0f), new Vector2(0.250004f, 5.1E-05f)),
            new VertexPositionNormalTexCoord(new Vector3(-1f, -1f, 1f), new Vector3(0f, -1f, 0f), new Vector2(0.499979f, 0.500001f)),
            new VertexPositionNormalTexCoord(new Vector3(-1f, -1f, -1f), new Vector3(0f, -1f, 0f), new Vector2(0.250004f, 0.500001f)),
            new VertexPositionNormalTexCoord(new Vector3(-1f, 1f, -1f), new Vector3(0f, 1f, 0f), new Vector2(2.6E-05f, 4.8E-05f)),
            new VertexPositionNormalTexCoord(new Vector3(1f, 1f, 1f), new Vector3(0f, 1f, 0f), new Vector2(0.250001f, 0.499998f)),
            new VertexPositionNormalTexCoord(new Vector3(1f, 1f, -1f), new Vector3(0f, 1f, 0f), new Vector2(2.6E-05f, 0.499998f)),
            new VertexPositionNormalTexCoord(new Vector3(-1f, 1f, -1f), new Vector3(-1f, 0f, 0f), new Vector2(0.00016f, 0.99999f)),
            new VertexPositionNormalTexCoord(new Vector3(1f, 1f, -1f), new Vector3(0f, 0f, -1f), new Vector2(0.250073f, 1f)),
            new VertexPositionNormalTexCoord(new Vector3(1f, 1f, 1f), new Vector3(1f, 0f, 0f), new Vector2(0.500027f, 0.999726f)),
            new VertexPositionNormalTexCoord(new Vector3(-1f, 1f, 1f), new Vector3(0f, 0f, 1f), new Vector2(0.750019f, 1f)),
            new VertexPositionNormalTexCoord(new Vector3(1f, -1f, 1f), new Vector3(0f, -1f, 0f), new Vector2(0.499979f, 5.1E-05f)),
            new VertexPositionNormalTexCoord(new Vector3(-1f, 1f, 1f), new Vector3(0f, 1f, 0f), new Vector2(0.250001f, 4.8E-05f)),
         };

        static int[] Indices = new int[]
        {
                0, 1,
                2, 3, 4, 5, 6, 7,
                8, 9, 10, 11, 12, 13,
                14, 15, 16, 17, 0, 18,
                1, 3, 19, 4, 6, 20,
                7, 9, 21, 10, 12, 22,
                13, 15, 23, 16,
        };
    }
}
