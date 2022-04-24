using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace GLFrameworkEngine
{
    public class CameraRay
    {
        public Vector4 Origin { get; set; }
        public Vector4 Far { get; set; }

        public Vector3 Direction { get; set; }

        public float Time { get; set; }

        public bool IntersectsPlane(Vector3 normal, Vector3 point, out float intersectDist)
        {
            float denom = Vector3.Dot(Direction, normal);
            if (Math.Abs(denom) == 0)
            {
                intersectDist = 0f;
                return false;
            }

            intersectDist = Vector3.Dot(point - Origin.Xyz, normal) / denom;
            return intersectDist > 0f;
        }

        public static Vector3 GetPlaneIntersection(int x, int y, Camera camera, Vector3 normal, Vector3 point) {
            return GetPlaneIntersection(new Vector2(x, y), camera, normal, point);
        }

        public static Vector3 GetPlaneIntersection(Vector2 mousePos, Camera camera, Vector3 normal, Vector3 point)
        {
            CameraRay ray = CameraRay.PointScreenRay((int)mousePos.X, (int)mousePos.Y, camera);
            if (ray.IntersectsPlane(normal, point, out float intersectDist))
                return ray.Origin.Xyz + (ray.Direction * intersectDist);
            else
                return Vector3.Zero;
        }

        public void Transform(Quaternion quat)
        {
            Origin = new Vector4(Vector3.Transform(Origin.Xyz, quat), Origin.W);
            Far = new Vector4(Vector3.Transform(Far.Xyz, quat), Far.W);
            Direction = Vector3.Transform(Direction, quat);
        }

        public void Transform(Matrix4 transform)
        {
            Origin = new Vector4(Vector3.TransformPosition(Origin.Xyz, transform), Origin.W);
            Far = new Vector4(Vector3.TransformPosition(Far.Xyz, transform), Far.W);
            Direction = Vector3.TransformPosition(Direction, transform);
        }

        public static CameraRay PointScreenRay(int x, int y, Camera camera)
        {
            if (camera.ViewMatrix.Determinant == 0)
                return null;

            Vector3 mousePosA = new Vector3(x, y, -1f);
            Vector3 mousePosB = new Vector3(x, y, 1f);

            Vector4 nearUnproj = UnProject(camera.ProjectionMatrix, camera.ViewMatrix, mousePosA, camera.Width, camera.Height);
            Vector4 farUnproj = UnProject(camera.ProjectionMatrix, camera.ViewMatrix, mousePosB, camera.Width, camera.Height);

            Vector3 dir = farUnproj.Xyz - nearUnproj.Xyz;
            dir.Normalize();

            return new CameraRay() { Origin = nearUnproj, Far = farUnproj, Direction = dir };
        }

        public static Vector4 UnProject(Matrix4 projection, Matrix4 view, Vector3 mouse, int width, int height)
        {
            Vector4 vec = new Vector4();

            vec.X = 2.0f * mouse.X / width - 1;
            vec.Y = -(2.0f * mouse.Y / height - 1);
            vec.Z = mouse.Z;
            vec.W = 1.0f;

            Matrix4 viewInv = Matrix4.Invert(view);
            Matrix4 projInv = Matrix4.Invert(projection);

            Vector4.Transform(ref vec, ref projInv, out vec);
            Vector4.Transform(ref vec, ref viewInv, out vec);

            if (vec.W > float.Epsilon || vec.W < float.Epsilon)
            {
                vec.X /= vec.W;
                vec.Y /= vec.W;
                vec.Z /= vec.W;
            }
            return vec;
        }
    }
}
