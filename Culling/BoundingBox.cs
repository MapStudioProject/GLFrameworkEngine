using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace GLFrameworkEngine
{
    public class BoundingBox
    {
        /// <summary>
        /// The minimum point in the box.
        /// </summary>
        public Vector3 Min { get; set; }

        /// <summary>
        /// The maximum point in the box.
        /// </summary>
        public Vector3 Max { get; set; }

        /// <summary>
        /// Gets the center of the bounding box.
        /// </summary>
        public Vector3 GetCenter()
        {
            return new Vector3((Min + Max) * 0.5f);
        }
            
        /// <summary>
        /// Gets the extent of the bounding box.
        /// </summary>
        public Vector3 GetExtent()
        {
            return GetSize() * 0.5f;
        }

        /// <summary>
        /// Gets the size of the bounding box.
        /// </summary>
        public Vector3 GetSize()
        {
            return Max - Min;
        }

        //Vertices of the box (local space)
        private Vector3[] Vertices = new Vector3[8];

        //Pre transformed vertices (world space)
        private Vector3[] TransformedVertices;

        public BoundingBox() {
        }

        public BoundingBox(BoundingBox box)
        {
            this.Include(box);
        }

        public void Include(BoundingBox box)
        {
            Vector3 min = Min;
            Vector3 max = Max;
            for (int i = 0; i < box.Vertices.Length; i++)
            {
                min.X = MathF.Min(min.X, box.Vertices[i].X);
                min.Y = MathF.Min(min.Y, box.Vertices[i].Y);
                min.Z = MathF.Min(min.Z, box.Vertices[i].Z);
                max.X = MathF.Max(max.X, box.Vertices[i].X);
                max.Y = MathF.Max(max.Y, box.Vertices[i].Y);
                max.Z = MathF.Max(max.Z, box.Vertices[i].Z);
            }
            UpdateVertices(min, max);
        }

        public BoundingBox(Vector3 min, Vector3 max) {
            UpdateVertices(min, max);
        }

        public void UpdateVertices(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;

            //Bottom left Z+
            Vertices[0] = new Vector3(min.X, min.Y, max.Z);
            //Bottom right Z+
            Vertices[1] = new Vector3(max.X, min.Y, max.Z);
            //Top left Z+
            Vertices[2] = new Vector3(min.X, max.Y, max.Z);
            //Top right Z+
            Vertices[3] = new Vector3(max.X, max.Y, max.Z);
            //Bottom left Z-
            Vertices[4] = new Vector3(min.X, min.Y, min.Z);
            //Bottom right Z-
            Vertices[5] = new Vector3(max.X, min.Y, min.Z);
            //Top left Z+
            Vertices[6] = new Vector3(min.X, max.Y, min.Z);
            //Top right Z+
            Vertices[7] = new Vector3(max.X, max.Y, min.Z);
        }

        public Vector3 GetFaceOrigin(int value)
        {
            var vertices = GetQuadFace(value);
            //Get center between vertices
            CalculateMinMax(vertices, out Vector3 min, out Vector3 max);
            return min + (max - min) * 0.5f;
        }

        public Vector3[] GetQuadFace(int value)
        {
            switch ((BoxFace)value)
            {
                case BoxFace.Top: //Top
                    return new Vector3[]
                    {
                        Vertices[2],Vertices[3],
                        Vertices[7],Vertices[6],
                    };
                case BoxFace.Bottom: //Bottom
                    return new Vector3[]
                    {
                        Vertices[0],Vertices[1],
                        Vertices[5],Vertices[4],
                    };
                case BoxFace.Right: //Right
                    return new Vector3[]
                    {
                        Vertices[1],Vertices[3],
                        Vertices[7],Vertices[5],
                    };
                case BoxFace.Left: //Left
                    return new Vector3[]
                    {
                        Vertices[0],Vertices[2],
                        Vertices[6],Vertices[4],
                    };
                case BoxFace.Front: //Front
                    return new Vector3[]
                    {
                        Vertices[0],Vertices[1],
                        Vertices[2],Vertices[3],
                    };
                case BoxFace.Back: //Back
                    return new Vector3[]
                    {
                        Vertices[4],Vertices[5],
                        Vertices[6],Vertices[7],
                    };
                default:
                    throw new Exception("Invalid face index.");
            }
        }

        enum BoxFace
        {
            Right,
            Top,
            Front,
            Left,
            Bottom,
            Back,
        }

        public void Set(Vector4[] vertices)
        {
            Vector3 max = new Vector3(float.MinValue);
            Vector3 min = new Vector3(float.MaxValue);
            for (int i = 0; i < vertices.Length; i++)
            {
                max.X = MathF.Max(max.X, vertices[i].X);
                max.Y = MathF.Max(max.Y, vertices[i].Y);
                max.Z = MathF.Max(max.Z, vertices[i].Z);
                min.X = MathF.Min(min.X, vertices[i].X);
                min.Y = MathF.Min(min.Y, vertices[i].Y);
                min.Z = MathF.Min(min.Z, vertices[i].Z);
            }
            Min = min;
            Max = max;
            UpdateVertices(min, max);
        }

        /// <summary>
        /// Checks if a point is inside this bounding box.
        /// </summary>
        public bool IsInside(Vector3 position)
        {
            return (position.X >= Min.X && position.X <= Max.X) &&
                   (position.Y >= Min.Y && position.Y <= Max.Y) &&
                   (position.Z >= Min.Z && position.Z <= Max.Z);
        }

        /// <summary>
        /// Checks if a bounding box is fully inside this bounding box.
        /// </summary>
        public bool IsInside(BoundingBox bounding)
        {
            if (IsInside(bounding.Min) && IsInside(bounding.Max))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if a bounding box is partially inside this bounding box.
        /// </summary>
        public bool IsOverlapping(BoundingBox bounding)
        {
            if (Min.X <= bounding.Max.X && Max.X >= bounding.Min.X &&
                Min.Y <= bounding.Max.Y && Max.Y >= bounding.Min.Y &&
                Min.Z <= bounding.Max.Z && Max.Z >= bounding.Min.Z)
                return false;

            return true;
        }

        public Vector3[] GetLocalSpaceVertices()
        {
            //Return transformed vertices if used
            return Vertices;
        }

        public Vector3[] GetVertices() {
            //Return transformed vertices if used
            return TransformedVertices != null ? TransformedVertices : Vertices;
        }

        public void ApplyTransform(Matrix4 transform)
        {
            for (int i = 0; i < 8; i++)
                Vertices[i] = (Matrix4.CreateTranslation(Vertices[i]) * transform).ExtractTranslation();
            CalculateMinMax(Vertices, out Vector3 min, out Vector3 max);
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Updates the current box points from the given transform.
        /// </summary>
        public void UpdateTransform(Matrix4 transform)
        {
           TransformedVertices = new Vector3[8];
            for (int i = 0; i < 8; i++)
                TransformedVertices[i] = Vector3.TransformPosition(Vertices[i], transform);
            CalculateMinMax(TransformedVertices, out Vector3 min, out Vector3 max);
            Min = min;
            Max = max;
        }

        public void DrawSolid(GLContext context, Matrix4 transform, Vector4 color)
        {
            UpdateTransform(transform);

            var solid = new StandardMaterial();
            solid.ModelMatrix = Matrix4.Identity;
            solid.Color = color;
            solid.Render(context);
            BoundingBoxRender.Draw(context, Min, Max);
        }

        public void Draw(GLContext context) {
            BoundingBoxRender.Draw(context, Min, Max);
        }

        /// <summary>
        /// Creates a bounding box from a min and max vertex point.
        /// </summary>
        public static BoundingBox FromMinMax(Vector3 min, Vector3 max) {
            return new BoundingBox(min, max);
        }

        public static BoundingBox FromVertices(Vector3[] vertices)
        {
            CalculateMinMax(vertices, out Vector3 min, out Vector3 max);
            return BoundingBox.FromMinMax(min, max);
        }

        /// <summary>
        /// Gets the min and max vector values from an array of points for creating a bounding box.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public static void CalculateMinMax(Vector3[] points, out Vector3 min, out Vector3 max)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;
            for (int i = 0; i < points.Length; i++)
            {
                maxX = Math.Max(points[i].X, maxX);
                maxY = Math.Max(points[i].Y, maxY);
                maxZ = Math.Max(points[i].Z, maxZ);
                minX = Math.Min(points[i].X, minX);
                minY = Math.Min(points[i].Y, minY);
                minZ = Math.Min(points[i].Z, minZ);
            }
            min = new Vector3(minX, minY, minZ);
            max = new Vector3(maxX, maxY, maxZ);
        }
    }
}
