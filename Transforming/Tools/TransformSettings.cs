using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace GLFrameworkEngine
{
    public class TransformSettings
    {
        /// <summary>
        /// The initial gizmo size before being scaled by the camera distance.
        /// </summary>
        public float GizmoSize = 0.05f;

        /// <summary>
        /// The scale of the gizmo adjusted on the distance of the camera and gizmo itself.
        /// Updates during the render loop for the gizmo rendering.
        /// </summary>
        public float GizmoScale = 1.0f;

        /// <summary>
        /// The angle of the current rotation gizmo.
        /// Used for the UI to draw an angle difference in the arc angle.
        /// </summary>
        public float RotationAngle { get; set; }

        /// <summary>
        /// The origin position the gizmo is placed at.
        /// </summary>
        public Vector3 Origin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector3 RotationStartVector { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector3 RotationStartAxis { get; set; }

        /// <summary>
        /// Gets or sets the active axis.
        /// </summary>
        public TransformEngine.Axis ActiveAxis { get; set; }

        public bool IsMultiAxis()
        {
            //Check if the active axis isn't multi axis to display a rotation change
            int counter = 0;
            if (ActiveAxis.HasFlag(TransformEngine.Axis.X)) counter++;
            if (ActiveAxis.HasFlag(TransformEngine.Axis.Y)) counter++;
            if (ActiveAxis.HasFlag(TransformEngine.Axis.Z)) counter++;

            return counter > 1;
        }

        public float TextInput = 0.0f;
        public bool HasTextInput = false;

        public bool DisplayTranslationOriginLines = true;
        public bool DisplayGizmo = true;

        public bool InDirectTransformMode = false;

        public bool CollisionDetect = true;

        public bool RotateFromNormal = false;

        private Quaternion _rotation = Quaternion.Identity;
        public Quaternion Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                RotationMatrix = Matrix4.CreateFromQuaternion(_rotation);
            }
        }

        public Matrix4 RotationMatrix;

        public Vector3 TranslateSnapFactor { get; set; } = new Vector3(0.25f);
        public Vector3 ScaleSnapFactor { get; set; } = new Vector3(0.25f);
        public Vector3 RotateSnapFactor { get; set; } = new Vector3(45);

        public bool MiddleMouseScale = false;

        private bool _snapMode;
        public bool SnapTransform
        {
            get { return _snapMode || KeyEventInfo.State.KeyCtrl; }
            set
            {
                _snapMode = value;
            }
        }

        public bool SnapState;

        public TransformSpace TransformMode { get; set; } = TransformSpace.Local;
        public PivotSpace PivotMode = PivotSpace.Selected;

        public bool IsLocal => TransformMode == TransformSpace.Local;
        public bool IsWorld => TransformMode == TransformSpace.World;

        public enum PivotSpace
        {
            Selected,
            Individual,
        }

        public enum TransformSpace
        {
            World,
            Local,
        }
    }
}
