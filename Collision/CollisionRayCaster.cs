using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class CollisionRayCaster
    {
        public bool IgnoreWalls = true;
        public bool DebugDraw = false;

        List<Triangle> Triangles = new List<Triangle>();

        Vector3 bBoxMin = new Vector3(float.MaxValue);
        Vector3 bBoxMax = new Vector3(float.MinValue);

        public class Triangle
        {
            public Vector3 v1;
            public Vector3 v2;
            public Vector3 v3;
            public Vector3 normal;
            public Vector3 bBoxMin;
            public Vector3 bBoxMax;
        }

        #region DebugRender

        private RenderMesh<VertexPositionNormal> MeshRender;

        public void Render(GLContext context)
        {
            if (!DebugDraw) return;

            if (MeshRender == null)
                MeshRender = new RenderMesh<VertexPositionNormal>(GetVertices(), GetIndices(), PrimitiveType.Triangles);

            var mat = new StandardMaterial();
            mat.HalfLambertShading = true;
            mat.Render(context);

            GL.Enable(EnableCap.DepthTest);

            MeshRender.Draw(context);
        }

        int[] GetIndices()
        {
            int ind = 0;

            int[] indices = new int[Triangles.Count];
            for (int i = 0; i < Triangles.Count; i++)
            {
                int index = i * 3;
                indices[index] = ind++;
                indices[index + 1] = ind++;
                indices[index + 2] = ind++;
            }
            return indices;
        }

        VertexPositionNormal[] GetVertices()
        {
            VertexPositionNormal[] vertices = new VertexPositionNormal[Triangles.Count * 3];
            for (int i = 0; i < Triangles.Count; i++)
            {
                int index = i * 3;
                vertices[index] = new VertexPositionNormal(Triangles[i].v1, Triangles[i].normal);
                vertices[index + 1] = new VertexPositionNormal(Triangles[i].v2, Triangles[i].normal);
                vertices[index + 2] = new VertexPositionNormal(Triangles[i].v3, Triangles[i].normal);
            }
            return vertices;
        }

        #endregion

        SubDivCache Cache;

        public void Clear()
        {
            bBoxMin = new Vector3(float.MaxValue);
            bBoxMax = new Vector3(float.MinValue);
            Triangles.Clear();
            Cache = null;
        }

        public void AddTri(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Triangle tri = new Triangle();
            tri.v1 = v1;
            tri.v2 = v2 - v1;
            tri.v3 = v3 - v1;
            tri.normal = Vector3.Cross(tri.v2, tri.v3).Normalized();

            tri.bBoxMin = Vector3.MagnitudeMin(v1, Vector3.MagnitudeMin(v2, v3));
            tri.bBoxMax = Vector3.MagnitudeMax(v1, Vector3.MagnitudeMax(v2, v3));

            this.bBoxMin = Vector3.MagnitudeMin(tri.bBoxMin, bBoxMin);
            this.bBoxMax = Vector3.MagnitudeMax(tri.bBoxMax, bBoxMax);
            Triangles.Add(tri);
        }

        public void UpdateCache()
        {
            Cache = BuildSubdivCache();
        }

        SubDivCache BuildSubdivCache(int xSubDivs = 10, int ySubDivs = 10, int zSubDivs = 10)
        {
            SubDivCache cache = new SubDivCache();
            cache.xSubDivs = xSubDivs;
            cache.ySubDivs = ySubDivs;
            cache.zSubDivs = zSubDivs;

            if (this.Triangles.Count == 0)
                return cache;

            Stack<Triangle[]> cells = new Stack<Triangle[]>();
            for (int zSubdiv = 0; zSubdiv < zSubDivs; zSubdiv++)
            {
                var zMin = this.bBoxMin.Z + (this.bBoxMax.Z - this.bBoxMax.Z) / zSubDivs * zSubdiv;
                var zMax = this.bBoxMin.Z + (this.bBoxMax.Z - this.bBoxMax.Z) / zSubDivs * (zSubdiv + 1);

                List<Triangle> zCells = new List<Triangle>();
                for (int ySubdiv = 0; ySubdiv < ySubDivs; ySubdiv++)
                {
                    var yMin = this.bBoxMin.Y + (this.bBoxMax.Y - this.bBoxMax.Y) / ySubDivs * ySubdiv;
                    var yMax = this.bBoxMin.Y + (this.bBoxMax.Y - this.bBoxMax.Y) / ySubDivs * (ySubdiv + 1);

                    List<Triangle> yCells = new List<Triangle>();
                    for (int xSubdiv = 0; xSubdiv < xSubDivs; xSubdiv++)
                    {
                        var xMin = this.bBoxMin.X + (this.bBoxMax.X - this.bBoxMax.X) / xSubDivs * xSubdiv;
                        var xMax = this.bBoxMin.X + (this.bBoxMax.X - this.bBoxMax.X) / xSubDivs * (xSubdiv + 1);

                        List<Triangle> xCells = new List<Triangle>();
                        foreach (var tri in this.Triangles)
                        {
                            if (tri.bBoxMin.X - 1 > xMax || tri.bBoxMax.X + 1 < xMin)
                                continue;


                            if (tri.bBoxMin.Y - 1 > yMax || tri.bBoxMax.Y + 1 < yMin)
                                continue;


                            if (tri.bBoxMin.Z - 1 > zMax || tri.bBoxMax.Z + 1 < zMin)
                                continue;

                            xCells.Add(tri);
                        }

                        cells.Push(cache.xCells.ToArray());
                    }
                    cells.Push(cache.yCells.ToArray());
                }
                cells.Push(cache.zCells.ToArray());
            }

            cache.cells = cells.ToArray();

            return cache;
        }

        public HitItem RayCast(Vector3 origin, Vector3 dir, float margin = 0.000001f)
        {
            if (Triangles.Count == 0)
                return null;

            if (this.Cache == null)
                UpdateCache();

            var cache = this.Cache;
            var current = origin;
            var step = dir.Normalized() * (float)(Min(
            (this.bBoxMax.X - this.bBoxMin.X) / cache.xSubDivs,
            (this.bBoxMax.Y - this.bBoxMin.Y) / cache.ySubDivs,
            (this.bBoxMax.Z - this.bBoxMin.Z) / cache.zSubDivs) * 0.9);

            int lastZCell = -1;
            int lastYCell = -1;
            int lastXCell = -1;
            for (int i = 0; i < 500; i++)
            {
                if ((current.X > this.bBoxMax.X && dir.X > 0) ||
                (current.X < this.bBoxMin.X && dir.X < 0) ||
                (current.Y > this.bBoxMax.Y && dir.Y > 0) ||
                (current.Y < this.bBoxMin.Y && dir.Y < 0) ||
                (current.Z > this.bBoxMax.Z && dir.Z > 0) ||
                (current.Z < this.bBoxMin.Z && dir.Z < 0))
                    break;

                int xCell = (int)Math.Floor((current.X - this.bBoxMin.X) / (this.bBoxMax.X - this.bBoxMin.X) * cache.xSubDivs);
                int yCell = (int)Math.Floor((current.Y - this.bBoxMin.Y) / (this.bBoxMax.Y - this.bBoxMin.Y) * cache.ySubDivs);
                int zCell = (int)Math.Floor((current.Z - this.bBoxMin.Z) / (this.bBoxMax.Z - this.bBoxMin.Z) * cache.zSubDivs);

                if (xCell >= 0 && xCell < cache.xSubDivs) {
                    if (yCell >= 0 && yCell < cache.ySubDivs) {
                        if (zCell >= 0 && zCell < cache.zSubDivs)
                            if (xCell != lastXCell || yCell != lastYCell || zCell != lastZCell)
                            {
                                int index = zCell + yCell + xCell;
                                var hit = this.RayCastTris(origin, dir, cache.cells[index].ToList(), margin);
                                if (hit != null)
                                {
                                    return hit;
                                }
                            }
                    }
                }
                lastXCell = xCell;
                lastYCell = yCell;
                lastZCell = zCell;

                current = current + step;
            }

            return this.RayCastTris(origin, dir, this.Triangles, margin);
        }

        //https://github.com/magcius/noclip.website/blob/55c1ee05048bb431641a29122f0f40fc3c0e3fe8/src/SuperMarioGalaxy/Collision.ts#L911
        private bool isWallPolygonAngle(float v) {
            // 70 degrees -- Math.cos(70*Math.PI/180)
            return Math.Abs(v) < 0.3420201433256688;
        }

        public HitItem RayCastTris(Vector3 origin, Vector3 dir, List<Triangle> tris, float margin = 0.000001f)
        {
            HitItem nearestHit = null;

            foreach (var tri in tris)
            {
                if (IgnoreWalls && isWallPolygonAngle(Vector3.Dot(tri.normal, new Vector3(0, 1, 0))))
                    continue;

                Vector3 crossP = Vector3.Cross(dir, tri.v2);
                float det = Vector3.Dot(crossP, tri.v3);

                if (det < margin)
                    continue;

                Vector3 v1toOrigin = origin - tri.v1;
                float u = Vector3.Dot(v1toOrigin, crossP);

                if (u < 0 || u > det)
                    continue;

                Vector3 crossQ = Vector3.Cross(v1toOrigin, tri.v3);
                float v = Vector3.Dot(dir, crossQ);

                if (v < 0 || u + v > det)
                    continue;

                var distScaled = Math.Abs(Vector3.Dot(tri.v2, crossQ) / det);
                Vector3 pos = origin + (dir * distScaled);
                if (float.IsNaN(pos.X) || float.IsNaN(pos.Y) || float.IsNaN(pos.Z))
                    continue;

                if (nearestHit == null || distScaled < nearestHit.distScaled)
                {
                    nearestHit = new HitItem()
                    {
                        distScaled = distScaled,
                        position = pos,
                        u = u,
                        v = v,
                        tri = tri,
                    };
                }
            }

            return nearestHit;
        }

        float Min(float x, float y, float z)
        {
            return Math.Min(x, Math.Min(y, z));
        }

        public class HitItem
        {
            public float distScaled;
            public Vector3 position;
            public float u;
            public float v;
            public Triangle tri;
        }

        class SubDivCache
        {
            public List<Triangle> xCells = new List<Triangle>();
            public List<Triangle> yCells = new List<Triangle>();
            public List<Triangle> zCells = new List<Triangle>();

            public Triangle[][] cells;

            public int xSubDivs;
            public int ySubDivs;
            public int zSubDivs;
        }
    }
}
