using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class Circle2DRenderer : RenderMesh<Vector3>
    {
        public Circle2DRenderer(int length, PrimitiveType type = PrimitiveType.LineLoop) : base(GetVertices(length), type)
        {

        }

        public void SetCustomSegment(Vector3 angleStartVec, Vector3 axis, float angleEnd, int length)
        {
            Vector3[] vertices = new Vector3[length+1];
            for (int i = 1; i < length; i++)
            {
                float angle =  angleEnd * ((float)(i - 1) / (length - 1));
                vertices[i] = Vector3.TransformPosition(angleStartVec, Matrix4.CreateFromAxisAngle(axis, angle));
            }
            UpdateVertexData(vertices);
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
