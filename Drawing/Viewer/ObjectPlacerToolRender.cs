using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents a marker to represent where an object will be placed if dropped in a scene.
    /// </summary>
    public class ObjectPlacerToolRender : RenderMesh<Vector3>, IDrawable
    {
        /// <summary>
        /// The transform matrix of the drawable.
        /// </summary>
        public GLTransform Transform = new GLTransform();

        /// <summary>
        /// The material of the cursor.
        /// </summary>
        private readonly StandardMaterial Material = new StandardMaterial();

        /// <summary>
        /// Toggles visibility of the cursor.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        public ObjectPlacerToolRender() : base(GetVertices(60), PrimitiveType.LineLoop)
        {

        }

        public void DrawModel(GLContext context, Pass pass)
        {
            if (pass != Pass.OPAQUE)
                return;

            Material.ModelMatrix = Transform.TransformMatrix;
            Material.Render(context);

            GL.Disable(EnableCap.DepthTest);
            GL.LineWidth(1.5f);

            Draw(context);

            GL.Enable(EnableCap.DepthTest);

            GL.LineWidth(1);
        }

        static Vector3[] GetVertices(int length)
        {
            Vector3[] vertices = new Vector3[length];
            for (int i = 0; i < length; i++)
            {
                double angle = 2 * Math.PI * i / length;
                vertices[i] = new Vector3(
                    MathF.Cos((float)angle),
                    MathF.Sin((float)angle), 0);
            }
            return vertices;
        }
    }
}
