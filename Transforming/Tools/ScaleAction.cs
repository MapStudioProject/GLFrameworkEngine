using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;

namespace GLFrameworkEngine
{
    public class ScaleAction : ITransformAction
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
        public Quaternion Rotation { get; set; } = Quaternion.Identity;

        /// <summary>
        /// The origin before transformation is applied.
        /// </summary>
        public Vector3 OriginStart { get; set; }

        /// <summary>
        /// The active axis.
        /// </summary>
        public TransformEngine.Axis Axis { get; set; }

        /// <summary>
        /// Gets or sets the transform settings.
        /// </summary>
        public TransformSettings Settings { get; set; }

        private Vector2 startMousePos = new Vector2();

        public Vector2 GetStartMousePos() => startMousePos;

        public override string ToString()
        {
            return $"(X: {ScaleFactor.X} Y: {ScaleFactor.Y} Z: {ScaleFactor.Z})";
        }

        public ScaleAction(TransformSettings settings)
        {
            Settings = settings;
        }

        public int ResetTransform(GLContext context, TransformSettings settings)
        {
            OriginStart = Settings.Origin;
            //Reset position
            startMousePos = new Vector2(context.CurrentMousePoint.X, context.CurrentMousePoint.Y);
            //Check if the object gizmo is selected
            return 0;
        }

        public int TransformChanged(GLContext context, float x, float y, TransformSettings settings)
        {
            Vector2 centerPoint = context.WorldToScreen(Settings.Origin);
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
            }
            else
                this.ScaleFactor = scaleInput;

            return ScaleFactor != Vector3.One ? 1 : 0;
        }

        public void ApplyTransform(List<GLTransform> previousTransforms, List<GLTransform> adjustedTransforms)
        {
            var settings = GLContext.ActiveContext.TransformTools.TransformSettings;

            //Update all transforms
            for (int i = 0; i < adjustedTransforms.Count; i++)
            {
                var transform = adjustedTransforms[i];
                var originalScale = previousTransforms[i].Scale;
                var originalPosition = previousTransforms[i].Position;

                var rotation = previousTransforms[i].Rotation;
                var rot = Matrix3.CreateFromQuaternion(rotation);

                if (transform.CustomScaleActionCallback != null)
                {
                    transform.CustomScaleActionCallback.Invoke(new GLTransform.CustomScaleArgs()
                    {
                        Origin = Settings.Origin,
                        Scale = ScaleFactor,
                        PreviousTransform = previousTransforms[i],
                        Rotation = rot,
                    }, EventArgs.Empty);
                }
                else
                {
                    if (settings.PivotMode != TransformSettings.PivotSpace.Individual && !transform.Config.ForceLocalScale)
                        transform.Position = (originalPosition - OriginStart) * ScaleFactor + OriginStart;

                    if (Settings.IsLocal)
                    {
                        transform.Scale = new Vector3(
                            originalScale.X * ScaleFactor.X,
                            originalScale.Y * ScaleFactor.Y,
                            originalScale.Z * ScaleFactor.Z
                            );
                    }
                    else
                    {
                        transform.Scale = new Vector3(
                            originalScale.X * new Vector3(rot.Row0 * ScaleFactor).Length,
                            originalScale.Y * new Vector3(rot.Row1 * ScaleFactor).Length,
                            originalScale.Z * new Vector3(rot.Row2 * ScaleFactor).Length
                            );
                    }
                    transform.UpdateMatrix(true);
                }
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
