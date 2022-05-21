using System;
using System.Collections.Generic;
using OpenTK;
using Toolbox.Core;

namespace GLFrameworkEngine
{
    public class RotateAction : ITransformAction
    {
        /// <summary>
        /// Toggles using gizmo.
        /// </summary>
        public bool UseGizmo = true;

        /// <summary>
        /// Determines if the translation action is in an active state or not.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the rotation from being dragged.
        /// </summary>
        public Quaternion DeltaRotation = Quaternion.Identity;

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

        public RotateAction(TransformSettings settings) {
            Settings = settings;
        }

        private Vector2 startMousePos = new Vector2();

        public override string ToString()
        {
            var euler = Toolbox.Core.STMath.ToEulerAngles(DeltaRotation) * Toolbox.Core.STMath.Rad2Deg;

            return $"(X: {euler.X} Y: {euler.Y} Z: {euler.Z})";
        }

        static readonly double eighthPI = Math.PI / 8d;

        Vector3 hitPt;

        double offset = 0;
        Quaternion previousRotation;

        public int ResetTransform(GLContext context, TransformSettings settings)
        {
            OriginStart = Settings.Origin;
            //Reset position
            startMousePos = new Vector2(context.CurrentMousePoint.X, context.CurrentMousePoint.Y);
            //Active axis vector
            var axis = GetAxis(settings.ActiveAxis);
            //Selected point ray when rotation gizmo is selected
            hitPt = CameraRay.GetPlaneIntersection(startMousePos, context.Camera, axis, Settings.Origin);
            //Direction from the current and gizmo ray
            Vector3 dir = (hitPt - Settings.Origin).Normalized();
            settings.RotationStartVector = dir;
            settings.RotationStartAxis = axis;
            //Current active angle (reset at 0)
            settings.RotationAngle = 0;
            previousRotation = settings.Rotation;
            DeltaRotation = Quaternion.Identity;

            return 0;
        }

        public int TransformChanged(GLContext context, float x, float y, TransformSettings settings)
        {
            Vector2 centerPoint = context.WorldToScreen(Settings.Origin);
            Vector2 mousePos = new Vector2(x, y);

            double angle = (Math.Atan2(mousePos.X - centerPoint.X, mousePos.Y - centerPoint.Y) -
                           Math.Atan2(startMousePos.X - centerPoint.X, startMousePos.Y - centerPoint.Y));

            Vector3 lastIntersection;
            Vector3 center = Settings.Origin;

            var vec = context.Camera.InverseRotationMatrix.Row2;
            if (settings.ActiveAxis != TransformEngine.Axis.All)
            {
                //The axis the angle is targeting
                Vector3 axis = GetAxis(settings.ActiveAxis);
                //The current ray position
                Vector3 pos = CameraRay.GetPlaneIntersection(mousePos, context.Camera, axis, Settings.Origin);
                if (pos != Vector3.Zero)
                {
                    //The current ray direction from the origin
                    Vector3 localPos = (pos - Settings.Origin).Normalized();
                    //The hit ray direction from the origin
                    Vector3 rotVecSrc = (hitPt - Settings.Origin).Normalized();
                    if (hitPt != Settings.Origin)
                    {
                        //Perpendicular angle to check the hit angle
                        Vector3 perpendicularVector = Vector3.Cross(rotVecSrc, axis).Normalized();

                        float acosAngle = Math.Clamp(Vector3.Dot(localPos, rotVecSrc), -1, 1);
                        angle = MathF.Acos(acosAngle);
                        angle *= (Vector3.Dot(localPos, perpendicularVector) < 0.0f) ? 1.0f : -1.0f;
                        if (angle != float.NaN)
                            settings.RotationAngle = (float)angle;
                    }
                }
            }

            if (settings.SnapTransform)
            {
                float snap = Settings.RotateSnapFactor.X * STMath.Deg2Rad;
                if (Settings.ActiveAxis == TransformEngine.Axis.X)
                    snap = Settings.RotateSnapFactor.X * STMath.Deg2Rad;
                if (Settings.ActiveAxis == TransformEngine.Axis.Y)
                    snap = Settings.RotateSnapFactor.Y * STMath.Deg2Rad;
                if (Settings.ActiveAxis == TransformEngine.Axis.Z)
                    snap = Settings.RotateSnapFactor.Z * STMath.Deg2Rad;

                angle = Math.Round(angle / snap) * snap;
            }

            if (angle != 0 && angle != float.NaN)
                DeltaRotation = GetRotation((float)angle, previousRotation);

            //Update the rotation of the gizmo real time for viewing

            //Local space order
            var rotation = previousRotation * DeltaRotation;
            //World space order
            if (settings.TransformMode == TransformSettings.TransformSpace.World)
                rotation = DeltaRotation * previousRotation;

            settings.Rotation = rotation;

            return 1;
        }

        public void ApplyTransform(List<GLTransform> previousTransforms, List<GLTransform> adjustedTransforms)
        {
            if (DeltaRotation == Quaternion.Identity)
                return;

            var transformTools = GLContext.ActiveContext.TransformTools;

            //Update all transforms
            for (int i = 0; i < adjustedTransforms.Count; i++) {
                var previous = previousTransforms[i];
                //Local space order
                var rotation = previous.Rotation * DeltaRotation;
                //World space order
                if (transformTools.TransformSettings.TransformMode == TransformSettings.TransformSpace.World)
                    rotation = DeltaRotation * previous.Rotation;
                //Text input to manually set specific values
                if (transformTools.TransformSettings.HasTextInput) {
                    float angle = transformTools.TransformSettings.TextInput;
                    rotation = GetRotation(MathHelper.DegreesToRadians(angle), previous.Rotation);
                }
                //Custom actions
                if (adjustedTransforms[i].CustomRotationActionCallback != null)
                {
                    adjustedTransforms[i].CustomRotationActionCallback.Invoke(
                       new GLTransform.CustomRotationArgs()
                       {
                           Origin = OriginStart,
                           PreviousTransform = previous,
                           Transform = adjustedTransforms[i],
                           DeltaRotation = DeltaRotation,
                           Rotation = rotation,
                       }, EventArgs.Empty);
                }
                else
                {
                    //Apply the rotation
                    adjustedTransforms[i].Rotation = rotation;

                    //Allow positions to be rotated from origin.
                    //Direct rotation
                    if (transformTools.TransformSettings.HasTextInput)
                    {
                        if (!adjustedTransforms[i].IndividualPivot && transformTools.TransformSettings.PivotMode == TransformSettings.PivotSpace.Selected)
                            adjustedTransforms[i].Position = Vector3.TransformPosition(previous.Position - OriginStart, Matrix4.CreateFromQuaternion(rotation)) + OriginStart;
                    }
                    else //Rotate by rotation difference
                    {
                        if (!adjustedTransforms[i].IndividualPivot && transformTools.TransformSettings.PivotMode == TransformSettings.PivotSpace.Selected)
                            adjustedTransforms[i].Position = Vector3.TransformPosition(previous.Position - OriginStart, Matrix4.CreateFromQuaternion(DeltaRotation)) + OriginStart;
                    }
                    adjustedTransforms[i].UpdateMatrix(true);
                }
            }
        }

        private Quaternion GetRotation(float angle, Quaternion previousRotation)
        {
            Vector3 vec = GLContext.ActiveContext.Camera.InverseRotationMatrix.Row2;
            switch (Settings.ActiveAxis)
            {
                case TransformEngine.Axis.X: vec = Vector3.UnitX; break;
                case TransformEngine.Axis.Y: vec = Vector3.UnitY; break;
                case TransformEngine.Axis.Z: vec = Vector3.UnitZ; break;
            }

            if (Settings.ActiveAxis == TransformEngine.Axis.All)
                vec = previousRotation.Inverted() * vec;

            return Quaternion.FromAxisAngle(vec, (float)angle);
        }

        private Vector3 GetAxis(TransformEngine.Axis axis)
        {
            //Unit or local axis vectors
            if (Settings.IsLocal)
            {
                var rot = Settings.RotationMatrix;
                switch (axis)
                {
                    case TransformEngine.Axis.X: return rot.Row0.Xyz;
                    case TransformEngine.Axis.Y: return rot.Row1.Xyz;
                    case TransformEngine.Axis.Z: return rot.Row2.Xyz;
                }
            }
            else
            {
                switch (axis)
                {
                    case TransformEngine.Axis.X: return Vector3.UnitX;
                    case TransformEngine.Axis.Y: return Vector3.UnitY;
                    case TransformEngine.Axis.Z: return Vector3.UnitZ;
                }
            }
            return GLContext.ActiveContext.Camera.InverseRotationMatrix.Row2;
        }

        public int FinishTransform()
        {
            return 0;
        }
    }
}
