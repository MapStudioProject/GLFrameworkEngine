using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents a 3D cursor renderer.
    /// This can be used to spawn objects at a specific point 
    /// or used as a pivot.
    /// </summary>
    public class Cursor3D : RenderMesh<Vector2>, IDrawable
    {
        /// <summary>
        /// The transform matrix of the drawable.
        /// </summary>
        public GLTransform Transform = new GLTransform();

        /// <summary>
        /// The material of the cursor.
        /// </summary>
        private readonly BillboardMaterial Material = new BillboardMaterial();

        /// <summary>
        /// Toggles visibility of the cursor.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        public Cursor3D() : base(GetVertices(60), PrimitiveType.LineLoop)
        {

        }

        /// <summary>
        /// Sets the cursor 3d position given the screen coordinates.
        /// </summary>
        public void SetCursor(GLContext context, int x, int y, bool useMouseDepth = false) {
            if (!IsVisible)
                return;

            Transform.Position = context.ScreenToWorld(x, y, 20, useMouseDepth);
            Transform.UpdateMatrix(true);
        }

        public void DrawModel(GLContext context, Pass pass)
        {
            if (pass != Pass.OPAQUE || !IsVisible)
                return;

            Material.ModelMatrix = Transform.TransformMatrix;
            Material.ScaleByCameraDistance = true;
            Material.Render(context);

            GL.Disable(EnableCap.DepthTest);
            GL.LineWidth(1.5f);

            Draw(context);

            GL.Enable(EnableCap.DepthTest);

            GL.LineWidth(1);
        }

        static Vector2[] GetVertices(int length)
        {
            Vector2[] vertices = new Vector2[length];
            for (int i = 0; i < length; i++)
            {
                double angle = 2 * Math.PI * i / length;
                vertices[i] = new Vector2(
                    MathF.Cos((float)angle),
                    MathF.Sin((float)angle));
            }
            return vertices;
        }
    }
}
