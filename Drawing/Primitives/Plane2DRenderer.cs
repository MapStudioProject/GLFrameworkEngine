using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
   public class Plane2DRenderer : RenderMesh<VertexPositionTexCoord>
    {
        public Plane2DRenderer(float size, bool flipY = false) : base(GetVertices(size, flipY), PrimitiveType.TriangleStrip)
        {

        }

        static VertexPositionTexCoord[] GetVertices(float size, bool flipY = false)
        {
            VertexPositionTexCoord[] vertices = new VertexPositionTexCoord[4];
            for (int i = 0; i < 4; i++)
            {
                vertices[i] = new VertexPositionTexCoord()
                {
                    Position = new Vector3(positions[i].X * size, positions[i].Y * size, 0),
                    TexCoord = flipY ? new Vector2(texCoords[i].X, 1.0f - texCoords[i].Y) : texCoords[i],
                };
            }
            return vertices;
        }

        static Vector2[] positions = new Vector2[4]
       {
                    new Vector2(-1.0f, 1.0f),
                    new Vector2(-1.0f, -1.0f),
                    new Vector2(1.0f, 1.0f),
                    new Vector2(1.0f, -1.0f),
       };

        static Vector2[] texCoords = new Vector2[4]
        {
                    new Vector2(0.0f, 0.0f),
                    new Vector2(0.0f, 1.0f),
                    new Vector2(1.0f, 0.0f),
                    new Vector2(1.0f, 1.0f),
        };
    }
}
