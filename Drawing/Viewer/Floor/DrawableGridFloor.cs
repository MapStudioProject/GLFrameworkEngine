using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class DrawableGridFloor : RenderMesh<Vector3>
    {
        public static System.Numerics.Vector4 GridColor = new System.Numerics.Vector4(0.3f, 0.3f, 0.3f, 1.0f);

        public static int CellAmount = 10;
        public static float CellSize = 1;

        public static bool Display = true;

        private static int _cellAmount;
        private static float _cellSize;

        private StandardMaterial Material = new StandardMaterial();

        public DrawableGridFloor() : base(Vertices, PrimitiveType.Lines)
        {

        }

        static Vector3[] Vertices => FillVertices(CellAmount, CellSize).ToArray();

        static List<Vector3> FillVertices(int amount, float size)
        {
            var vertices = new List<Vector3>();
            for (var i = -amount; i <= amount; i++)
            {
                vertices.Add(new Vector3(-amount * size, 0f, i * size));
                vertices.Add(new Vector3(amount * size, 0f, i * size));
                vertices.Add(new Vector3(i * size, 0f, -amount * size));
                vertices.Add(new Vector3(i * size, 0f, amount * size));
            }
            return vertices;
        }

        public void Draw(GLContext control, Pass pass)
        {
            if (pass != Pass.OPAQUE || !Display)
                return;

            if (_cellAmount != CellAmount || CellSize != _cellSize) {
                UpdateVertexData(Vertices);
                _cellAmount = CellAmount;
                _cellSize = CellSize;
            }

            Material.Color = new Vector4(GridColor.X, GridColor.Y, GridColor.Z, GridColor.W);
            Material.Render(control);

            this.Draw(control);

            GL.UseProgram(0);
        }
    }
}