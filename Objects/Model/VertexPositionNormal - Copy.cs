using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public struct VertexPositionNormalColor
    {
        [RenderAttribute(0, VertexAttribPointerType.Float, 0)]
        public Vector3 Position;

        [RenderAttribute(1, VertexAttribPointerType.Float, 12)]
        public Vector3 Normal;

        [RenderAttribute(GLConstants.VColor, VertexAttribPointerType.Float, 24)]
        public Vector3 Color;

        public VertexPositionNormalColor(Vector3 position, Vector3 normal, Vector3 color)
        {
            Position = position;
            Normal = normal;
            Color = color;
        }
    }
}
