using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents a lined box renderer from an axis aligned bounding box.
    /// </summary>
    public class BoundingBoxRender : RenderMesh<Vector3>
    {
        public GLTransform Transform = new GLTransform();

        public BoundingBoxRender(Vector3 min, Vector3 max) :
            base(GetVertices(min, max), Indices, PrimitiveType.Lines)
        {

        }

        public void Update(Vector3 min, Vector3 max) {
            this.UpdateVertexData(GetVertices(min, max));
        }

        static BoundingBoxRender BoudingRender;

        public static void Draw(GLContext context, BoundingBox box) {
            Draw(context, box.Min, box.Max);
        }

        public static void Draw(GLContext context, Vector3 min, Vector3 max)
        {
            if (BoudingRender == null)
                BoudingRender = new BoundingBoxRender(min, max);

            BoudingRender.Update(min, max);
            BoudingRender.Draw(context);
        }

        public static int[] Indices = new int[]
        {
            0, 1, 2, 3, //Bottom & Top
            4, 5, 6, 7, //Bottom & Top -Z
            0, 2, 1, 3, //Bottom to Top
            4, 6, 5, 7, //Bottom to Top -Z
            0, 4, 6, 2, //Bottom Z to -Z
            1, 5,  3, 7 //Top Z to -Z
        };

        static Vector3[] GetVertices(Vector3 Min, Vector3 Max)
        {
            Vector3[] corners = new Vector3[8];

            corners[0] = Min;
            corners[1] = new Vector3(Min.X, Min.Y, Max.Z);
            corners[2] = new Vector3(Min.X, Max.Y, Min.Z);
            corners[3] = new Vector3(Min.X, Max.Y, Max.Z);
            corners[4] = new Vector3(Max.X, Min.Y, Min.Z);
            corners[5] = new Vector3(Max.X, Min.Y, Max.Z);
            corners[6] = new Vector3(Max.X, Max.Y, Min.Z);
            corners[7] = Max;

            return corners;
        }
    }
}
