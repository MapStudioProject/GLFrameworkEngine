using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class AxisLines : IDrawable
    {
        /// <summary>
        /// The transform matrix of the drawable.
        /// </summary>
        public GLTransform Transform = new GLTransform();

        /// <summary>
        /// Toggles visibility of the axis.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// The line size of the axis.
        /// </summary>
        public float LineSize = 1.5f;

        /// <summary>
        /// The material of the axis.
        /// </summary>
        private readonly StandardMaterial Material = new StandardMaterial();

        private readonly RenderMesh<VertexPositionColor> Renderer;

        public AxisLines(float size) {
            Renderer = new RenderMesh<VertexPositionColor>(GetVertices(size), PrimitiveType.Lines);
            Material.hasVertexColors = true;
        }

        public void DrawModel(GLContext context, Pass pass)
        {
            if (pass != Pass.OPAQUE)
                return;

            Material.ModelMatrix = Transform.TransformMatrix;
            Material.Render(context);

            GL.LineWidth(LineSize);
            Renderer.Draw(context);
            GL.LineWidth(1);
        }

        public void Dispose() => Renderer.Dispose();

        static VertexPositionColor[] GetVertices(float scale)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[6];
            //X
            vertices[0] = new VertexPositionColor(new Vector3(scale, 0, 0), new Vector4(1, 0, 0, 1));
            vertices[1] = new VertexPositionColor(new Vector3(0, 0, 0), new Vector4(1, 0, 0, 1));
            //Y
            vertices[2] = new VertexPositionColor(new Vector3(0, scale, 0), new Vector4(0, 1, 0, 1));
            vertices[3] = new VertexPositionColor(new Vector3(0, 0, 0), new Vector4(0, 1, 0, 1));
            //Z
            vertices[4] = new VertexPositionColor(new Vector3(0, 0, scale), new Vector4(0, 0, 1, 1));
            vertices[5] = new VertexPositionColor(new Vector3(0, 0, 0), new Vector4(0, 0, 1, 1));
            return vertices;
        }
    }

    struct VertexPositionColor
    {
        [RenderAttribute(0, VertexAttribPointerType.Float, 0)]
        public Vector3 Position;

        [RenderAttribute(GLConstants.VColor, VertexAttribPointerType.Float, 12)]
        public Vector4 Color;

        public VertexPositionColor(Vector3 position, Vector4 color)
        {
            Position = position;
            Color = color;
        }
    }
}
