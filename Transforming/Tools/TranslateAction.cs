using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;

namespace GLFrameworkEngine
{
    public class TranslateAction : ITransformAction
    {
        /// <summary>
        /// Gets or sets the translation offset from being dragged.
        /// </summary>
        public Vector3 TranslationOffset { get; set; }

        /// <summary>
        /// The origin before transformation is applied.
        /// </summary>
        public Vector3 OriginStart { get; set; }

        /// <summary>
        /// Gets or sets the transform settings.
        /// </summary>
        public TransformSettings Settings { get; set; }

        /// <summary>
        /// The active axis.
        /// </summary>
        public TransformEngine.Axis Axis { get; set; }

        //Previous stored position for dragging.
        private Vector3 previousPosition = Vector3.Zero;

        public override string ToString()
        {
            return $"(X: {TranslationOffset.X} Y: {TranslationOffset.Y} Z: {TranslationOffset.Z})";
        }

        public TranslateAction(TransformSettings settings)
        {
            Settings = settings;
        }

        public int ResetTransform(GLContext context, TransformSettings settings)
        {
            OriginStart = Settings.Origin;
            //Reset position
            previousPosition = Vector3.Zero;
            TranslationOffset = Vector3.Zero;
            return 0;
        }

        public int TransformChanged(GLContext context, float x, float y, TransformSettings settings)
        {
            Vector2 point = new Vector2(x, y);
            var ray = context.PointScreenRay((int)point.X, (int)point.Y);

            var localX = settings.IsLocal ? Vector3.Transform(Vector3.UnitX, settings.Rotation) : Vector3.UnitX;
            var localY = settings.IsLocal ? Vector3.Transform(Vector3.UnitY, settings.Rotation) : Vector3.UnitY;
            var localZ = settings.IsLocal ? Vector3.Transform(Vector3.UnitZ, settings.Rotation) : Vector3.UnitZ;

            var currentPosition = Settings.Origin;
            Vector3 distance = currentPosition - context.Camera.GetViewPostion();
            Vector3 axis_vector = GetSelectedAxisVector3(Settings.ActiveAxis);
            if (settings.IsLocal)
                axis_vector = Vector3.Transform(axis_vector, settings.Rotation);

            Vector3 plane_tangent = Vector3.Cross(axis_vector, distance);
            //Plane normal for X, Y, Z movement
            Vector3 plane_normal = Vector3.Cross(axis_vector, plane_tangent);
            //Plane normal for multi axis movement
            if (Settings.ActiveAxis.HasFlag(TransformEngine.Axis.XY))
                plane_normal = Vector3.Cross(localX, localY);
            if (Settings.ActiveAxis.HasFlag(TransformEngine.Axis.YZ))
                plane_normal = Vector3.Cross(localY, localZ);
            if (Settings.ActiveAxis.HasFlag(TransformEngine.Axis.XZ))
                plane_normal = Vector3.Cross(localX, localZ);
            //Plane normal for all axis movement
            if (Settings.ActiveAxis.HasFlag(TransformEngine.Axis.All))
                plane_normal = context.Camera.InverseRotationMatrix.Row2;

            if (ray.IntersectsPlane(plane_normal, currentPosition, out float intersectDist))
            {
                Vector3 hitPos = ray.Origin.Xyz + (ray.Direction * intersectDist);
                if (settings.SnapTransform)
                    hitPos = hitPos.Snap(settings.TranslateSnapFactor);

                //Apply axis constraints if necessary
                Vector3 newPosition = Vector3.Zero;
                if (!Settings.ActiveAxis.HasFlag(TransformEngine.Axis.All))
                {
                    //Multiple axis can be applied
                    if (Settings.ActiveAxis.HasFlag(TransformEngine.Axis.X))
                        newPosition += localX * Vector3.Dot(hitPos, localX);
                    if (Settings.ActiveAxis.HasFlag(TransformEngine.Axis.Y))
                        newPosition += localY * Vector3.Dot(hitPos, localY);
                    if (Settings.ActiveAxis.HasFlag(TransformEngine.Axis.Z))
                        newPosition += localZ * Vector3.Dot(hitPos, localZ);
                }
                else
                    newPosition = hitPos;

                //Set the previous position if not set yet to find the differences in movement
                if (previousPosition == Vector3.Zero)
                {
                    previousPosition = newPosition;
                    //No need to update the current action as previous is the same
                    return 0;
                }

                Vector3 localDelta = newPosition - previousPosition;
                TranslationOffset += localDelta;
                //Apply the new position for dragging again.
                previousPosition = newPosition;

                //Prevent updates if the offset is not changed
                if (TranslationOffset == Vector3.Zero)
                    return 0;
            }
            else
            {
                // Our raycast missed the plane
                return 0;
            }
            return 1;
        }

        public void ApplyTransform(GLContext context, List<GLTransform> previousTransforms, List<GLTransform> adjustedTransforms)
        {
            var settings = context.TransformTools.TransformSettings;
            var rayCaster = context.CollisionCaster;

            for (int i = 0; i < previousTransforms.Count; i++)
            {
                var targetTransform = adjustedTransforms[i];    
                var newPosition = previousTransforms[i].Position + TranslationOffset;
                var rotation = previousTransforms[i].Rotation;

                //Support automatic collision connection if enabled
                if (targetTransform.EnableCollisionDrop && settings.CollisionDetect && !KeyEventInfo.State.KeyShift)
                {
                    CollisionDetection.SetObjectToCollision(context, rayCaster, ref newPosition, ref rotation, !settings.RotateFromNormal);
                    if (settings.RotateFromNormal)
                        targetTransform.Rotation = rotation;
                }
                //Text input relative to new position
                var input = new Vector3(context.TransformTools.TransformSettings.TextInput);
                newPosition += GetAxisPosition(input);

                if (adjustedTransforms[i].CustomTranslationActionCallback != null)
                {
                    adjustedTransforms[i].CustomTranslationActionCallback.Invoke(
                       new GLTransform.CustomTranslationArgs()
                       {
                           Origin = Settings.Origin,
                           PreviousTransform = previousTransforms[i],
                           Transform = adjustedTransforms[i],
                           Translation = newPosition,
                           TranslationDelta = TranslationOffset,
                       }, EventArgs.Empty);
                }
                else
                {
                    targetTransform.Position = newPosition;
                    targetTransform.UpdateMatrix(true);
                }
            }
        }

        public int FinishTransform()
        {
            previousPosition = Vector3.Zero;
            return 0;
        }

        private Vector3 GetAxisPosition(Vector3 position)
        {
            if (Settings.ActiveAxis == TransformEngine.Axis.All)
                return position;

            Vector3 axis = GetSelectedAxisVector3(Settings.ActiveAxis);
            return axis * Vector3.Dot(position, axis);
        }

        private Vector3 GetSelectedAxisVector3(TransformEngine.Axis axis)
        {
            switch (axis)
            {
                case TransformEngine.Axis.X: return Vector3.UnitX;
                case TransformEngine.Axis.Y: return Vector3.UnitY;
                case TransformEngine.Axis.Z: return Vector3.UnitZ;
                default:
                    return Vector3.UnitZ;
            }
        }
    }
}
