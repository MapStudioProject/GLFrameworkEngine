using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core.ViewModels;
using GLFrameworkEngine.UI;

namespace GLFrameworkEngine
{
    public class RenderablePathPoint : ITransformableObject, IRayCastPicking, IRenderNode
    {
        public virtual string Name => $"Point {Index}";

        /// <summary>
        /// The tree node attached to the point for the outliner.
        /// </summary>
        public NodeBase UINode { get; set; }

        /// <summary>
        /// The transformation of the point.
        /// </summary>
        public GLTransform Transform { get; set; } = new GLTransform();

        /// <summary>
        /// Determines if the current point is being hovered or not.
        /// </summary>
        public bool IsHovered { get; set; }

        /// <summary>
        /// Determines if the current point is being selected or not.
        /// </summary>
        public virtual bool IsSelected
        {
            //Only select points while the path is in an editable state.
            get { return _isSelected; }
            set {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    ParentPath.UpdateSelectionList(this, value);
                }
            }
        }

        private bool _isSelected;

        /// <summary>
        /// Ray bounding size for the sphere.
        /// </summary>
        public float RaySphereSize = 10;

        /// <summary>
        /// The camera scale for the point to scale by camera distance.
        /// </summary>
        public float CameraScale = 1.0f;

        /// <summary>
        /// The normal vector of the point.
        /// </summary>
        public Vector3 Normal = new Vector3(0, 1, 0);

        private bool canSelect = true;

        /// <summary>
        /// Determines if the point can be selected or not.
        /// </summary>
        public bool CanSelect
        {
            get
            {
                //Make sure it is in edit mode and not being over a point during a hover connection
                return canSelect && !IsPointOver && ParentPath.EditMode;
            }
            set
            {
                canSelect = value;
            }
        }

        public virtual RenderablePathPoint Duplicate()
        {
            return this.ParentPath.CreatePoint(Transform.Position);
        }

        public virtual BoundingBox BoundingBox { get; }  = new BoundingBox(new Vector3(-0.5f), new Vector3(0.5f));

        /// <summary>
        /// Gets the bounding ray for ray testing the current point sphere.
        /// </summary>
        public virtual BoundingNode GetRayBounding()
        {
            float size = RaySphereSize * CameraScale;
            var box = new BoundingBox(new Vector3(-(size / 2)), new Vector3(size / 2));

            return new BoundingNode()
            {
                //Note boundings get transformed automatically
                Center = new Vector3(0),
                Radius = size,
                Box = box,
                TransformByScale = false,
            };
        }

        /// <summary>
        /// Gets the index of the point from the parent path.
        /// </summary>
        public int Index => ParentPath.PathPoints.IndexOf(this);

        /// <summary>
        /// Determines if the point is visible or not.
        /// </summary>
        public virtual bool IsVisible { get; set; } = true;

        /// <summary>
        /// Determines if the point is hovered over or not during a connection.
        /// </summary>
        public bool IsPointOver { get; set; }

        private List<RenderablePathPoint> _parents;
        private List<RenderablePathPoint> _children;

        public List<RenderablePathPoint> Parents
        {
            get {
                if (ParentPath.AutoConnectByNext)
                {
                    //Auto connect the last point for this kind of path connection
                    if (Index <= 0)
                        return new List<RenderablePathPoint>();

                    return new List<RenderablePathPoint>() { ParentPath.PathPoints[Index-1] };
                }
                return _parents; 
            }
            private set { _parents = value; }
        }

        public List<RenderablePathPoint> Children
        {
            get {
                if (ParentPath.AutoConnectByNext)
                {
                    //Auto connect the next point for this kind of path connection
                    if (Index == ParentPath.PathPoints.Count - 1)
                        return new List<RenderablePathPoint>();

                    return new List<RenderablePathPoint>() { ParentPath.PathPoints[Index+1] };
                }
                return _children;
            }
            private set { _children = value; }
        }

        //For bezier interpolation types
        public ControlPoint ControlPoint1;
        public ControlPoint ControlPoint2;

        public class ControlPoint : ITransformableObject, IRayCastPicking
        {
            public GLTransform Transform { get; set; } = new GLTransform();

            public float CameraScale = 1.0f;

            public bool IsHovered { get; set; }
            public bool IsSelected { get; set; }
            public bool CanSelect
            {
                get { return ParentPoint.CanSelect; }
                set { }
            }

            public Vector3 LocalPosition
            {
                get { return this.Transform.Position - ParentPoint.Transform.Position; }
                set { this.Transform.Position = ParentPoint.Transform.Position + value; }
            }

            public RenderablePathPoint ParentPoint;

            public ControlPoint(RenderablePathPoint point)
            {
                ParentPoint = point;
            }

            public virtual BoundingNode GetRayBounding()
            {
                return new BoundingNode()
                {
                    //Note boundings get transformed automatically
                    Center = Vector3.Zero,
                    Radius = 3 * RenderablePath.BezierPointScale * CameraScale,
                };
            }
        }

        public RenderablePath ParentPath;

        //Resources 
        public static SphereRender SphereRender;

        public static void Init() {
            SphereRender = new SphereRender(1, 32);
        }

        private TransformInfo PreviousTransform;
        private TransformInfo PreviousControlPoint1;
        private TransformInfo PreviousControlPoint2;

        public RenderablePathPoint(RenderablePath path, Vector3 position)
        {
            ParentPath = path;
            ControlPoint1 = new ControlPoint(this);
            ControlPoint2 = new ControlPoint(this);

            UINode = new RenderablePath.PointNode(this);
            if (path.PointUITagType != null)
                UINode.Tag = Activator.CreateInstance(path.PointUITagType);

            Transform.Position = position;
            Transform.UpdateMatrix(true);

            Transform.BeforeTransformUpdate += OnBeforeTransformChanged;
            Transform.TransformActionApplied += OnTransformApplied;

            if (path.InterpolationMode == RenderablePath.Interpolation.Bezier)
                SetupBezierTransformHandling();

            Parents = new List<RenderablePathPoint>();
            Children = new List<RenderablePathPoint>();

            if (path.InterpolationMode == RenderablePath.Interpolation.Bezier)
                RaySphereSize = 3;
        }

        public virtual void Render(GLContext context, Pass pass)
        {
            if (SphereRender == null)
                Init();

            if (pass == Pass.OPAQUE)
            {
                //Scale from camera position
                CameraScale = context.Camera.ScaleByCameraDistance(Transform.Position);
                var color = ParentPath.PointColor;
                if (ParentPath.GetPointColor != null)
                    color = ParentPath.GetPointColor(this);

                if (ParentPath.InterpolationMode == RenderablePath.Interpolation.Linear)
                {
                    float scale = 10 * RenderablePath.PointSize;
                    RaySphereSize = scale;

                    var matrix = Matrix4.CreateScale(scale * CameraScale) * Transform.TransformNoScaleMatrix;
                    SphereRender.DrawSolidWithSelection(context, matrix,
                        color, IsSelected || IsHovered);
                }
                else
                {
                    RaySphereSize = 3 * RenderablePath.BezierPointScale;

                    var matrix = Matrix4.CreateScale(2 * RenderablePath.BezierPointScale * CameraScale) * Transform.TransformNoScaleMatrix;

                    SphereRender.DrawSolidWithSelection(context, matrix,
                        color, IsSelected || IsHovered);

                    DrawControlPoint(context, ControlPoint1);
                    DrawControlPoint(context, ControlPoint2);
                }
            }
            if (pass == Pass.TRANSPARENT)
            {
                //Diplay only when enabled and the scale is visible
                if (RenderablePath.DisplayPointSize && (Transform.Scale.X > 2 || Transform.Scale.Y > 2 || Transform.Scale.Z > 2))
                {
                    Matrix4 boundingTransform = Matrix4.CreateScale(0.5f) * Transform.TransformMatrix;
                    DrawSizeSphere(context, boundingTransform, Pass.TRANSPARENT);
                }
            }
        }

        public virtual void DrawSizeSphere(GLContext context, Matrix4 matrix, Pass pass)
        {
            GLMaterialBlendState.Translucent.RenderBlendState();

            Vector4 blockColor = new Vector4(1, 1, 0, 0.2f);
            SphereRender.DrawSolid(context, matrix, blockColor);

            GLMaterialBlendState.Opaque.RenderBlendState();
        }

        public void ResetTransformHandling()
        {
            //Custom on start events
            Transform.TransformStarted = null;
            ControlPoint1.Transform.TransformStarted = null;
            ControlPoint2.Transform.TransformStarted = null;
            //Custom moving
            Transform.CustomTranslationActionCallback = null;
            ControlPoint1.Transform.CustomTranslationActionCallback = null;
            ControlPoint2.Transform.CustomTranslationActionCallback = null;
            //Custom rotation handling
            Transform.CustomRotationActionCallback = null;
            ControlPoint1.Transform.CustomRotationActionCallback = null;
            ControlPoint2.Transform.CustomRotationActionCallback = null;
            //Custom scale handling
            Transform.CustomScaleActionCallback = null;
            ControlPoint1.Transform.CustomScaleActionCallback = null;
            ControlPoint2.Transform.CustomScaleActionCallback = null;
        }

        public virtual void SetupBezierTransformHandling()
        {
            //Setup the control points for transform handling
            Transform.TransformStarted += CustomUndoHandler;
            ControlPoint1.Transform.TransformStarted += CustomUndoHandler;
            ControlPoint2.Transform.TransformStarted += CustomUndoHandler;

            //Setup additional settings for handles
            ControlPoint1.Transform.TransformStarted += HandleSetupTransformAction;
            ControlPoint2.Transform.TransformStarted += HandleSetupTransformAction;

            //Setup custom translation handling
            Transform.CustomTranslationActionCallback += OnPointMoved;
            ControlPoint1.Transform.CustomTranslationActionCallback += OnControlPoint1Moved;
            ControlPoint2.Transform.CustomTranslationActionCallback += OnControlPoint2Moved;
            //Setup custom rotation handling
            Transform.CustomRotationActionCallback += OnPointRotated;
            ControlPoint1.Transform.CustomRotationActionCallback += OnPointRotated;
            ControlPoint2.Transform.CustomRotationActionCallback += OnPointRotated;
            //Setup custom scale handling
            Transform.CustomScaleActionCallback += OnPointScaled;
            ControlPoint1.Transform.CustomScaleActionCallback += OnControlPoint1Scaled;
            ControlPoint2.Transform.CustomScaleActionCallback += OnControlPoint2Scaled;
        }

        private void HandleSetupTransformAction(object sender, EventArgs e)
        {
            if (this.IsSelected)
                return;

            //For transforming handles, use the point origin
            var transformTools = GLContext.ActiveContext.TransformTools;
            foreach (var action in transformTools.ActiveActions)
            {
                if (!(action is TranslateAction))
                    transformTools.TransformSettings.Origin = this.Transform.Position;
            }
        }

        private void CustomUndoHandler(object sender, EventArgs e)
        {
            //Setup the control point for an undo operation
            PreviousTransform = new TransformInfo(this.Transform);
            PreviousControlPoint1 = new TransformInfo(ControlPoint1.Transform);
            PreviousControlPoint2 = new TransformInfo(ControlPoint2.Transform);

            //Add to undo. Note this method is ran in UndoBegin() {  } UndoEnd() for batch undo operations
            GLContext.ActiveContext.Scene.AddToUndo(new TransformUndo(new TransformInfo[] {
                   PreviousTransform, PreviousControlPoint1, PreviousControlPoint2,
                }));
        }

        private void OnControlPoint1Moved(object sender, EventArgs e)
        {
            var arguments = sender as GLTransform.CustomTranslationArgs;
            MoveControlHandles(arguments.Translation, ControlPoint1, ControlPoint2);
        }

        private void OnControlPoint2Moved(object sender, EventArgs e)
        {
            var arguments = sender as GLTransform.CustomTranslationArgs;
            MoveControlHandles(arguments.Translation, ControlPoint2, ControlPoint1);
        }

        private void MoveControlHandles(Vector3 translation, ControlPoint targetHandle, ControlPoint nextHandle)
        {
            //Only move individual handles when the main point is not selected
            if (this.IsSelected)
                return;

            //Direction of the moving handle and the point position
            var dir = targetHandle.Transform.Position - Transform.Position;
            dir.Normalize();

            //Set the direction of the handle to the left handle length relative from the point.
            var handleAligned = nextHandle.LocalPosition.Length * dir;

            //Translate the current handle
            targetHandle.Transform.Position = translation;
            //Align the first control handle
            if (KeyEventInfo.State.KeyCtrl)
                nextHandle.Transform.Position = this.Transform.Position - handleAligned;

            UpdateMatrices();
        }

        private void OnPointRotated(object sender, EventArgs e)
        {
            var arguments = sender as GLTransform.CustomRotationArgs;
            var rotation = arguments.DeltaRotation;
            //Apply the rotation with the handles moving
            var previous1 = PreviousControlPoint1.PrevPosition;
            var previous2 = PreviousControlPoint2.PrevPosition;
            //Rotate the control point handles
            Transform.Position = Vector3.TransformPosition(PreviousTransform.PrevPosition - arguments.Origin, Matrix4.CreateFromQuaternion(rotation)) + arguments.Origin;
            ControlPoint1.Transform.Position = Vector3.TransformPosition(previous1 - arguments.Origin, Matrix4.CreateFromQuaternion(rotation)) + arguments.Origin;
            ControlPoint2.Transform.Position = Vector3.TransformPosition(previous2 - arguments.Origin, Matrix4.CreateFromQuaternion(rotation)) + arguments.Origin;
            //Apply matrices
            UpdateMatrices();
        }

        private void OnPointTwisted(object sender, EventArgs e)
        {

        }

        private void OnPointMoved(object sender, EventArgs e)
        {
            var arguments = sender as GLTransform.CustomTranslationArgs;
            var translation = arguments.Translation - arguments.PreviousTransform.Position;
            //Apply the rotation with the handles moving
            var previousPoint = arguments.PreviousTransform.Position;
            var previous1 = PreviousControlPoint1.PrevPosition;
            var previous2 = PreviousControlPoint2.PrevPosition;
            //Move both the point and each control handle
            ControlPoint1.Transform.Position = previous1 + translation;
            ControlPoint2.Transform.Position = previous2 + translation;
            Transform.Position = previousPoint + translation;

            //Apply matrices
            UpdateMatrices();
        }

        private void OnPointScaled(object sender, EventArgs e)
        {
            var arguments = sender as GLTransform.CustomScaleArgs;
            //Scale all point positions
            Transform.Position = (arguments.PreviousTransform.Position - arguments.Origin) * arguments.Scale + arguments.Origin;
            ControlPoint1.Transform.Position = (PreviousControlPoint1.PrevPosition - arguments.Origin) * arguments.Scale + arguments.Origin;
            ControlPoint2.Transform.Position = (PreviousControlPoint2.PrevPosition - arguments.Origin) * arguments.Scale + arguments.Origin;
            UpdateMatrices();
        }

        private void OnControlPoint1Scaled(object sender, EventArgs e)
        {
            //Only scale individual handles when the main point is not selected
            if (this.IsSelected)
                return;

            var arguments = sender as GLTransform.CustomScaleArgs;
            ControlPoint1.Transform.Position = (PreviousControlPoint1.PrevPosition - arguments.Origin) * arguments.Scale + arguments.Origin;
            UpdateMatrices();
        }

        private void OnControlPoint2Scaled(object sender, EventArgs e)
        {
            //Only scale individual handles when the main point is not selected
            if (this.IsSelected)
                return;

            var arguments = sender as GLTransform.CustomScaleArgs;
            ControlPoint2.Transform.Position = (PreviousControlPoint2.PrevPosition - arguments.Origin) * arguments.Scale + arguments.Origin;
            UpdateMatrices();
        }

        public void UpdateMatrices()
        {
            Transform.UpdateMatrix(true);
            ControlPoint1.Transform.UpdateMatrix(true);
            ControlPoint2.Transform.UpdateMatrix(true);
        }

        public virtual void AddChild(RenderablePathPoint point, bool keepScale = false)
        {
            if (keepScale)
                point.Transform.Scale = this.Transform.Scale;

            point.Parents.Add(this);
            Children.Add(point);
        }

        public void RemoveChild(RenderablePathPoint point)
        {
            point.Parents.Remove(this);
            Children.Remove(point);
        }

        private void DrawControlPoint(GLContext context, ControlPoint controlPoint)
        {
            float distance = (context.Camera.GetViewPostion() - controlPoint.Transform.Position).Length;
            controlPoint.CameraScale = Math.Max(distance * 0.002f, 1.0f);

            var matrix = Matrix4.CreateScale(2 * RenderablePath.BezierPointScale * controlPoint.CameraScale) * controlPoint.Transform.TransformNoScaleMatrix;
            var color = new Vector4(ParentPath.PointColor.Xyz * 0.5F, 1.0f);

            SphereRender.DrawSolidWithSelection(context, matrix,
                color, controlPoint.IsHovered || controlPoint.IsSelected);
        }

        private void OnBeforeTransformChanged(object sender, EventArgs e)
        {
            foreach (var point in ParentPath.PathPoints) {
                //Connect points through the transformation when one is over another
                //This is so connections can be previewed before being applied
                if (point.IsPointOver && !point.IsSelected) {
                    Transform.Position = point.Transform.Position;
                }
            }
        }

        private void OnTransformApplied(object sender, EventArgs e)
        {
            //Create a list of possible points being hovered over
            List<RenderablePathPoint> pointsConnected = new List<RenderablePathPoint>();
            foreach (var point in ParentPath.PathPoints) {
                if (point.IsPointOver && !point.IsSelected)
                    pointsConnected.Add(point);
            }

            //Undo the operation
            if (pointsConnected.Count > 0) {
                GLContext.ActiveContext.Scene.AddToUndo(new RevertableConnectPointCollection(pointsConnected, this));
            }

            //Reset the point over points
            foreach (var point in ParentPath.PathPoints)
                point.IsPointOver = false;

            //Connect them all to this point
            foreach (var point in pointsConnected)
                ConnectToPoint(point);
        }

        public virtual void ConnectToPoint(RenderablePathPoint point)
        {
            List<RenderablePathPoint> parents = new List<RenderablePathPoint>();
            List<RenderablePathPoint> children = new List<RenderablePathPoint>();

            foreach (var pt in Parents)
                parents.Add(pt);

            foreach (var pt in Children)
                children.Add(pt);

            foreach (var child in children)
                point.AddChild(child);

            foreach (var parent in parents)
                parent.AddChild(point);

            foreach (var parent in parents)
                parent.Children.Remove(this);

            foreach (var child in children)
                child.Parents.Remove(this);

            ParentPath.RemovePoint(this);
        }

        public virtual void SelectAll()
        {
            this.IsSelected = true;
            if (ParentPath.InterpolationMode == RenderablePath.Interpolation.Bezier)
            {
                this.ControlPoint1.IsSelected = true;
                this.ControlPoint2.IsSelected = true;
            }
        }

        public virtual void DeselectAll()
        {
            this.IsSelected = false;
            this.IsHovered = false;
            if (ParentPath.InterpolationMode == RenderablePath.Interpolation.Bezier)
            {
                this.ControlPoint1.IsSelected = false;
                this.ControlPoint1.IsHovered = false;
                this.ControlPoint2.IsSelected = false;
                this.ControlPoint2.IsHovered = false;
            }
        }
    }
}
