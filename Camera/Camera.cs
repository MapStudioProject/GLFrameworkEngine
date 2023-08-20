using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;
using Toolbox.Core;

namespace GLFrameworkEngine
{
    public class Camera
    {
        //Camera fustrum data.
        private CameraFrustum CameraFrustum = new CameraFrustum();

        public EventHandler OnBeforeUpdate;
        public EventHandler OnAfterUpdate;

        public bool UseSquareAspect = false;

        /// <summary>
        /// Keyed values for animating a camera.
        /// </summary>
        public Dictionary<CameraAnimationKeys, float> AnimationKeys = new Dictionary<CameraAnimationKeys, float>();

        /// <summary>
        /// The speed of the camera used when zooming.
        /// </summary>
        public float ZoomSpeed { get; set; } = 1.0f;

        /// <summary>
        /// The speed of the camera used when padding.
        /// </summary>
        public float PanSpeed { get; set; } = 1.0f;

        public float RotationSpeed { get; set; } = 1.0f;

        /// <summary>
        /// The move speed of the camera used when using key movements.
        /// </summary>
        public float KeyMoveSpeed { get; set; } = 10.0f;

        /// <summary>
        /// The width of the camera fustrum.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the camera fustrum.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The aspect ratio of the camera fustrum.
        /// </summary>
        public float AspectRatio => (float)Width / Height;

        /// <summary>
        /// The field of view in degrees.
        /// </summary>
        public float FovDegrees
        {
            get { return Fov * STMath.Rad2Deg; }
            set { Fov = value * STMath.Deg2Rad; }
        }

        private float _fov = 45 * STMath.Deg2Rad;

        /// <summary>
        /// The field of view in radians.
        /// </summary>
        public float Fov
        {
            get {
                if (AnimationKeys.ContainsKey(CameraAnimationKeys.FieldOfView))
                    return AnimationKeys[CameraAnimationKeys.FieldOfView];

                return _fov; 
            }
            set
            {
                _fov = Math.Max(value, 0.01f);
                _fov = Math.Min(_fov, 3.14f);
            }
        }

        private float _znear = 1.0f;

        /// <summary>
        /// The z near value.
        /// </summary>
        public float ZNear
        {
            get {
                if (AnimationKeys.ContainsKey(CameraAnimationKeys.Near))
                    return AnimationKeys[CameraAnimationKeys.Near];

                return _znear; }
            set
            {
                _znear = Math.Max(value, 0.001f);
                _znear = Math.Min(_znear, 100);
            }
        }

        private float _zfar = 100000.0f;

        /// <summary>
        /// The z far value.
        /// </summary>
        public float ZFar
        {
            get {
                if (AnimationKeys.ContainsKey(CameraAnimationKeys.Far))
                    return AnimationKeys[CameraAnimationKeys.Far];

                return _zfar; }
            set
            {
                _zfar = Math.Max(value, 10.0f);
            }
        }

        /// <summary>
        /// The rotation of the camera on the X axis in radians.
        /// </summary>
        public float RotationX = 0;

        /// <summary>
        /// The rotation of the camera on the Y axis in radians.
        /// </summary>
        public float RotationY = 0;

        /// <summary>
        /// The rotation of the camera on the Z axis in radians.
        /// </summary>
        public float RotationZ = 0;

        /// <summary>
        /// The rotation of the camera on the X axis in degrees.
        /// </summary>
        public float RotationDegreesX
        {
            get { return RotationX * STMath.Rad2Deg;  }
            set { RotationX = value * STMath.Deg2Rad; }
        }

        /// <summary>
        /// The rotation of the camera on the Y axis in degrees.
        /// </summary>
        public float RotationDegreesY
        {
            get { return RotationY * STMath.Rad2Deg; }
            set { RotationY = value * STMath.Deg2Rad; }
        }

        /// <summary>
        /// The rotation of the camera on the Z axis in degrees.
        /// </summary>
        public float RotationDegreesZ
        {
            get { return RotationZ * STMath.Rad2Deg; }
            set { RotationZ = value * STMath.Deg2Rad; }
        }

        /// <summary>
        /// Locks the camera state to prevent rotations.
        /// </summary>
        public bool LockRotation { get; set; } = false;

        internal Vector3 _targetPosition;

        /// <summary>
        /// The position of the camera in world space.
        /// </summary>
        public Vector3 TargetPosition
        {
            get { return _targetPosition; }
            set { _targetPosition = value; }
        }

        /// <summary>
        /// The controller of the camera to handle user movement.
        /// </summary>
        public ICameraController Controller { get; set; }

        /// <summary>
        /// Toggles orthographic projection in the camera.
        /// </summary>
        public bool IsOrthographic { get; set; }

        /// <summary>
        /// Determines if the camera is 2D or not.
        /// </summary>
        public bool Is2D => IsOrthographic && LockRotation;

        /// <summary>
        /// Gets the distance of the camera.
        /// </summary>
        public float Distance
        {
            get { return Math.Abs(_targetPosition.Z); }
            set
            {
                _targetPosition.Z = value;
            }
        }

        internal float _targetDistance;

        /// <summary>
        /// The distance to the camera target
        /// </summary>
        public float TargetDistance
        {
            get { return _targetDistance; }
            set { _targetDistance = value; }
        }

        protected Matrix4 projectionMatrix;
        protected Matrix4 viewMatrix;
        protected Matrix3 invRotationMatrix;

        /// <summary>
        /// Gets the model matrix of the camera.
        /// </summary>
        public Matrix4 ModelMatrix { get; set; } = Matrix4.Identity;

        /// <summary>
        /// Gets the combined view projection matrix of the camera.
        /// </summary>
        public Matrix4 ViewProjectionMatrix { get; private set; }

        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix4 ProjectionMatrix
        {
            get { return projectionMatrix; }
            set { projectionMatrix = value; }
        }

        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        public Matrix4 ViewMatrix
        {
            get { return viewMatrix; }
            set { viewMatrix = value; }
        }

        /// <summary>
        /// Gets or sets the inverse rotation matrix.
        /// </summary>
        public Matrix3 InverseRotationMatrix
        {
            get { return invRotationMatrix; }
            set { invRotationMatrix = value; }
        }

        /// <summary>
        /// Inverts the camera rotation controls on the X axis.
        /// </summary>
        public bool InvertRotationX { get; set; } = false;

        /// <summary>
        /// Inverts the camera rotation controls on the Y axis.
        /// </summary>
        public bool InvertRotationY { get; set; } = false;

        /// <summary>
        /// The horizontal aspect factor of the camera fustrum.
        /// </summary>
        public float FactorX => (2f * (float)Math.Tan(Fov * 0.5f) * AspectRatio) / Width;

        /// <summary>
        /// The verticle aspect factor of the camera fustrum.
        /// </summary>
        public float FactorY => (2f * (float)Math.Tan(Fov * 0.5f) * AspectRatio) / Height;

        /// <summary>
        /// The right vector of the camera view.
        /// </summary>
        public Vector3 RightVector
        {
            get
            {
                var invView = ViewMatrix.Inverted();
                return Vector3.TransformNormal(Vector3.UnitX, invView); 
            }
        }

        /// <summary>
        /// The up vector of the camera view.
        /// </summary>
        public Vector3 UpVector
        {
            get
            {
                var invView = ViewMatrix.Inverted();
                return Vector3.TransformNormal(Vector3.UnitY, invView);
            }
        }

        /// <summary>
        /// The forward vector of the camera view.
        /// </summary>
        public Vector3 ForwardVector
        {
            get
            {
                var invView = ViewMatrix.Inverted();
                return Vector3.TransformNormal(Vector3.UnitZ, invView);
            }
        }

        /// <summary>
        /// The camera distance when an object is being focused on.
        /// </summary>
        public float FocusDistance = 200.0f;

        /// <summary>
        /// The depth of the mouse cursor.
        /// </summary>
        public float Depth { get; set; }

        /// <summary>
        /// The animation timer used during camera transitional movements.
        /// </summary>
        public System.Timers.Timer AnimationTimer { get; set; }

        /// <summary>
        /// Resets the camera transform values.
        /// </summary>
        public void ResetTransform()
        {
            TargetPosition = new Vector3();
            RotationX = 0;
            RotationY = 0;
            TargetDistance = 0;
            UpdateMatrices();
        }

        /// <summary>
        /// Resets the viewport camera transform values.
        /// </summary>
        public void ResetViewportTransform()
        {
            RotationX = 0;
            RotationY = 0;
            if (Mode == CameraMode.Inspect)
            {
                TargetPosition = new OpenTK.Vector3(0, 1, 0);
                TargetDistance = 5;
            }
            else
            {
                TargetPosition = new OpenTK.Vector3(0, 1, 5);
                TargetDistance = 0;
            }
            UpdateMatrices();
        }

        /// <summary>
        /// Rotates the camera from a given eye and target position.
        /// </summary>
        public void RotateFromLookat(Vector3 eye, Vector3 target, Vector3 up)
        {
            Matrix4 mat = Matrix4.LookAt(eye, target, up);
            var rot = STMath.GetEulerAngle(mat);
            RotationX = rot.X;
            RotationY = rot.Y;
            RotationZ = rot.Z;
        }

        /// <summary>
        /// Rotates the camera from a given eye and target position.
        /// </summary>
        public void RotateFromLookat(Vector3 eye, Vector3 target)
        {
            Vector3 direction = (eye - target).Normalized();
            RotationX = MathF.Asin(direction.Y);
            RotationY = -MathF.Atan2(direction.X, direction.Z);
        }

        /// <summary>
        /// Calculates a scaling factor given the camera distance.
        /// This can be used for objects to maintain their scale while the camera moves.
        /// </summary>
        public float ScaleByCameraDistance(Vector3 position, float factor = 0.002f)
        {
            float distance = (GetViewPostion() - position).Length;
            return Math.Max(distance * factor, 0.01f);
        }

        public bool CanUpdate = true;

        /// <summary>
        /// Updates the view and projection matrices with current camera data.
        /// </summary>
        public void UpdateMatrices()
        {
            OnBeforeUpdate?.Invoke(this, EventArgs.Empty);

            if (CanUpdate)
                projectionMatrix = GetProjectionMatrix();

            viewMatrix = GetViewMatrix();

            ViewProjectionMatrix = viewMatrix * projectionMatrix;

            OnAfterUpdate?.Invoke(this, EventArgs.Empty);

            CameraFrustum.UpdateCamera(this);

            //Frustum checks
            foreach (var ob in GLContext.ActiveContext.Scene.Objects) {
                if (ob is IFrustumCulling && ((IFrustumCulling)ob).EnableFrustumCulling)
                   ((IFrustumCulling)ob).InFrustum = ((IFrustumCulling)ob).IsInsideFrustum(GLContext.ActiveContext);
            }
        }

        /// <summary>
        /// Transforms the camera position to focus on the given transformation.
        /// </summary>
        public void FocusOnObject(GLTransform transform) {
            var position = transform.Position;

            //Todo animated camera does not look very good 

           /* Dictionary<CameraAnimationKeys, float> anim = new Dictionary<CameraAnimationKeys, float>();
            anim.Add(CameraAnimationKeys.PositionX, position.X);
            anim.Add(CameraAnimationKeys.PositionY, position.Y);
            anim.Add(CameraAnimationKeys.PositionZ, position.Z);
            anim.Add(CameraAnimationKeys.Distance, distance);

            StartCameraAnimation(anim, 5);

            return;*/

            _targetPosition = position;
            if (cameraMode == CameraMode.Inspect)
                _targetDistance = FocusDistance;
            else
                TargetPosition += Vector3.Transform(InverseRotationMatrix, new Vector3(0, 0, FocusDistance));
        }

        /// <summary>
        /// Checks if a bounding node is within the camera fustrum.
        /// </summary>
        public CameraFrustum.Frustum GetFustrumState(BoundingNode boundingNode)
        {
            return CameraFrustum.CheckIntersectionState(this, boundingNode);
        }

        /// <summary>
        /// Checks if a bounding node is within the camera fustrum.
        /// </summary>
        public bool InFustrum(BoundingNode boundingNode) {
           return CameraFrustum.CheckIntersection(this, boundingNode);
        }

        /// <summary>
        /// Checks if the given position is in range with the camera position.
        /// </summary>
        public bool InRange(Vector3 pos, float rangeSquared) {
            return (pos - GetViewPostion()).LengthSquared < rangeSquared;
        }

        /// <summary>
        /// Gets the camera view position.
        /// </summary>
        public Vector3 GetViewPostion() {
            var pos = _targetPosition;
            var dist = _targetDistance;

            if (AnimationKeys.ContainsKey(CameraAnimationKeys.PositionX))
                pos.X = AnimationKeys[CameraAnimationKeys.PositionX];
            if (AnimationKeys.ContainsKey(CameraAnimationKeys.PositionY))
                pos.Y = AnimationKeys[CameraAnimationKeys.PositionY];
            if (AnimationKeys.ContainsKey(CameraAnimationKeys.PositionZ))
                pos.Z = AnimationKeys[CameraAnimationKeys.PositionZ];
            if (AnimationKeys.ContainsKey(CameraAnimationKeys.Distance))
                dist = AnimationKeys[CameraAnimationKeys.Distance];

            return pos + InverseRotationMatrix.Row2 * dist;
        }

        public Vector3 GetLookAtPostion(float dist = 1.0f)
        {
            //Get the eye direction and subtract from the camera position
            return GetViewPostion() - InverseRotationMatrix.Row2 * dist;
        }

        private CameraMode cameraMode = CameraMode.Inspect;

        /// <summary>
        /// Sets and updates the camera controller mode.
        /// </summary>
        public CameraMode Mode
        {
            get { return cameraMode; }
            set
            {
                if (cameraMode != value)
                {
                    cameraMode = value;
                    UpdateMode();
                }
            }
        }

        private FaceDirection faceDirection = FaceDirection.Any;

        /// <summary>
        /// Gets and sets the current camera direction being faced.
        /// </summary>
        public FaceDirection Direction
        {
            get { return faceDirection; }
            set
            {
                faceDirection = value;
                UpdateDirection();
            }
        }

        //Look at properties

        /// <summary>
        /// Sets the rotation type to a look at type. Generally used for animating.
        /// </summary>
        public bool RotationLookat { get; set; }

        /// <summary>
        /// Gets or sets the eye target for look at rotation type.
        /// </summary>
        public Vector3 EyeTarget { get; set; }
        /// <summary>
        /// Gets or sets the Z rotation for look at rotation type.
        /// </summary>
        public float Twist { get; set; }

        //Matrix handling

        /// <summary>
        /// Gets the calculated projection matrix.
        /// </summary>
        public Matrix4 GetProjectionMatrix()
        {
            if (ZNear > ZFar)
                ZFar = ZNear * 2;

            if (IsOrthographic)
            {
                //Make sure the scale isn't negative or it would invert the viewport
                // float scale = Math.Max((Distance + TargetDistance) / 1000.0f, 0.000001f);
                float distance = this.cameraMode == CameraMode.Inspect ? TargetDistance : Distance;
                float scale = Math.Max((distance) / 1000.0f, 0.000001f);

                return Matrix4.CreateOrthographicOffCenter(-(Width * scale), Width * scale, -(Height * scale), Height * scale, -100000, 100000);
            }
            else
                return Matrix4.CreatePerspectiveFieldOfView(Fov, AspectRatio, 
                    ZNear * GLContext.PreviewScale,
                    ZFar * GLContext.PreviewScale);
        }

        /// <summary>
        /// Gets the calculated view matrix.
        /// </summary>
        public Matrix4 GetViewMatrix()
        {
            var position = TargetPosition;
            var rotation = new Vector3(RotationX, RotationY, RotationZ);
            var twist = this.Twist;
            var target = this.EyeTarget;
            var distance = this.TargetDistance;

            //Update keyed values used for an animated camera.
            UpdateKeyedAnimationValues(ref position, ref rotation, ref target, ref twist, ref distance);

            //Update the inv rotation matrix with the current rotation values
            invRotationMatrix = Matrix3.CreateRotationX(-rotation.X) *
                                Matrix3.CreateRotationY(-rotation.Y) *
                                Matrix3.CreateRotationZ(-rotation.Z);

            //Fly camera will use distance from the position
            if (cameraMode == CameraMode.FlyAround) {
                position += Vector3.Transform(InverseRotationMatrix, new Vector3(0, 0, distance));
            }

            //Check for different rotation handling
            if (RotationLookat) {
                return Matrix4.LookAt(position, target,  new Vector3(0, 1, 0)) * Matrix4.CreateRotationZ(twist);
            }
            else
            {
                var translationMatrix = Matrix4.CreateTranslation(-position);
                var rotationMatrix = Matrix4.CreateRotationZ(rotation.Z) *
                                     Matrix4.CreateRotationY(rotation.Y) * 
                                     Matrix4.CreateRotationX(rotation.X);

                if (cameraMode == CameraMode.Inspect)
                    return translationMatrix * rotationMatrix * Matrix4.CreateTranslation(0, 0, -distance);
                else
                    return translationMatrix * rotationMatrix;
            }
        }

        //Animation handling

        /// <summary>
        /// Resets the keyed animation data for the camera.
        /// </summary>
        public void ResetAnimations()
        {
            RotationLookat = false;
            AnimationKeys.Clear();
            UpdateMatrices();
        }

        /// <summary>
        /// Sets a key value for animation usage.
        /// </summary>
        public void SetKeyframe(CameraAnimationKeys keyType, float value)
        {
            if (AnimationKeys.ContainsKey(keyType))
                AnimationKeys[keyType] = value;
            else
                AnimationKeys.Add(keyType, value);
        }

        private void UpdateKeyedAnimationValues(ref Vector3 position, ref Vector3 rotation, ref Vector3 target, ref float twist, ref float distance)
        {
            TryUpdateKeyedValue(CameraAnimationKeys.PositionX, ref position.X);
            TryUpdateKeyedValue(CameraAnimationKeys.PositionY, ref position.Y);
            TryUpdateKeyedValue(CameraAnimationKeys.PositionZ, ref position.Z);
            TryUpdateKeyedValue(CameraAnimationKeys.RotationX, ref rotation.X);
            TryUpdateKeyedValue(CameraAnimationKeys.RotationY, ref rotation.Y);
            TryUpdateKeyedValue(CameraAnimationKeys.RotationZ, ref rotation.Z);
            TryUpdateKeyedValue(CameraAnimationKeys.TargetX, ref target.X);
            TryUpdateKeyedValue(CameraAnimationKeys.TargetY, ref target.Y);
            TryUpdateKeyedValue(CameraAnimationKeys.TargetZ, ref target.Z);
            TryUpdateKeyedValue(CameraAnimationKeys.Twist, ref twist);
            TryUpdateKeyedValue(CameraAnimationKeys.Distance, ref distance);

            //Update the rotation handling for look at types
            if (RotationLookat)
            {
                Vector3 direction = (position - target).Normalized();
                rotation.X = MathF.Asin(direction.Y);
                rotation.Y = -MathF.Atan2(direction.X, direction.Z);
            }
        }

        private float animationFrameCount;
        private float animationFrameTime;
        private Dictionary<CameraAnimationKeys, float> animationTarget;
        private Dictionary<CameraAnimationKeys, float> animationCurrent;

        /// <summary>
        /// Starts a simple camera animation between the current and given target values during a given frame count.
        /// </summary>
        public void StartCameraAnimation(Dictionary<CameraAnimationKeys, float> animTarget, float frameCount)
        {
            animationCurrent = GetCurrentKeys();
            animationTarget = animTarget;
            animationFrameCount = frameCount;
            animationFrameTime = 0;

            if (AnimationTimer != null) {
                AnimationTimer.Stop();
                AnimationTimer.Dispose();
            }

            //Start an animation timer for animating the camera
            AnimationTimer = new System.Timers.Timer();
            AnimationTimer.Interval = (int)(1000.0f / 60.0f);
            AnimationTimer.Elapsed += Animation_Tick;
            AnimationTimer.Start();
        }

        private void Animation_Tick(object sender, EventArgs e)
        {
            if (animationFrameTime >= animationFrameCount)
            {
                EndCameraAnimation();
                return;
            }

            //Left and right key frames
            float leftFrame = 0;
            float rightFrame = animationFrameCount - 1;

            foreach (var track in animationTarget)
            {
                //Left and right values
                var leftValue = animationCurrent[track.Key];
                var rightValue = track.Value;

                //Weight calculation
                float frameDiff = animationFrameTime - leftFrame;
                float weight = frameDiff / (rightFrame - leftFrame);
                //Lerp between the track values
                var lerp = Toolbox.Core.Animations.InterpolationHelper.Lerp(leftValue, rightValue, weight);
                //Apply lerp value
                AnimationKeys[track.Key] = lerp;
            }
            UpdateMatrices();
            GLContext.ActiveContext.UpdateViewport = true;

            animationFrameTime++;
        }

        private void EndCameraAnimation()
        {
            animationFrameTime = 0; 
            animationFrameCount = 0;

            AnimationTimer.Stop();
            AnimationTimer?.Dispose();
            AnimationTimer = null;

            ApplyAnimationKeys(AnimationKeys);

            ResetAnimations();
        }

        private void ApplyAnimationKeys(Dictionary<CameraAnimationKeys, float> keys)
        {
            foreach (var key in keys)
            {
                switch (key.Key)
                {
                    case CameraAnimationKeys.PositionX: _targetPosition.X = key.Value; break;
                    case CameraAnimationKeys.PositionY: _targetPosition.Y = key.Value; break;
                    case CameraAnimationKeys.PositionZ: _targetPosition.Z = key.Value; break;
                    case CameraAnimationKeys.FieldOfView: Fov = key.Value; break;
                    case CameraAnimationKeys.Distance:
                        //Apply distance via position on the fly camera
                        if (cameraMode == CameraMode.FlyAround) 
                            _targetPosition += Vector3.Transform(InverseRotationMatrix, new Vector3(0, 0, key.Value));
                        else
                            TargetDistance = key.Value;
                        break;
                    case CameraAnimationKeys.Far: ZFar = key.Value; break;
                    case CameraAnimationKeys.Near: ZNear = key.Value; break;
                }
            }
        }

        private Dictionary<CameraAnimationKeys, float> GetCurrentKeys()
        {
            Dictionary<CameraAnimationKeys, float> keyValues = new Dictionary<CameraAnimationKeys, float>();
            keyValues.Add(CameraAnimationKeys.Distance, this.Distance);
            keyValues.Add(CameraAnimationKeys.Far, this.ZFar);
            keyValues.Add(CameraAnimationKeys.Near, this.ZNear);
            keyValues.Add(CameraAnimationKeys.FieldOfView, this.Fov);
            keyValues.Add(CameraAnimationKeys.PositionX, this.TargetPosition.X);
            keyValues.Add(CameraAnimationKeys.PositionY, this.TargetPosition.Y);
            keyValues.Add(CameraAnimationKeys.PositionZ, this.TargetPosition.Z);
            keyValues.Add(CameraAnimationKeys.RotationX, this.RotationX);
            keyValues.Add(CameraAnimationKeys.RotationY, this.RotationY);
            keyValues.Add(CameraAnimationKeys.RotationZ, 0);
            return keyValues;
        }

        private void TryUpdateKeyedValue(CameraAnimationKeys key, ref float current)
        {
            if (AnimationKeys.ContainsKey(key))
                current = AnimationKeys[key];
        }

        //Frame handling

        public void FrameBoundingSphere(Vector4 boundingSphere) {
            FrameBoundingSphere(boundingSphere.Xyz, boundingSphere.W, 0);
        }

        public void FrameBoundingSphere(Vector3 center, float radius, float offset)
        {
            // Find the min to avoid clipping for non square aspect ratios.
            float fovHorizontal = (float)(2 * Math.Atan(Math.Tan(Fov / 2) * AspectRatio));
            float minFov = Math.Min(Fov, fovHorizontal);

            // Calculate the height of a right triangle using field of view and the sphere radius.
            float distance = radius / (float)Math.Tan(minFov / 2.0f);

            Vector3 translation = Vector3.Zero;

            translation.X = center.X;
            translation.Y = center.Y;

            float distanceOffset = offset / minFov;
            translation.Z =  (distance + distanceOffset);

            if (Mode == CameraMode.Inspect)
            {
                TargetPosition = new Vector3(translation.X, translation.Y, 0);
                TargetDistance = translation.Z;
            }
            else
                TargetPosition = translation;
        }

        /// <summary>
        /// Gets the 3D coordinates for the given mouse XY coordinates and depth value.
        /// </summary>
        /// <returns></returns>
        public Vector3 CoordFor(int x, int y, float depth)
        {
            Vector3 vec;

            Vector2 normCoords = OpenGLHelper.NormMouseCoords(x, Height - y, Width, Height);
            Vector3 cameraPosition = TargetPosition + invRotationMatrix.Row2 * Distance;

            vec.X = (normCoords.X * depth) * FactorX;
            vec.Y = (normCoords.Y * depth) * FactorY;
            vec.Z = depth - Distance;

            return -cameraPosition + Vector3.Transform(invRotationMatrix, vec);
        }

        public Camera() {
            UpdateMode();
        }

        public void KeyPress(KeyEventInfo e)
        {
            if (e.IsKeyDown(InputSettings.INPUT.Camera.CameraFront))
                Direction = FaceDirection.Front;
            if (e.IsKeyDown(InputSettings.INPUT.Camera.CameraLeft)) 
                Direction = FaceDirection.Left;
            if (e.IsKeyDown(InputSettings.INPUT.Camera.CameraRight))
                Direction = FaceDirection.Right;
            if (e.IsKeyDown(InputSettings.INPUT.Camera.CameraTop))
                Direction = FaceDirection.Top;
            if (e.IsKeyDown(InputSettings.INPUT.Camera.CameraOrtho))
                IsOrthographic = !IsOrthographic;
        }

        private void UpdateMode()
        {
            if (Mode == CameraMode.Inspect)
                Controller = new InspectCameraController(this);
            else if (Mode == CameraMode.FlyAround)
                Controller = new FlyCameraController(this);
            else
                throw new Exception($"Invalid camera mode! {Mode}");

            ResetViewportTransform();
        }

        private void UpdateDirection()
        {
            switch (faceDirection)
            {
                case FaceDirection.Top:
                    RotationX = 1.570796f;
                    RotationY = 0.0f;
                    break;
                case FaceDirection.Bottom:
                    RotationX = -1.570796f;
                    RotationY = 0.0f;
                    break;
                case FaceDirection.Front:
                    RotationX = 0.0f;
                    RotationY = 0.0f;
                    break;
                case FaceDirection.Back:
                    RotationX = 0.0f;
                    RotationY = 3.14159f;
                    break;
                case FaceDirection.Left:
                    RotationX = 0.0f;
                    RotationY = 1.570796f;
                    break;
                case FaceDirection.Right:
                    RotationX = 0.0f;
                    RotationY = -1.570796f;
                    break;
            }
        }

        public enum FaceDirection
        {
            Any,
            Top,
            Bottom,
            Front,
            Back,
            Left,
            Right,
        }

        public enum CameraMode
        {
            FlyAround,
            Inspect,
        }
    }

    public class FlyCameraController : ICameraController
    {
        private Camera _camera;

        private float rotFactorX => _camera.InvertRotationX ? -0.002f : 0.002f * _camera.RotationSpeed;
        private float rotFactorY => _camera.InvertRotationY ? -0.002f : 0.002f * _camera.RotationSpeed;

        public FlyCameraController(Camera camera)
        {
            _camera = camera;
        }

        public void MouseClick(MouseEventInfo e, KeyEventInfo k) { }

        public void MouseMove(MouseEventInfo e, KeyEventInfo k, Vector2 previousLocation)
        {
            var position = new Vector2(e.X, e.Y);
            var movement = new Vector2(position.X, position.Y) - previousLocation;

            //Click + Control then drag mouse for zooming in/out.
            if ((e.RightButton == ButtonState.Pressed) && !_camera.LockRotation)
            {
                if (k.KeyCtrl)
                {
                    //Based on depth of nearest object. Zoom is less intense near closer objects
                    float delta = ((float)movement.Y * -200 * Math.Min(0.01f, _camera.Depth / 500f));
                    Vector3 vec;
                    vec.X = 0;
                    vec.Y = 0;
                    vec.Z = delta;

                    _camera.TargetPosition += Vector3.Transform(_camera.InverseRotationMatrix, vec);
                }
                else
                {
                    //Rotation from mouse movement
                    if (!k.IsKeyDown(InputSettings.INPUT.Camera.AxisY))
                        _camera.RotationX += movement.Y * rotFactorX;
                    if (!k.IsKeyDown(InputSettings.INPUT.Camera.AxisX))
                        _camera.RotationY += movement.X * rotFactorY;

                    //Reset direction
                    _camera.Direction = Camera.FaceDirection.Any;
                }
            }
        }

        public void MouseWheel(MouseEventInfo e, KeyEventInfo k)
        {
            //Standard mouse scroll zoom in/out
            //Increase amount based on key shift
            float delta = e.Delta * (k.KeyShift ? 80 : 50) * _camera.ZoomSpeed;

            Vector3 vec;

            Vector2 normCoords = OpenGLHelper.NormMouseCoords(e.X, e.Y, _camera.Width, _camera.Height);

            //Place the camera near the current mouse coordinates
            vec.X = (-normCoords.X * delta) * _camera.FactorX;
            vec.Y = (normCoords.Y * delta) * _camera.FactorY;
            vec.Z = delta;

            _camera.TargetPosition -= Vector3.Transform(_camera.InverseRotationMatrix, vec);
        }

        public void KeyPress(KeyEventInfo e)
        {
            //Control is used for other key combinations. Only focus on up/down movement.
            if (e.KeyCtrl)
            {
                if (!e.IsKeyDown(InputSettings.INPUT.Camera.MoveDown) ||
                    !e.IsKeyDown(InputSettings.INPUT.Camera.MoveUp))
                    return;
            }

            float movement = 0.2f * _camera.KeyMoveSpeed;
            Vector3 vec = Vector3.Zero;

            if (e.KeyShift)
                movement *= 2;

            //Key movements
            if (e.IsKeyDown(InputSettings.INPUT.Camera.MoveForward))
                vec.Z -= movement;
            if (e.IsKeyDown(InputSettings.INPUT.Camera.MoveBack))
                vec.Z += movement;
            if (e.IsKeyDown(InputSettings.INPUT.Camera.MoveLeft))
                vec.X -= movement;
            if (e.IsKeyDown(InputSettings.INPUT.Camera.MoveRight))
                vec.X += movement;

            //Up/down directions
            if (e.IsKeyDown(InputSettings.INPUT.Camera.MoveDown))
                vec.Y -= movement;
            else if (e.IsKeyDown(InputSettings.INPUT.Camera.MoveUp))
                vec.Y += movement;

            float UP = 0;

            _camera.TargetPosition += Vector3.Transform(_camera.InverseRotationMatrix, vec) + Vector3.UnitY * UP;
        }
    }

    public class InspectCameraController : ICameraController
    {
        public bool PanDirectly = false;

        private Camera _camera;

        private float rotFactorX => _camera.InvertRotationX ? -0.01f : 0.01f * _camera.RotationSpeed;
        private float rotFactorY => _camera.InvertRotationY ? -0.01f : 0.01f * _camera.RotationSpeed;

        public InspectCameraController(Camera camera)
        {
            _camera = camera;
        }

        public void MouseClick(MouseEventInfo e, KeyEventInfo k) { }

        public void MouseMove(MouseEventInfo e, KeyEventInfo k, Vector2 previousLocation)
        {
            var position = e.Position;
            var movement = new Vector2(position.X, position.Y) - previousLocation;

            if (e.RightButton == ButtonState.Pressed && !_camera.LockRotation)
            {
                if (k.KeyCtrl)
                {
                    //Zoom for control + mouse click and drag
                    _camera._targetDistance *= 1 - movement.Y * -5 * 0.001f;
                }
                else
                {
                    //Camera rotation during drag
                    if (!k.IsKeyDown(InputSettings.INPUT.Camera.AxisY))
                        _camera.RotationX += movement.Y * rotFactorX;
                    if (!k.IsKeyDown(InputSettings.INPUT.Camera.AxisX))
                        _camera.RotationY += movement.X * rotFactorY;

                    //Reset direction
                    _camera.Direction = Camera.FaceDirection.Any;
                }
            }
            //Pan via right click in 2D ortho view when camera is locked
            if (e.RightButton == ButtonState.Pressed && _camera.Is2D)
            {
                //Mouse pan from left click
                Pan(movement.X * _camera.PanSpeed, movement.Y * _camera.PanSpeed);
            }

            //3D Pam
            if (e.LeftButton == ButtonState.Pressed && !_camera.Is2D)
            {
                //Mouse pan from left click
                Pan(movement.X * _camera.PanSpeed, movement.Y * _camera.PanSpeed);
            }

            _camera.UpdateMatrices();
        }

        public void MouseWheel(MouseEventInfo e, KeyEventInfo k)
        {
            if (k.KeyCtrl)
            {
                //Zoom into hovered mouse coordinates
                float delta = -e.Delta * Math.Min(0.1f, _camera.Depth / 500f);

                delta *= _camera.TargetDistance;

                Vector2 normCoords = OpenGLHelper.NormMouseCoords(e.X, e.Y, _camera.Width, _camera.Height);
                Vector3 vec = _camera.InverseRotationMatrix.Row0 * -normCoords.X * delta * _camera.FactorX +
                              _camera.InverseRotationMatrix.Row1 * normCoords.Y * delta * _camera.FactorY +
                              _camera.InverseRotationMatrix.Row2 * delta;

                _camera.TargetPosition += vec;
            }
            else
            {
                //Standard mouse scroll
                Zoom(e.Delta * 0.1f * _camera.ZoomSpeed, true);
            }
        }

        public void KeyPress(KeyEventInfo e)
        {

        }

        private void Pan(float xAmount, float yAmount, bool scaleByDistanceToOrigin = true)
        {
            // Find the change in normalized screen coordinates.
            float deltaX = -xAmount / _camera.Width;
            float deltaY = yAmount / _camera.Height;

            if (scaleByDistanceToOrigin)
            {
                // Translate the camera based on the distance from the target and field of view.
                // Objects will "follow" the mouse while panning.
                deltaY *= ((float)Math.Sin(_camera.Fov) * _camera._targetDistance);
                deltaX *= ((float)Math.Sin(_camera.Fov) * _camera._targetDistance);
            }

            Matrix3 mtx = _camera.InverseRotationMatrix;
            // Regular panning.
            _camera._targetPosition += mtx.Row1 * deltaY;
            _camera._targetPosition += mtx.Row0 * deltaX;

            _camera.UpdateMatrices();
        }

        private void Zoom(float amount, bool scaleByDistanceToOrigin)
        {
            float zoomScale = 1;
            // Increases zoom speed when zooming out. 
            if (scaleByDistanceToOrigin && _camera._targetDistance > 0)
                zoomScale *= _camera._targetDistance;
            else //Constant scaling amount
                zoomScale = 1f;

            _camera._targetDistance -= amount * zoomScale;

            _camera.UpdateMatrices();
        }
    }
}
