using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace GLFrameworkEngine
{
    public class RayPicking
    {
        public bool HasIntersection(GLContext context, IRayCastPicking drawable, Vector2 position)
        {
            var camera = context.Camera;
            var ray = CameraRay.PointScreenRay((int)position.X, context.Height - (int)position.Y, camera);
            var bounding = drawable.GetRayBounding();
            if (bounding == null)
                return false;

            if (drawable is ITransformableObject) {
                var transform = ((ITransformableObject)drawable).Transform;
                if (bounding.TransformByScale)
                    bounding.UpdateTransform(transform.TransformMatrix);
                else
                    bounding.UpdateTransform(transform.TransformNoScaleMatrix);
            }

            float intersectionDistance = 0;
            return HasIntersection(ray, bounding, ref intersectionDistance);
        }

        public IRayCastPicking FindPickableAtPosition(GLContext context, List<IRayCastPicking> drawables, Vector2 position)
        {
            var camera = context.Camera;
            var ray = CameraRay.PointScreenRay((int)position.X, context.Height - (int)position.Y, camera);

            List<Result> rayHit = new List<Result>();
            foreach (var drawable in drawables)
            {
                var bounding = drawable.GetRayBounding();
                if (bounding == null)
                    continue;

                if (drawable is ITransformableObject) {
                    var transform = ((ITransformableObject)drawable).Transform;
                    if (bounding.TransformByScale)
                        bounding.UpdateTransform(transform.TransformMatrix);
                    else
                        bounding.UpdateTransform(transform.TransformNoScaleMatrix);
                }

                float intersectionDistance = 0;
                if (HasIntersection(ray, bounding, ref intersectionDistance))
                    rayHit.Add(new Result() { Pickable = drawable, Distance = intersectionDistance });
            }

            rayHit.Sort((x, y) => x.Distance.CompareTo(y.Distance));
            var output = rayHit.FirstOrDefault();
            if (output != null)
                return output.Pickable;

            return null;
        }

        class Result
        {
            public IRayCastPicking Pickable;
            public float Distance;
        }

        public bool HasIntersection(CameraRay ray, BoundingNode bounding, ref float intersectionDistance) {
            //Check sphere first
            if (HasSphereIntersection(ray, bounding, ref intersectionDistance))
                return true;

            return HasAABBIntersection(ray, bounding.Box, ref intersectionDistance);
        }

        bool HasSphereIntersection(CameraRay ray, BoundingNode bounding, ref float intersectionDistance)
        {
            Vector3 center = bounding.GetCenter();
            float radius = bounding.GetRadius();

            Vector3 oc = ray.Origin.Xyz - center;


            float a = Vector3.Dot(ray.Direction, ray.Direction);
            float b = 2.0f * Vector3.Dot(oc, ray.Direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            float discriminant = b * b - 4 * a * c;

            intersectionDistance = discriminant;

            return discriminant > 0;
        }

        bool HasAABBIntersection(CameraRay ray, BoundingBox bounding, ref float intersectionDistance)
        {
            Vector3 t_1 = new Vector3(), t_2 = new Vector3();

            float tNear = float.MinValue;
            float tFar = float.MaxValue;

            // Test infinite planes in each directin.
            for (int i = 0; i < 3; i++)
            {
                // Ray is parallel to planes in this direction.
                if (ray.Direction[i] == 0)
                {
                    if ((ray.Origin[i] < bounding.Min[i]) || (ray.Origin[i] > bounding.Max[i]))
                    {
                        // Parallel and outside of the box, thus no intersection is possible.
                        intersectionDistance = float.MinValue;
                        return false;
                    }
                }
                else
                {
                    t_1[i] = (bounding.Min[i] - ray.Origin[i]) / ray.Direction[i];
                    t_2[i] = (bounding.Max[i] - ray.Origin[i]) / ray.Direction[i];

                    // Ensure T_1 holds values for intersection with near plane.
                    if (t_1[i] > t_2[i])
                    {
                        Vector3 temp = t_2;
                        t_2 = t_1;
                        t_1 = temp;
                    }

                    if (t_1[i] > tNear)
                        tNear = t_1[i];

                    if (t_2[i] < tFar)
                        tFar = t_2[i];

                    if ((tNear > tFar) || (tFar < 0))
                    {
                        intersectionDistance = float.MinValue;
                        return false;
                    }
                }
            }

            intersectionDistance = tNear;
            return true;
        }
    }
}
