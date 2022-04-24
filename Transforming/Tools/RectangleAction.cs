using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;

namespace GLFrameworkEngine
{
    public class RectangleAction : ITransformAction
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
        /// Gets or sets the scale from being dragged.
        /// </summary>
        public Vector3 ScaleFactor = Vector3.One;

        /// <summary>
        /// Gets or sets the rotation for local axis transforming.
        /// </summary>
        public Quaternion Rotation { get; set; }

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

        Vector3[] RectangleFaceOrigins = new Vector3[6];

        private Vector2 startMousePos = new Vector2();

        public Vector2 GetStartMousePos() => startMousePos;

        public override string ToString()
        {
            return $"(X: {ScaleFactor.X} Y: {ScaleFactor.Y} Z: {ScaleFactor.Z})";
        }

        public RectangleAction(TransformSettings settings)
        {
            Settings = settings;
            Rotation = Quaternion.Identity;
        }

        public int ResetTransform(GLContext context, TransformSettings settings)
        {
             OriginStart = Settings.Origin;
            //Reset position
            startMousePos = new Vector2(context.CurrentMousePoint.X, context.CurrentMousePoint.Y);
            var box = GLContext.ActiveContext.TransformTools.BoundingBox;
            var mat = Matrix4.CreateFromQuaternion(Settings.Rotation) * Matrix4.CreateTranslation(Settings.Origin);

            for (int i = 0; i < RectangleFaceOrigins.Length; i++)
            {
                var point = box.GetFaceOrigin(i);
                RectangleFaceOrigins[i] = (Matrix4.CreateTranslation(point) * mat).ExtractTranslation();
            }

            return 0;
        }

        public int TransformChanged(GLContext context, float x, float y, TransformSettings settings)
        {
            var axisVec = GetSelectedAxisVector3(Settings.ActiveAxis);

            Vector2 centerPoint = context.WorldToScreen(GetOrigin(Settings.ActiveAxis));
            Vector2 mousePos = new Vector2(x, y);

            int x1 = (int)(mousePos.X - centerPoint.X);
            int y1 = (int)(mousePos.Y - centerPoint.Y);
            int x2 = (int)(startMousePos.X - centerPoint.X);
            int y2 = (int)(startMousePos.Y - centerPoint.Y);

            float scaling = (float)(Math.Sqrt(x1 * x1 + y1 * y1) / Math.Sqrt(x2 * x2 + y2 * y2));

            Vector3 scaleInput = new Vector3(scaling);
            if (settings.SnapTransform)
                scaleInput = scaleInput.Snap(settings.ScaleSnapFactor);
            if (settings.TextInput != 0.0f)
                scaleInput *= settings.TextInput;

            ScaleFactor = Vector3.One;
            if (Settings.ActiveAxis != TransformEngine.Axis.All)
            {
                if (Settings.ActiveAxis.HasFlag(TransformEngine.Axis.X)) this.ScaleFactor.X = scaleInput.X;
                if (Settings.ActiveAxis.HasFlag(TransformEngine.Axis.Y)) this.ScaleFactor.Y = scaleInput.Y;
                if (Settings.ActiveAxis.HasFlag(TransformEngine.Axis.Z)) this.ScaleFactor.Z = scaleInput.Z;
                if (Settings.ActiveAxis.HasFlag(TransformEngine.Axis.XN)) this.ScaleFactor.X = scaleInput.X;
                if (Settings.ActiveAxis.HasFlag(TransformEngine.Axis.YN)) this.ScaleFactor.Y = scaleInput.Y;
                if (Settings.ActiveAxis.HasFlag(TransformEngine.Axis.ZN)) this.ScaleFactor.Z = scaleInput.Z;
            }
            else
                this.ScaleFactor = scaleInput;

            return ScaleFactor != Vector3.One ? 1 : 0;
        }

        public void ApplyTransform(List<GLTransform> previousTransforms, List<GLTransform> adjustedTransforms)
        {
            //Update all transforms
            for (int i = 0; i < adjustedTransforms.Count; i++)
            {
                var transform = adjustedTransforms[i];
                var originalScale = previousTransforms[i].Scale;
                var originalPosition = previousTransforms[i].Position;

                var rotation = previousTransforms[i].Rotation;
                var origin = GetOrigin(Settings.ActiveAxis);
                if (Settings.IsLocal)
                {
                    var rot = Matrix4.CreateFromQuaternion(Settings.Rotation);
                    var delta = Vector3.TransformPosition(((originalPosition - origin) * ScaleFactor + origin) - originalPosition, rot);
                    transform.Position = originalPosition + delta;

                    transform.Scale = new Vector3(
                        originalScale.X * ScaleFactor.X,
                        originalScale.Y * ScaleFactor.Y,
                        originalScale.Z * ScaleFactor.Z
                        );
                }
                else
                {
                    var rot = Matrix3.CreateFromQuaternion(rotation);

                    transform.Position = (originalPosition - origin) * ScaleFactor + origin;
                    transform.Scale = new Vector3(
                        originalScale.X * new Vector3(rot.Row0 * ScaleFactor).Length,
                        originalScale.Y * new Vector3(rot.Row1 * ScaleFactor).Length,
                        originalScale.Z * new Vector3(rot.Row2 * ScaleFactor).Length
                        );
                }
                transform.UpdateMatrix(true);
            }
            GLContext.ActiveContext.TransformTools.UpdateOrigin();
            GLContext.ActiveContext.TransformTools.UpdateBoundingBox();
        }

        public Vector3 GetOrigin(TransformEngine.Axis axis)
        {
            //Select the opposite faces
            switch (axis)
            {
                case TransformEngine.Axis.XN: return RectangleFaceOrigins[0];
                case TransformEngine.Axis.YN: return RectangleFaceOrigins[1];
                case TransformEngine.Axis.ZN: return RectangleFaceOrigins[2];
                case TransformEngine.Axis.X: return RectangleFaceOrigins[3];
                case TransformEngine.Axis.Y: return RectangleFaceOrigins[4];
                case TransformEngine.Axis.Z: return RectangleFaceOrigins[5];
            }
            return Vector3.Zero;
        }


        private Vector3 GetSelectedAxisVector3(TransformEngine.Axis axis)
        {
            switch (axis)
            {
                case TransformEngine.Axis.X: return Vector3.UnitX;
                case TransformEngine.Axis.Y: return Vector3.UnitY;
                case TransformEngine.Axis.Z: return Vector3.UnitZ;
                case TransformEngine.Axis.XN: return Vector3.UnitX;
                case TransformEngine.Axis.YN: return Vector3.UnitY;
                case TransformEngine.Axis.ZN: return Vector3.UnitZ;
                default:
                    return Vector3.UnitZ;
            }
        }

        public int FinishTransform()
        {
            return 0;
        }

        public bool SetTransform(GLContext context, Vector2 point, TransformSettings settings)
        {

            return true;
        }
    }
}
