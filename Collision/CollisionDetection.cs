using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace GLFrameworkEngine
{
    public class CollisionDetection
    {
        public static void SetObjectToCollision(GLContext context,
       CollisionRayCaster collision, ref Vector3 position, ref Quaternion rotation, bool ignoreWalls)
        {
            if (collision == null) return;

            var toScreen = context.WorldToScreen(position);
            var ray = context.PointScreenRay((int)toScreen.X, (int)toScreen.Y);

            collision.IgnoreWalls = ignoreWalls;

            var hit = collision.RayCast(ray.Origin.Xyz, ray.Direction);
            if (hit != null)
            {
                position = hit.position;
                rotation = RotateFromNormal(hit.tri.normal, new Vector3(0, -1, 0));
            }
            else
                return;
        }

        public static void SetObjectToCollision(GLContext context,
CollisionRayCaster collision, Vector2 toScreen, ref Vector3 position, ref Quaternion rotation)
        {
            if (collision == null) return;

            var ray = context.PointScreenRay((int)toScreen.X, (int)toScreen.Y);

            var hit = collision.RayCast(ray.Origin.Xyz, ray.Direction);
            if (hit != null)
            {
                position = hit.position;
                rotation *= RotateFromNormal(hit.tri.normal, new Vector3(0, -1, 0));
            }
            else
                return;
        }

        static Quaternion RotateFromNormal(Vector3 normal, Vector3 up)
        {
            if (normal == up)
                return Quaternion.Identity;

            var axis = Vector3.Normalize(Vector3.Cross(up, normal));
            float angle = MathF.Acos(Vector3.Dot(up, normal));
            if (!float.IsNaN(axis.X) && !float.IsNaN(axis.Y) && !float.IsNaN(axis.Z))
              return Quaternion.FromAxisAngle(axis, angle);

            return Quaternion.Identity;
        }
    }
}
