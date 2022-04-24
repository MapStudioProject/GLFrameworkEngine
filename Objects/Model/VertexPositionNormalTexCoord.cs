using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public struct VertexPositionNormalTexCoord
    {
        [RenderAttribute(0, VertexAttribPointerType.Float, 0)]
        public Vector3 Position;

        [RenderAttribute(GLConstants.VNormal, VertexAttribPointerType.Float, 12)]
        public Vector3 Normal;

        [RenderAttribute(GLConstants.VTexCoord0, VertexAttribPointerType.Float, 24)]
        public Vector2 TexCoord;

        public VertexPositionNormalTexCoord(Vector3 position, Vector3 normal, Vector2 texCoord)
        {
            Position = position;
            Normal = normal;
            TexCoord = texCoord;
        }
    }
}
