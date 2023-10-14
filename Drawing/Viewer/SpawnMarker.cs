using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents a marker for representing a spot an object will be placed at.
    /// </summary>
    public class SpawnMarker : IDrawable
    {
        /// <summary>
        /// The transform matrix of the drawable.
        /// </summary>
        public GLTransform Transform = new GLTransform();

        /// <summary>
        /// The material of the marker.
        /// </summary>
        private readonly StandardMaterial Material = new StandardMaterial();

        private readonly RenderMesh<VertexPositionColor> Renderer;

        /// <summary>
        /// Toggles visibility of the marker.
        /// </summary>
        public bool IsVisible { get; set; } = false;

        AxisLines AxisLines { get; set; }

        public SpawnMarker() 
        {
            Renderer = new RenderMesh<VertexPositionColor>(Vertices, PrimitiveType.Lines);
            AxisLines = new AxisLines(25);
            Material.hasVertexColors = true;
        }

        /// <summary>
        /// Sets the cursor 3d position given the screen coordinates.
        /// </summary>
        public void SetCursor(GLContext context, bool useMouseDepth = false) {
            EditorUtility.SetObjectPlacementPosition(context, this.Transform, useMouseDepth);
        }

        public void DrawModel(GLContext context, Pass pass)
        {
            if (!IsVisible || pass != Pass.OPAQUE)
                return;

            float scale = context.Camera.Is2D ? 100f : 1f;

            Material.ModelMatrix = Matrix4.CreateScale(scale) * Transform.TransformMatrix;
            Material.Color = new Vector4(1, 1, 1, 1);
            Material.Render(context);

            GL.LineWidth(1);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GLMaterialBlendState.TranslucentAlphaOne.RenderBlendState();
            Renderer.Draw(context);
            GLMaterialBlendState.Opaque.RenderBlendState();

            AxisLines.Transform = this.Transform;
            AxisLines.DrawModel(context, pass);

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            GL.LineWidth(1);
        }

        public void Dispose() => Renderer?.Dispose();

        static int CellAmount = 10;
        static float CellSize = 2;

        static VertexPositionColor[] Vertices => FillVertices(CellAmount, CellSize).ToArray();

        static List<VertexPositionColor> FillVertices(int amount, float size)
        {
            var vertices = new List<VertexPositionColor>();
            for (var i = -amount; i <= amount; i++)
            {
                Vector4 color = new Vector4(1, 1, 1, 1f);
                vertices.Add(new VertexPositionColor(new Vector3(-amount * size, 0f, i * size), color));
                vertices.Add(new VertexPositionColor(new Vector3(amount * size, 0f, i * size), color));
                vertices.Add(new VertexPositionColor(new Vector3(i * size, 0f, -amount * size), color));
                vertices.Add(new VertexPositionColor(new Vector3(i * size, 0f, amount * size), color));
            }
            return vertices;
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
}
