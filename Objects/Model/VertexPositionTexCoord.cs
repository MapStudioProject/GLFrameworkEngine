using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public struct VertexPositionTexCoord
    {
        [RenderAttribute(0, VertexAttribPointerType.Float, 0)]
        public Vector3 Position;

        [RenderAttribute(GLConstants.VTexCoord0, VertexAttribPointerType.Float, 12)]
        public Vector2 TexCoord;

        public VertexPositionTexCoord(Vector3 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
        }
    }
}
