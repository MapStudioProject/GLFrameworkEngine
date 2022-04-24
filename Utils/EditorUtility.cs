using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using GLFrameworkEngine;

namespace GLFrameworkEngine
{
    public class EditorUtility
    {
        /// <summary>
        /// Gets the current position for spawning objects.
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetObjectPlacementPosition()
        {
            var context = GLContext.ActiveContext;

            //If the object is being added without the mouse on screen, add to the center of the screen
             if (!context.Focused)
             {
                 int centerX = context.Width / 2;
                 int centerY = context.Width / 2;
                 return context.ScreenToWorld(centerX, centerY, 100);
             }

            //By default show the object next to the camera
            var position = context.GetPointUnderMouse(100);

            //Hit the nearest collision if possible
            if (context.EnableDropToCollision && context.CollisionCaster != null)
            {
                var toScreen = context.CurrentMousePoint;
                var ray = context.PointScreenRay((int)toScreen.X, (int)toScreen.Y);
                var hit = context.CollisionCaster.RayCast(ray.Origin.Xyz, ray.Direction);
                if (hit != null) {
                    position = hit.position;
                }
            }

            return position;
        }

        /// <summary>
        /// Sets the current transform for spawning objects.
        /// </summary>
        /// <returns></returns>
        public static void SetObjectPlacementPosition(GLTransform transform, bool byCursor = false)
        {
            var context = GLContext.ActiveContext;

            if (byCursor)
            {
                transform.Position = context.Cursor3DPosition;
                transform.UpdateMatrix(true);
                return;
            }

            //If the object is being added without the mouse on screen, add to the center of the screen
            if (!context.Focused)
            {
                int centerX = context.Width / 2;
                int centerY = context.Height / 2;
                transform.Position = context.ScreenToWorld(centerX, centerY);
                transform.UpdateMatrix(true);
                return;
            }

            //By default show the object next to the camera
            var position = context.GetPointUnderMouse(100);
            var rotation = transform.Rotation;

            //Hit the nearest collision if possible
            if (context.EnableDropToCollision && context.CollisionCaster != null)
            {
                var toScreen = context.CurrentMousePoint;
                var ray = context.PointScreenRay((int)toScreen.X, (int)toScreen.Y);
                var hit = context.CollisionCaster.RayCast(ray.Origin.Xyz, ray.Direction);
                if (hit != null) {
                    position = hit.position;

                    //Collision rotation handling. Todo this doesn't quite work in all angles atm.
                    if (context.TransformTools.TransformSettings.RotateFromNormal)
                    {
                        var up = new Vector3(0, -1, 0);
                        var normal = hit.tri.normal;
                        var axis = Vector3.Normalize(Vector3.Cross(up, normal));
                        if (!float.IsNaN(axis.X))
                        {
                            float angle = MathF.Acos(Vector3.Dot(up, normal));
                            rotation = Quaternion.FromAxisAngle(axis, angle);
                        }
                    }
                }
            }
            transform.Position = position;
            transform.Rotation = rotation;
            transform.UpdateMatrix(true);
        }
    }
}
