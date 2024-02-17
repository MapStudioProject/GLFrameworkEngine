using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using GLFrameworkEngine.UI;
using Toolbox.Core.ViewModels;

namespace GLFrameworkEngine
{
    public partial class RenderablePath : IDrawable, IEditModeObject, IColorPickable, IRenderNode, IDrawableInput
    {
        public virtual string Name => $"Path {UINode.Index}";

        public EventHandler OnSelected;
        public EventHandler OnSelectedChanged;
        public EventHandler OnEditModeToggled;

        public bool IsActive = false;

        private bool _editMode = false;
        public virtual bool EditMode
        {
            get { return _editMode; }
            set
            {
                if (_editMode != value) {
                    _editMode = value;
                    OnEditModeToggled?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public virtual bool CanToggleEditMode { get; set; } = false;

        public GLTransform Transform { get; set; } = new GLTransform();

        public bool IsHovered { get; set; }
        public bool CanSelect { get; set; } = true;


        private bool isSelected;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                //Update the selection origin
                if (value)
                {
                    OnSelected?.Invoke(this, EventArgs.Empty);
                    Transform.Position = CalculateOrigin();
                    Transform.UpdateMatrix(true);
                }
                OnSelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public virtual IEnumerable<ITransformableObject> Selectables
        {
            get
            {
                List<ITransformableObject> objects = new List<ITransformableObject>();
                for (int i = 0; i < PathPoints.Count; i++) {
                    objects.Add(PathPoints[i]);
                    if (this.InterpolationMode == Interpolation.Bezier) {
                        objects.Add(PathPoints[i].ControlPoint1);
                        objects.Add(PathPoints[i].ControlPoint2);
                    }
                }
                return objects;
            }
        }

        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// A list of all the points used in the path object.
        /// </summary>
        public List<RenderablePathPoint> PathPoints = new List<RenderablePathPoint>();

        private List<RenderablePathPoint> SelectedPathPoints = new List<RenderablePathPoint>();

        public bool ConnectHoveredPoints = true;

        /// <summary>
        /// Determines to scale points based on camera distance.
        /// </summary>
        public bool ScaleByCamera = true;

        /// <summary>
        /// Determines to scale points based on configurable point sizes.
        /// </summary>
        public bool ScaleByPointSize = true;

        /// <summary>
        /// Determines the factor scale points based on camera distance.
        /// </summary>
        public static float CameraScaleFactor = 0.1f;

        /// <summary>
        /// Determines the arrow size
        /// </summary>
        public virtual float ArrowScale { get; set; } = 1;

        /// <summary>
        /// Determines to draw the arrow in between points. Defaults to the child if disabled.
        /// </summary>
        public virtual bool IsArrowCentered { get; set; } = false;

        /// <summary>
        /// Determines the thickness of the line connecting the points
        /// </summary>
        public virtual float LineWidth { get; set; } = 30.0f;

        /// <summary>
        /// The scale to default to when a point is created
        /// </summary>
        public virtual Vector3 DefaultScale { get; set; } = new Vector3(2);

        /// <summary>
        /// The offset of the line in the Y axis when drawn. 
        /// Can prevent z fighting points depenting on how the point is drawn
        /// </summary>
        public virtual float LineOffset { get; set; } = 0.0f;

        /// <summary>
        /// Determines to auto connect points by the next point as one connection to the next.
        /// </summary>
        public bool AutoConnectByNext = false;

        public virtual Vector4 LineColor { get; set; } = new Vector4(1);
        public virtual Vector4 ArrowColor { get; set; } = new Vector4(1);
        public virtual Vector4 PointColor { get; set; } = new Vector4(1);

        public Func<RenderablePathPoint, Vector4> GetPointColor;

        public static float PointSize  = 1;

        public static bool DisplayPointSize = true;

        public bool XRayMode;

        private Interpolation _interpolationMode;
        public Interpolation InterpolationMode
        {
            get { return _interpolationMode; }
            set
            {
                if (_interpolationMode != value) {
                    _interpolationMode = value;

                    foreach (var point in PathPoints)
                    {
                        point.ResetTransformHandling();

                        if (value == Interpolation.Bezier)
                            point.SetupBezierTransformHandling();

                    }
                }
            }
        }

        public bool Loop = false;

        public static float BezierPointScale = 5.0f;
        public static float BezierLineWidth = 5.0f;
        public static float BezierArrowLength = 2.0f;

        //UI
        public NodeBase UINode { get; set; }

        public Type PathUITagType;
        public Type PointUITagType;

        //Events
        public EventHandler PointAddedCallback;
        public EventHandler PointRemovedCallback;
        public EventHandler RemovedSelectedCallback;

        public EventHandler AddCallback;
        public EventHandler RemoveCallback;

        //Renders
        SphereRender OriginRenderer;
        //Line rendering
        public LineRender LineRenderer; //line connection between children
        LineRender BezierLineArrowRenderer; //bezier interpolated lines
        LineRender LineHandleRenderer; //bezier handles
        //Drawing tool
        LineRender DrawPointTool;
        public static float SegmentDrawPointLength = 200;
        //Creation tool
        RenderablePathPoint CreatePointDisplay;

        private static ToolMode _editToolMode = ToolMode.Transform;
        public static ToolMode EditToolMode
        {
            get { return _editToolMode; }
            set
            {
                if (_editToolMode != value) {
                    _editToolMode = value;
                    ResetToolMode();
                }
            }
        }

        public enum ToolMode
        {
            Transform, //Default transform movement
            Connection, //Connect from mouse down to hover on dest point
            Create, //Create on mouse click
            Drawing, //Create points while the mouse is moving around while held down.
            Erase, //Erase on mouse click
        }

        static void ResetToolMode()
        {
            if (_editToolMode == ToolMode.Create)
            {
                GLContext.ActiveContext.PickingTools.Enabled = false;
                GLContext.ActiveContext.TransformTools.Enabled = false;
            }
            else
            {
                GLContext.ActiveContext.PickingTools.Enabled = true;
                GLContext.ActiveContext.TransformTools.Enabled = true;
            }
        }

        /// <summary>
        /// Automatically connect to the hovered point in the connection tool.
        /// </summary>
        public static bool ConnectAuto = false;

        RenderablePathPoint StartConnecitonPoint;
        Vector3 ConnectionPoint;

        ConeRenderer ConeRenderer;

        public RenderablePath()
        {
            Transform.TransformStarted += PrepareTransform;
            Transform.CustomTranslationActionCallback += TranslateAllPoints;
            Transform.CustomRotationActionCallback += RotateAllPoints;
            Transform.CustomScaleActionCallback += ScaleAllPoints;

            UINode = new PathNode(this);
        }

        //We need to update the selection list in the order points are selected
        internal void UpdateSelectionList(RenderablePathPoint point, bool selected)
        {
            if (selected)
                SelectedPathPoints.Add(point);
            else
                SelectedPathPoints.Remove(point);
            GLContext.ActiveContext.Scene.OnSelectionChanged(GLContext.ActiveContext, point);
        }

        //Batch transform editing
        private TransformInfo[] previousTransforms;

        private void PrepareTransform(object sender, EventArgs e)
        {
            previousTransforms = new TransformInfo[PathPoints.Count * 3];
            for (int i = 0; i < PathPoints.Count; i++)
            {
                int index = i * 3;
                previousTransforms[index] = new TransformInfo(PathPoints[i].Transform);
                previousTransforms[index + 1] = new TransformInfo(PathPoints[i].ControlPoint1.Transform);
                previousTransforms[index + 2] = new TransformInfo(PathPoints[i].ControlPoint2.Transform);
            }
            GLContext.ActiveContext.Scene.AddToUndo(new TransformUndo(previousTransforms));
        }

        public void Translate(Vector3 position)
        {
            TranslateDelta(position - CalculateOrigin());
        }

        public void TranslateDelta(Vector3 diff)
        {
            for (int i = 0; i < PathPoints.Count; i++)
            {
                PathPoints[i].Transform.Position +=  diff;
                PathPoints[i].ControlPoint1.Transform.Position += diff;
                PathPoints[i].ControlPoint2.Transform.Position += diff;
                PathPoints[i].UpdateMatrices();
            }
            this.Transform.Position = CalculateOrigin();
        }

        public void ScalePoints(Vector3 scale)
        {
            Vector3 origin = this.Transform.Position;

            for (int i = 0; i < PathPoints.Count; i++)
            {
                int index = i * 3;

                PathPoints[i].Transform.Position = (PathPoints[i].Transform.Position - origin) * scale + origin;
                PathPoints[i].ControlPoint1.Transform.Position = (PathPoints[i].ControlPoint1.Transform.Position - origin) * scale + origin;
                PathPoints[i].ControlPoint2.Transform.Position = (PathPoints[i].ControlPoint2.Transform.Position - origin) * scale + origin;
                PathPoints[i].UpdateMatrices();
            }
            this.Transform.Position = CalculateOrigin();
            this.Transform.UpdateMatrix(true);
        }

        private void ScaleAllPoints(object sender, EventArgs e)
        {
            var arguments = (GLTransform.CustomScaleArgs)sender;

            for (int i = 0; i < PathPoints.Count; i++)
            {
                int index = i * 3;
                var previous = previousTransforms[index];
                var previous1 = previousTransforms[index + 1];
                var previous2 = previousTransforms[index + 2];

                PathPoints[i].Transform.Position = (previous.PrevPosition - arguments.Origin) * arguments.Scale + arguments.Origin;
                PathPoints[i].ControlPoint1.Transform.Position = (previous1.PrevPosition - arguments.Origin) * arguments.Scale + arguments.Origin;
                PathPoints[i].ControlPoint2.Transform.Position = (previous2.PrevPosition - arguments.Origin) * arguments.Scale + arguments.Origin;
                PathPoints[i].UpdateMatrices();
            }
            this.Transform.Position = CalculateOrigin();
            this.Transform.UpdateMatrix(true);
        }

        private void RotateAllPoints(object sender, EventArgs e)
        {
            var arguments = (GLTransform.CustomRotationArgs)sender;
            var rotation = arguments.Rotation;

            for (int i = 0; i < PathPoints.Count; i++)
            {
                int index = i * 3;
                var previous = previousTransforms[index];
                var previous1 = previousTransforms[index + 1];
                var previous2 = previousTransforms[index + 2];

                PathPoints[i].Transform.Position = Vector3.TransformPosition(previous.PrevPosition - arguments.Origin, Matrix4.CreateFromQuaternion(rotation)) + arguments.Origin;
                PathPoints[i].ControlPoint1.Transform.Position = Vector3.TransformPosition(previous1.PrevPosition - arguments.Origin, Matrix4.CreateFromQuaternion(rotation)) + arguments.Origin;
                PathPoints[i].ControlPoint2.Transform.Position = Vector3.TransformPosition(previous2.PrevPosition - arguments.Origin, Matrix4.CreateFromQuaternion(rotation)) + arguments.Origin;
                PathPoints[i].UpdateMatrices();
            }
            this.Transform.Position = CalculateOrigin();
            this.Transform.UpdateMatrix(true);
        }

        private void TranslateAllPoints(object sender, EventArgs e)
        {
            var arguments = (GLTransform.CustomTranslationArgs)sender;
            for (int i = 0; i < PathPoints.Count; i++)
            {
                int index = i * 3;
                var previous = previousTransforms[index];
                var previous1 = previousTransforms[index+1];
                var previous2 = previousTransforms[index+2];

                PathPoints[i].Transform.Position = previous.PrevPosition + arguments.TranslationDelta;
                PathPoints[i].ControlPoint1.Transform.Position = previous1.PrevPosition + arguments.TranslationDelta;
                PathPoints[i].ControlPoint2.Transform.Position = previous2.PrevPosition + arguments.TranslationDelta;
                PathPoints[i].UpdateMatrices();
            }
            this.Transform.Position = CalculateOrigin();
            this.Transform.UpdateMatrix(true);
        }

        /// <summary>
        /// Calculates the center origin of all the selected path points.
        /// </summary>
        /// <returns></returns>
        public Vector3 CalculateSelectedOrigin()
        {
            //Calculate center between object selection for origin
            //The origin will determine gizmo placement and rotation pivot
            List<Vector3> points = new List<Vector3>();
            foreach (var ob in PathPoints)
                if (ob.IsSelected)
                    points.Add(ob.Transform.Position);

            BoundingBox.CalculateMinMax(points.ToArray(), out Vector3 min, out Vector3 max);
            return (min + max) * 0.5f;
        }

        /// <summary>
        /// Calculates the center origin of all the path points.
        /// </summary>
        /// <returns></returns>
        public Vector3 CalculateOrigin()
        {
            //Calculate center between object selection for origin
            //The origin will determine gizmo placement and rotation pivot
            List<Vector3> points = new List<Vector3>();
            foreach (var ob in PathPoints)
                points.Add(ob.Transform.Position);

            BoundingBox.CalculateMinMax(points.ToArray(), out Vector3 min, out Vector3 max);
            return (min + max) * 0.5f;
        }

        /// <summary>
        /// Gets all the selected points in the path object.
        /// </summary>
        /// <returns></returns>
        public List<RenderablePathPoint> GetSelectedPoints() {
            return SelectedPathPoints;
        }

        /// <summary>
        /// Gets all the hovered points in the path object.
        /// </summary>
        /// <returns></returns>
        public List<RenderablePathPoint> GetHoveredPoints()
        {
            return PathPoints.Where(x => x.IsHovered).ToList();
        }

        public virtual RenderablePathPoint CreatePoint(Vector3 position)
        {
            var point = new RenderablePathPoint(this, position);
            point.UINode.Icon = this.UINode.Icon;
            point.UINode.IconColor = this.UINode.IconColor;
            point.Transform.Scale = DefaultScale;
            point.Transform.UpdateMatrix(true);
            return point;
        }

        public virtual void AddPoint(RenderablePathPoint point, int index = -1)
        {
            if (index != -1 && PathPoints.Count > index)
                PathPoints.Insert(index, point);
            else
                PathPoints.Add(point);

            if (index != -1 && UINode.Children.Count > index)
                UINode.Children.Insert(index, point.UINode);
            else
                UINode.AddChild(point.UINode);

            PointAddedCallback?.Invoke(point, EventArgs.Empty);
        }

        public virtual void OnPointAdded(RenderablePathPoint point)
        {
            //Property UI update
            GLContext.ActiveContext.Scene.SelectionUIChanged?.Invoke(point.UINode, EventArgs.Empty);
        }

        public void RemoveSelected()
        {
            var selected = GetSelectedPoints().ToList();
            if (selected.Count == 0)
                return;

            GLContext.ActiveContext.Scene.AddToUndo(new RevertableDelPointCollection(selected));

            foreach (var obj in selected)
                RemovePoint(obj);

            RemovedSelectedCallback?.Invoke(selected, EventArgs.Empty);

            //Clear transforms present
            GLContext.ActiveContext.TransformTools.Clear();
        }

        public virtual void RemovePoint(RenderablePathPoint point)
        {
            point.IsSelected = false;

            RemovePointReferences(point);
            PathPoints.Remove(point);
            UINode.Children.Remove(point.UINode);

            PointRemovedCallback?.Invoke(point, EventArgs.Empty);
        }

        public virtual void OnAdded()
        {
            AddCallback?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnRemoved()
        {
            RemoveCallback?.Invoke(this, EventArgs.Empty);
        }

        public void DeselectAll()
        {
            for (int i = 0; i < PathPoints.Count; i++) {
                PathPoints[i].DeselectAll();
            }
        }

        public void SelectAll()
        {
            for (int i = 0; i < PathPoints.Count; i++)
                PathPoints[i].SelectAll();
        }

        public virtual void Dispose()
        {
            LineRenderer?.Dispose();
            ConeRenderer?.Dispose();
        }

        //Remove points from all children and parents
        public void RemovePointReferences(RenderablePathPoint removePoint)
        {
            foreach (var point in PathPoints)
            {
                if (point.Parents.Contains(removePoint))
                    point.Parents.Remove(removePoint);
                if (point.Children.Contains(removePoint))
                    point.Children.Remove(removePoint);
            }

            removePoint.Children.Clear();
            removePoint.Parents.Clear();
        }

        public enum Interpolation
        {
            Linear,
            Bezier,
        }
    }
}
