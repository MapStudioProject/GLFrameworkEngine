using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace GLFrameworkEngine
{
    public class DrawingHelper
    {
        public static VertexPositionNormalTexCoord[] GetPlaneVertices(float size)
        {
            VertexPositionNormalTexCoord[] vertices = new VertexPositionNormalTexCoord[4];
            vertices[0] = new VertexPositionNormalTexCoord(new Vector3(-1, -1, 0), new Vector3(-1, -1, 0), new Vector2(0, 0)); //Bottom Left
            vertices[1] = new VertexPositionNormalTexCoord(new Vector3(1, -1, 0), new Vector3(1, -1, 0), new Vector2(1, 0)); //Bottom Right
            vertices[2] = new VertexPositionNormalTexCoord(new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector2(1, 1)); //Top Right
            vertices[3] = new VertexPositionNormalTexCoord(new Vector3(-1, 1, 0), new Vector3(-1, 1, 0), new Vector2(0, 1)); //Top Left
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Position *= size;
            return vertices;
        }

        public static Tuple<List<VertexPositionNormalTexCoord>, int[]> GetUVCubeVertices(float size)
        {
            return FromObj(new System.IO.MemoryStream(Properties.Resources.Cube), size);
        }

        public static VertexPositionNormal[] GetCubeVertices(float size)
        {
            VertexPositionNormal[] vertices = new VertexPositionNormal[8];
            vertices[0] = new VertexPositionNormal(new Vector3(-1, -1, 1), new Vector3(-1, -1, 1)); //Bottom Left
            vertices[1] = new VertexPositionNormal(new Vector3(1, -1, 1), new Vector3(1, -1, 1)); //Bottom Right
            vertices[2] = new VertexPositionNormal(new Vector3(1, 1, 1), new Vector3(1, 1, 1)); //Top Right
            vertices[3] = new VertexPositionNormal(new Vector3(-1, 1, 1), new Vector3(-1, 1, 1)); //Top Left
            vertices[4] = new VertexPositionNormal(new Vector3(-1, -1, -1), new Vector3(-1, -1, -1)); //Bottom Left -Z
            vertices[5] = new VertexPositionNormal(new Vector3(1, -1, -1), new Vector3(1, -1, -1)); //Bottom Right -Z
            vertices[6] = new VertexPositionNormal(new Vector3(1, 1, -1), new Vector3(1, 1, -1)); //Top Right -Z
            vertices[7] = new VertexPositionNormal(new Vector3(-1, 1, -1), new Vector3(-1, 1, -1)); //Top Left -Z
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Position *= size;

            return vertices;
        }

        public static VertexPositionNormalTexCoord[] GetUVSphereVertices(float radius, float u_segments, float v_segments)
        {
            List<VertexPositionNormalTexCoord> vertices = new List<VertexPositionNormalTexCoord>();

            float halfPi = (float)(Math.PI * 0.5);
            float oneThroughPrecision = 1.0f / u_segments;
            float twoPiThroughPrecision = (float)(Math.PI * 2.0 * oneThroughPrecision);

            float theta1, theta2, theta3;
            Vector3 norm = new Vector3(), pos = new Vector3();

            float latitudeSpacing = 1.0f / (u_segments + 1.0f);
            float longitudeSpacing = 1.0f / (v_segments);

            for (uint j = 0; j < u_segments / 2; j++)
            {
                theta1 = (j * twoPiThroughPrecision) - halfPi;
                theta2 = ((j + 1) * twoPiThroughPrecision) - halfPi;

                for (uint i = 0; i <= v_segments; i++)
                {
                    theta3 = i * twoPiThroughPrecision;

                    Vector2 texCoord = new Vector2(
                    i * longitudeSpacing,
                    1.0f - (j + 1) * latitudeSpacing);

                    norm.X = (float)(Math.Cos(theta1) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta1);
                    norm.Z = (float)(Math.Cos(theta1) * Math.Sin(theta3));
                    pos.X = radius * norm.X;
                    pos.Y = radius * norm.Y;
                    pos.Z = radius * norm.Z;

                    vertices.Add(new VertexPositionNormalTexCoord() { Position = pos, Normal = norm, TexCoord = texCoord });

                    norm.X = (float)(Math.Cos(theta2) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta2);
                    norm.Z = (float)(Math.Cos(theta2) * Math.Sin(theta3));
                    pos.X = radius * norm.X;
                    pos.Y = radius * norm.Y;
                    pos.Z = radius * norm.Z;

                    vertices.Add(new VertexPositionNormalTexCoord() { Position = pos, Normal = norm, TexCoord = texCoord });
                }
            }

            var normals = CalculateNormals(vertices.Select(x => x.Position).ToList());
            for (int i = 0; i < vertices.Count; i++)
                vertices[i] = new VertexPositionNormalTexCoord()
                {
                    Position = vertices[i].Position,
                    Normal = normals[i],
                    TexCoord = vertices[i].TexCoord,
                };

            return vertices.ToArray();
        }

        public static VertexPositionNormal[] GetSphereVertices(float radius, float u_segments, float v_segments)
        {
            List<VertexPositionNormal> vertices = new List<VertexPositionNormal>();

            float halfPi = (float)(Math.PI * 0.5);
            float oneThroughPrecision = 1.0f / u_segments;
            float twoPiThroughPrecision = (float)(Math.PI * 2.0 * oneThroughPrecision);

            float theta1, theta2, theta3;
            Vector3 norm = new Vector3(), pos = new Vector3();

            for (uint j = 0; j < u_segments / 2; j++)
            {
                theta1 = (j * twoPiThroughPrecision) - halfPi;
                theta2 = ((j + 1) * twoPiThroughPrecision) - halfPi;

                for (uint i = 0; i <= v_segments; i++)
                {
                    theta3 = i * twoPiThroughPrecision;

                    norm.X = (float)(Math.Cos(theta1) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta1);
                    norm.Z = (float)(Math.Cos(theta1) * Math.Sin(theta3));
                    pos.X = radius * norm.X;
                    pos.Y = radius * norm.Y;
                    pos.Z = radius * norm.Z;

                    vertices.Add(new VertexPositionNormal() { Position = pos, Normal = norm });

                    norm.X = (float)(Math.Cos(theta2) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta2);
                    norm.Z = (float)(Math.Cos(theta2) * Math.Sin(theta3));
                    pos.X = radius * norm.X;
                    pos.Y = radius * norm.Y;
                    pos.Z = radius * norm.Z;

                    vertices.Add(new VertexPositionNormal() { Position = pos, Normal = norm });
                }
            }

            return vertices.ToArray();
        }

        public static VertexPositionNormal[] GetCylinderVertices(float radius, float startHeight, float height, float slices)
        {
            List<VertexPositionNormal> vertices = new List<VertexPositionNormal>();

            List<Vector3> discPointsBottom = new List<Vector3>();
            List<Vector3> discPointsTop = new List<Vector3>();

            float sliceArc = 360.0f / (float)slices;
            float angle = 0;
            for (int i = 0; i < slices; i++)
            {
                float x = radius * (float)Math.Cos(MathHelper.DegreesToRadians(angle));
                float z = radius * (float)Math.Sin(MathHelper.DegreesToRadians(angle));
                discPointsBottom.Add(new Vector3(x, startHeight, z));

                x = radius * (float)Math.Cos(MathHelper.DegreesToRadians(angle));
                z = radius * (float)Math.Sin(MathHelper.DegreesToRadians(angle));

                discPointsTop.Add(new Vector3(x, height, z));
                angle += sliceArc;
            }

            for (int i = 0; i < slices; i++)
            {
                Vector3 p2 = discPointsBottom[i];
                Vector3 p1 = new Vector3(discPointsBottom[(i + 1) % discPointsBottom.Count]);

                vertices.Add(new VertexPositionNormal() { Position = new Vector3(0, startHeight, 0) });
                vertices.Add(new VertexPositionNormal() { Position = new Vector3(p2.X, startHeight, p2.Z) });
                vertices.Add(new VertexPositionNormal() { Position = new Vector3(p1.X, startHeight, p1.Z) });


                p2 = discPointsTop[i % discPointsTop.Count];
                p1 = discPointsTop[(i + 1) % discPointsTop.Count];

                vertices.Add(new VertexPositionNormal() { Position = new Vector3(0, height, 0) });
                vertices.Add(new VertexPositionNormal() { Position = new Vector3(p1.X, height, p1.Z) });
                vertices.Add(new VertexPositionNormal() { Position = new Vector3(p2.X, height, p2.Z) });
            }

            for (int i = 0; i < slices; i++)
            {
                Vector3 p1 = discPointsBottom[i];
                Vector3 p2 = discPointsBottom[((i + 1) % discPointsBottom.Count)];
                Vector3 p3 = discPointsTop[i];
                Vector3 p4 = discPointsTop[(i + 1) % discPointsTop.Count];

                vertices.Add(new VertexPositionNormal() { Position = p1 });
                vertices.Add(new VertexPositionNormal() { Position = p3 });
                vertices.Add(new VertexPositionNormal() { Position = p4 });

                vertices.Add(new VertexPositionNormal() { Position = p1 });
                vertices.Add(new VertexPositionNormal() { Position = p4 });
                vertices.Add(new VertexPositionNormal() { Position = p2 });
            }

            var normals = CalculateNormals(vertices.Select(x => x.Position).ToList());
            for (int i = 0; i < normals.Length; i++)
                vertices[i] = new VertexPositionNormal()
                {
                    Position = vertices[i].Position,
                    Normal = normals[i],
                };

                return vertices.ToArray();
        }

        public static VertexPositionNormal[] GetConeVertices(float radiusBottom, float radiusTop, float height, float slices, float startHeight = 0)
        {
            List<VertexPositionNormal> vertices = new List<VertexPositionNormal>();

            List<Vector3> discPointsBottom = new List<Vector3>();
            List<Vector3> discPointsTop = new List<Vector3>();

            float sliceArc = 360.0f / (float)slices;
            float angle = 0;
            for (int i = 0; i < slices; i++)
            {
                float x = radiusBottom * (float)Math.Cos(MathHelper.DegreesToRadians(angle));
                float z = radiusBottom * (float)Math.Sin(MathHelper.DegreesToRadians(angle));
                discPointsBottom.Add(new Vector3(x, startHeight, z));

                x = radiusTop * (float)Math.Cos(MathHelper.DegreesToRadians(angle));
                z = radiusTop * (float)Math.Sin(MathHelper.DegreesToRadians(angle));

                discPointsTop.Add(new Vector3(x, height, z));
                angle += sliceArc;
            }

            for (int i = 0; i < slices; i++)
            {
                Vector3 p2 = discPointsBottom[i];
                Vector3 p1 = new Vector3(discPointsBottom[(i + 1) % discPointsBottom.Count]);

                vertices.Add(new VertexPositionNormal() { Position = new Vector3(0, startHeight, 0) });
                vertices.Add(new VertexPositionNormal() { Position = new Vector3(p2.X, startHeight, p2.Z) });
                vertices.Add(new VertexPositionNormal() { Position = new Vector3(p1.X, startHeight, p1.Z) });

                p2 = discPointsTop[i % discPointsTop.Count];
                p1 = discPointsTop[(i + 1) % discPointsTop.Count];

                vertices.Add(new VertexPositionNormal() { Position = new Vector3(0, height, 0) });
                vertices.Add(new VertexPositionNormal() { Position = new Vector3(p1.X, height, p1.Z) });
                vertices.Add(new VertexPositionNormal() { Position = new Vector3(p2.X, height, p2.Z) });
            }

            for (int i = 0; i < slices; i++)
            {
                Vector3 p1 = discPointsBottom[i];
                Vector3 p2 = discPointsBottom[((i + 1) % discPointsBottom.Count)];
                Vector3 p3 = discPointsTop[i];
                Vector3 p4 = discPointsTop[(i + 1) % discPointsTop.Count];

                vertices.Add(new VertexPositionNormal() { Position = p1 });
                vertices.Add(new VertexPositionNormal() { Position = p3 });
                vertices.Add(new VertexPositionNormal() { Position = p4 });

                vertices.Add(new VertexPositionNormal() { Position = p1 });
                vertices.Add(new VertexPositionNormal() { Position = p4 });
                vertices.Add(new VertexPositionNormal() { Position = p2 });
            }

            var normals = CalculateNormals(vertices.Select(x => x.Position).ToList());
            for (int i = 0; i < vertices.Count; i++)
                vertices[i] = new VertexPositionNormal()
                {
                    Position = vertices[i].Position,
                    Normal = normals[i],
                };

            return vertices.ToArray();
        }

        public static Tuple<List<VertexPositionNormalTexCoord>, int[]> FromObj(byte[] data, float size = 1.0f)
        {
            return FromObj(new System.IO.MemoryStream(data), size);
        }

        public static Tuple<List<VertexPositionNormalTexCoord>, int[]> FromObj(System.IO.Stream stream, float size = 1.0f)
        {
            var obj = new ObjLoader();
            obj.LoadObj(stream);

            List<int> indices = new List<int>();
            List<VertexPositionNormalTexCoord> vertices = new List<VertexPositionNormalTexCoord>();
            foreach (var mesh in obj.Meshes) {
                foreach (var poly in mesh.Polygons.Values) {
                    indices.AddRange(poly.Indices.ToArray());

                    foreach (var vertex in poly.Vertices) {
                        vertices.Add(new VertexPositionNormalTexCoord()
                        {
                            Position = vertex.Position * size,
                            Normal = vertex.Normal,
                            TexCoord = vertex.TexCoord,
                        });
                    }
                }
            }
            return Tuple.Create(vertices, indices.ToArray());
        }

        public static Vector3[] CalculateNormals(List<Vector3> positions)
        {
            Vector3[] normals = new Vector3[positions.Count];
            for (int i = 0; i < positions.Count; i += 3)
            {
                if (i + 3 > positions.Count)
                    break;

                var v1 = positions[i + 0];
                var v2 = positions[i + 1];
                var v3 = positions[i + 2];

                var v1to2 = v2 - v1;
                var v1to3 = v3 - v1;

                var normal = Vector3.Cross(v1to2, v1to3).Normalized();

                normals[i + 0] = normal;
                normals[i + 1] = normal;
                normals[i + 2] = normal;
            }
            return normals;
        }

        public static Vector3[] CalculateNormals(List<Vector3> positions, List<int> indices)
        {
            Vector3[] normals = new Vector3[positions.Count];
            for (int i = 0; i < indices.Count; i += 3)
            {
                if (i + 3 >= indices.Count)
                    break;

                var v1 = positions[indices[i + 0]];
                var v2 = positions[indices[i + 1]];
                var v3 = positions[indices[i + 2]];

                var v1to2 = v2 - v1;
                var v1to3 = v3 - v1;

                var normal = Vector3.Cross(v1to2, v1to3).Normalized();

                normals[indices[i + 0]] = normal;
                normals[indices[i + 1]] = normal;
                normals[indices[i + 2]] = normal;
            }
            return normals;
        }
    }
}
