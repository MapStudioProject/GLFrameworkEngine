using System;
using System.ComponentModel;
using OpenTK;
using Toolbox.Core;

namespace GLFrameworkEngine
{
    public class GLTransform : INotifyPropertyChanged
    {
        /// <summary>
        /// Configuration for transforming this specific transform object,
        /// </summary>
        public GLTransformConfig Config = new GLTransformConfig();

        /// <summary>
        /// The model bounding for the transform object.
        /// Currently used to determine collision handling 
        /// for detecting collision drops in the transform.
        /// </summary>
        public BoundingBox ModelBounding = null;

        public bool IgnoreY = false;

        public bool EnableCollisionDrop = true;

        public bool IndividualPivot = false;

        public Vector3 Origin;

        private bool originOverride = false;

        public bool HasCustomOrigin => originOverride;

        public void SetCustomOrigin(Vector3 origin)
        {
            originOverride = true;
            Origin = origin;
        }

        public Vector3 _position = Vector3.Zero;
        public Vector3 _scale = Vector3.One;

        /// <summary>
        /// Gets or sets the position of the bone in world space.
        /// </summary>
        [BindGUI("TRANSLATE", Category = "TRANSFORM")]
        public Vector3 Position
        {
            get { return _position; }
            set
            {
                if (IgnoreY)
                    _position = new Vector3(value.X, _position.Y, value.Z);
                else
                    _position = value;

                //Origin shares the position unless overidden
                if (!originOverride)
                    Origin = value;
                NotifyPropertyChanged("Position");
            }
        }

        private Matrix3 RotationMatrix = Matrix3.Identity;

        /// <summary>
        /// Gets or sets the rotation of the bone in world space.
        /// </summary>
        public Quaternion Rotation
        {
            get { return quaternion; }
            set
            {
                quaternion = value;
                RotationMatrix = Matrix3.CreateFromQuaternion(value);
                rotationEuler = RotationMatrix.ExtractEulerAngles();
            }
        }

        private Quaternion quaternion = Quaternion.Identity;

        /// <summary>
        /// Gets or sets the <see cref="Rotation"/> using euler method in radians. 
        /// </summary>
        public Vector3 RotationEuler
        {
            get { return rotationEuler; }
            set
            {
                rotationEuler = value;
                RotationMatrix = Matrix3Extension.FromEulerAngles(value);
                quaternion = RotationMatrix.ExtractRotation();
            }
        }

        private Vector3 rotationEuler;

        /// <summary>
        /// Gets or sets the <see cref="Rotation"/> using euler method in degrees. 
        /// </summary>
        [BindGUI("ROTATE", Category = "TRANSFORM")]
        public Vector3 RotationEulerDegrees
        {
            get { return rotationEuler * STMath.Rad2Deg; }
            set
            {
                rotationEuler = value * STMath.Deg2Rad;
                RotationMatrix = Matrix3Extension.FromEulerAngles(rotationEuler);
                quaternion = RotationMatrix.ExtractRotation();
                NotifyPropertyChanged("RotationEulerDegrees");
            }
        }

        /// <summary>
        /// Gets or sets the scale of the bone in world space.
        /// </summary>
        [BindGUI("SCALE", Category = "TRANSFORM")]
        public Vector3 Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                NotifyPropertyChanged("Scale");
            }
        }

        /// <summary>
        /// Determines to update the transform or not.
        /// </summary>
        public bool UpdateTransform = true;

        private Matrix4 transformMatrix = Matrix4.Identity;

        /// <summary>
        /// Gets or sets the calculated transform matrix.
        /// </summary>
        public Matrix4 TransformMatrix
        {
            get { return transformMatrix; }
            set { transformMatrix = value; }
        }

        /// <summary>
        /// Gets or sets the calculated transform matrix without the scale value.
        /// </summary>
        public Matrix4 TransformNoScaleMatrix { get; set; } = Matrix4.Identity;

        /// <summary>
        /// A callback for before the transform has been updated
        /// </summary>
        public EventHandler BeforeTransformUpdate;

        /// <summary>
        /// A callback for after the transform has been updated
        /// </summary>
        public EventHandler TransformUpdated;

        /// <summary>
        /// A callback for when the transform has been applied using a transform tool.
        /// </summary>
        public EventHandler TransformActionApplied;

        public EventHandler TransformStarted;

        public EventHandler CustomTranslationActionCallback;
        public EventHandler CustomRotationActionCallback;
        public EventHandler CustomScaleActionCallback;

        public GLTransform Clone()
        {
            var transform = new GLTransform()
            {
                Config = new GLTransformConfig()
                {
                    ForceLocalScale = this.Config.ForceLocalScale,
                },
                Position = this.Position,
                Rotation = this.Rotation,
                Scale = this.Scale,
                TransformUpdated = this.TransformUpdated,
                TransformActionApplied = this.TransformActionApplied,
                TransformStarted = this.TransformStarted,
                CustomTranslationActionCallback = this.CustomTranslationActionCallback,
                CustomRotationActionCallback = this.CustomRotationActionCallback,
                CustomScaleActionCallback = this.CustomScaleActionCallback,
            };
            transform.UpdateMatrix(true);
            return transform;
        }

        /// <summary>
        /// Updates the TransformMatrix from the current position, scale and rotation values.
        /// </summary>
        public void UpdateMatrix(bool forceUpdate = false)
        {
            if (!UpdateTransform && !forceUpdate)
                return;

            BeforeTransformUpdate?.Invoke(this, EventArgs.Empty);

            var translationMatrix = Matrix4.CreateTranslation(Position);
            var rotationMatrix = new Matrix4(RotationMatrix);
            var scaleMatrix = Matrix4.CreateScale(Scale);
            TransformNoScaleMatrix = rotationMatrix * translationMatrix;
            transformMatrix = scaleMatrix * TransformNoScaleMatrix;

            TransformUpdated?.Invoke(this, EventArgs.Empty);
            UpdateTransform = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class CustomTranslationArgs
        {
            //Before transform is applied
            public GLTransform PreviousTransform;
            //Current transform applied in view
            public GLTransform Transform;
            //Translation output
            public Vector3 Translation;
            //Translation difference
            public Vector3 TranslationDelta;

            public Vector3 Origin;
        }

        public class CustomRotationArgs
        {
            //Before transform is applied
            public GLTransform PreviousTransform;
            //Current transform applied in view
            public GLTransform Transform;
            //Rotation output
            public Quaternion Rotation;
            //The difference in the rotation output
            public Quaternion DeltaRotation;

            public Vector3 Origin;
        }

        public class CustomScaleArgs
        {
            //Before transform is applied
            public GLTransform PreviousTransform;
            //Current transform applied in view
            public GLTransform Transform;
            //Rotation output
            public Vector3 Scale;

            public Matrix3 Rotation;

            public Vector3 Origin;
        }
    }
}
