using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core;

namespace GLFrameworkEngine
{
    public partial class RenderablePath : IDrawable, IEditModeObject, IColorPickable, ITransformableObject
    {
        public static float PathDisplaySliceCount = 16;

        StandardMaterial LineMaterial = new StandardMaterial();
        LineRender PickingLineRender;
        EventHandler PickingChanged;

        int SelectedLineIndex;

        public virtual void DrawColorPicking(GLContext context)
        {
            if (EditMode && EditToolMode == ToolMode.Erase)
                DrawLinePicking(context);

            if (EditMode)
                return;

            //For empty bezier types, draw an origin point to pick where the current bezier is located at
            if (PathPoints.Count == 0 && InterpolationMode == Interpolation.Bezier)
            {
                if (OriginRenderer == null) OriginRenderer = new SphereRender(1);
                OriginRenderer.DrawPicking(context, this, Transform.TransformMatrix);
            }

            DrawLineDisplay(context, true);
        }

        public virtual void DrawLineTool(GLContext context)
        {
            if (drawPointStack == null || drawPointStack?.Count == 0)
                return;

            if (DrawPointTool == null)
                DrawPointTool = new LineRender();

            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < drawPointStack.Count; i++)
                points.Add(context.ScreenToWorld(drawPointStack[i].X, drawPointStack[i].Y, 30));

            var mat = new StandardMaterial();
            mat.Render(context);
            DrawPointTool.Draw(context, points, true);
        }

        private void DrawLinePicking(GLContext context)
        {
            List<Vector3> points = new List<Vector3>();
            List<Vector4> colors = new List<Vector4>();

            for (int i = 0; i < PathPoints.Count; i++)
            {
                for (int j = 0; j < PathPoints[i].Children.Count; j++)
                {
                    points.Add(PathPoints[i].Transform.Position);
                    points.Add(PathPoints[i].Children[j].Transform.Position);
                    var color = context.ColorPicker.SetPickingColor(new LineObject(PathPoints[i], j));
                    colors.Add(color);
                    colors.Add(color);
                }
            }
            if (PickingLineRender == null)
                PickingLineRender = new LineRender();

            GL.LineWidth(LineWidth + 5);

            var mat = new StandardMaterial();
            mat.displayOnlyVertexColors = true;
            mat.Render(context);

            PickingLineRender.Draw(context, points, colors, true);

            GL.LineWidth(1);
        }

        class LineObject : ITransformableObject
        {
            public GLTransform Transform { get; set; } = new GLTransform();
            public bool IsHovered { get; set; }
            public bool CanSelect { get; set; } = true;

            private bool isSelected;
            public bool IsSelected
            {
                get { return isSelected; }
                set
                {
                    isSelected = value;
                }
            }

            public RenderablePathPoint Point;
            public int ChildIndex;

            public LineObject(RenderablePathPoint point, int childIndex) {
                Point = point;
                ChildIndex = childIndex;
            }
        }

        public virtual void DrawModel(GLContext context, Pass pass)
        {
            foreach (var pathPoint in PathPoints)
                pathPoint.CameraScale = context.Camera.ScaleByCameraDistance(pathPoint.Transform.Position);

            if (EditMode && EditToolMode == ToolMode.Drawing)
                DrawLineTool(context);

            if (XRayMode)
            {
                GL.Disable(EnableCap.DepthTest);
            }

            if (pass == Pass.OPAQUE)
            {
                DrawLineDisplay(context);

                if (EditMode && InterpolationMode == Interpolation.Linear)
                    DrawArrowDisplay(context);
            }

            if (EditMode)
            {
                foreach (var pathPoint in PathPoints)
                    if (pathPoint.IsVisible)
                        pathPoint.Render(context, pass);

                //Draw a point preview of a point before creation
                if (CreatePointDisplay != null && EditToolMode == ToolMode.Create)
                    CreatePointDisplay.Render(context, pass);
            }
            else
            {
                //For empty bezier types, draw an origin point to display where the current bezier is located at
                if (PathPoints.Count == 0 && InterpolationMode == Interpolation.Bezier)
                {
                    if (OriginRenderer == null) OriginRenderer = new SphereRender(1);

                    OriginRenderer.DrawSolidWithSelection(context, Transform.TransformMatrix, Vector4.One, false);
                }
            }

            if (XRayMode)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Lequal);
                GL.DepthRange(0.0, 1.0);
                GL.DepthMask(true);
            }
        }


        public virtual void DrawLineDisplay(GLContext context, bool picking = false)
        {
            Vector3 offset = new Vector3(0, LineOffset, 0);
            List<Vector3> points = new List<Vector3>();
            List<Vector3> handles = new List<Vector3>();

            //Draw bezier lines
            if (InterpolationMode == Interpolation.Bezier) {
                DrawLinesBezier(ref points, ref handles);
            }
            else //Draw linear lines from point to next point
            {
                for (int i = 0; i < PathPoints.Count; i++)
                {
                    if (!PathPoints[i].IsVisible)
                        continue;

                    foreach (var nextPt in PathPoints[i].Children)
                    {
                        points.Add((PathPoints[i].Transform.Position + offset));
                        points.Add((nextPt.Transform.Position + offset));
                    }

                    if (Loop && i == PathPoints.Count - 1 && i != 0)
                    {
                        points.Add((PathPoints[i].Transform.Position + offset));
                        points.Add((PathPoints[0].Transform.Position + offset));
                    }

                    if (PathPoints[i] == StartConnecitonPoint && EditToolMode == ToolMode.Connection)
                    {
                        points.Add((PathPoints[i].Transform.Position + offset));
                        points.Add(this.ConnectionPoint);
                    }
                    //Draw a line connection for selected points between the creation point
                    if (KeyEventInfo.State.KeyAlt && PathPoints[i].IsSelected && EditToolMode == ToolMode.Create && CreatePointDisplay != null)
                    {
                        points.Add((PathPoints[i].Transform.Position + offset));
                        points.Add(CreatePointDisplay.Transform.Position);
                    }
                }
            }
            DrawLines(context, points, handles, picking);
        }

        public void DrawLines(GLContext context,
            List<Vector3> points,
            List<Vector3> handles,
            bool picking = false)
        {

            //Prepare renderers
            if (LineRenderer == null)
                LineRenderer = new LineRender(PrimitiveType.Lines);
            if (LineHandleRenderer == null)
                LineHandleRenderer = new LineRender(PrimitiveType.Lines);
            if (BezierLineArrowRenderer == null)
                BezierLineArrowRenderer = new LineRender(PrimitiveType.Lines);

            if (picking)
            {
                var shader = GlobalShaders.GetShader("PICKING");
                context.CurrentShader = shader;
                context.ColorPicker.SetPickingColor(this, context.CurrentShader);
            }
            else
            {
                LineMaterial.Color = LineColor;

                if ((IsSelected || IsHovered) && !EditMode)
                    LineMaterial.Color = new Vector4(GLConstants.SelectColor.Xyz, 1f);

                LineMaterial.Render(context);
            }

            if (!EditMode && !picking)
                GL.LineWidth(5);
            else if (InterpolationMode == RenderablePath.Interpolation.Bezier)
                GL.LineWidth(BezierLineWidth);
            else
                GL.LineWidth(LineWidth);

            if (InterpolationMode == RenderablePath.Interpolation.Bezier)
            {
                if (picking)
                    GL.LineWidth(30);

                LineRenderer.UpdatePrimitiveType(PrimitiveType.LineStrip);
                if (points.Count > 0)
                    LineRenderer.Draw(context, points, new List<Vector4>(), true);

                if (EditMode)
                {
                    float length = BezierArrowLength * BezierPointScale;

                    List<Vector3> arrowPoints = new List<Vector3>();
                    for (int i = 0; i < points.Count; i++)
                    {
                        if (i == points.Count - 1 && !Loop)
                            break;

                        Vector3 nextPoint = points[0];
                        if (i < points.Count - 1)
                            nextPoint = points[i + 1];

                        Vector3 dist = nextPoint - points[i];
                        var rotation = STMath.RotationFromTo(new Vector3(0, 0, 1), dist.Normalized());

                        var line1 = Vector3.TransformNormal(new Vector3(length, 0, 0), rotation);
                        var line2 = Vector3.TransformNormal(new Vector3(-length, 0, 0), rotation);

                        arrowPoints.Add(points[i] + line1);
                        arrowPoints.Add(nextPoint);

                        arrowPoints.Add(points[i] + line2);
                        arrowPoints.Add(nextPoint);
                    }

                    if (arrowPoints.Count > 0)
                        BezierLineArrowRenderer.Draw(context, arrowPoints, new List<Vector4>(), true);
                }
            }
            else
            {
                if (points.Count > 0)
                    LineRenderer.Draw(context, points, new List<Vector4>(), true);
            }

            LineMaterial.Color = new Vector4(0.7f, 0, 0, 1);
            LineMaterial.Render(context);

            if (handles.Count > 0)
                LineHandleRenderer.Draw(context, handles, new List<Vector4>(), true);

            //DrawNormals(context);

            GL.LineWidth(1);
        }

        //For drawing directional normals to determine path twist direction
        private void DrawNormals(GLContext context)
        {
            GL.LineWidth(1.0F);

            //Normals drawer
            float displayLength = 5.0f * RenderablePath.BezierPointScale;

            List<Vector3> normals = new List<Vector3>();
            for (int i = 0; i < PathPoints.Count; i++)
            {
                if (!PathPoints[i].IsVisible)
                    continue;

                var pos = PathPoints[i].Transform.Position;
                normals.Add(pos);
                normals.Add(pos + (PathPoints[i].Normal * displayLength));
            }

            LineMaterial.Color = new Vector4(1, 1, 0, 1);
            LineMaterial.Render(context);
            LineHandleRenderer.Draw(context, normals, new List<Vector4>(), true);
        }

        public virtual void DrawArrowDisplay(GLContext context)
        {
            for (int i = 0; i < PathPoints.Count; i++)
            {
                if (!PathPoints[i].IsVisible)
                    continue;

                var children = PathPoints[i].Children;
                foreach (var nextPt in children)
                {
                    DrawArrow(context, PathPoints[i], nextPt);
                }

                if (Loop && i == PathPoints.Count - 1 && i != 0)
                    DrawArrow(context, PathPoints[i], PathPoints[0]);
            }
        }

        private void DrawArrow(GLContext context, RenderablePathPoint point, RenderablePathPoint nextPt)
        {
            //Scale the arrow with the current point size
            Vector3 scale = new Vector3(ArrowScale);
            scale *= PointSize;
            scale *= point.CameraScale;
            if (InterpolationMode == RenderablePath.Interpolation.Bezier)
                scale = new Vector3(0.05f);

            //Get the distance between the next and current point
            var dist = nextPt.Transform.Position - point.Transform.Position;
            //Rotate based on the direction of the distance to point at the next point
            Matrix4 rotation = RotationFromTo(new Vector3(0, 0.00000001f, 1), dist.Normalized());

            //Keep the cone rotated 90 degrees to not face upwards
            var rot = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(90));
            //Offset the arrow slightly from it's position
            Matrix4 offsetMat = Matrix4.CreateTranslation(new Vector3(0, 0, -25));
            Vector3 lineOffset = new Vector3(0, LineOffset, 0);
            Matrix4 translateMat = Matrix4.CreateTranslation(nextPt.Transform.Position + lineOffset);

            if (IsArrowCentered)
            {
                offsetMat = Matrix4.Identity;
                //Use the center point between the distance
                translateMat = Matrix4.CreateTranslation((point.Transform.Position + (dist / 2f)) + lineOffset);
            }

            //Load the cone render
            if (ConeRenderer == null)
                ConeRenderer = new ConeRenderer(10, 2, 15, PathDisplaySliceCount);

            //Draw the cone with a solid shader
            Matrix4 modelMatrix = offsetMat * Matrix4.CreateScale(scale) * rotation * translateMat;
            ConeRenderer.DrawSolid(context, rot * modelMatrix, ArrowColor);
        }

        static Matrix4 RotationFromTo(Vector3 start, Vector3 end)
        {
            var axis = Vector3.Cross(start, end).Normalized();

            var angle = Vector3.CalculateAngle(start, end);
            return Matrix4.CreateFromAxisAngle(axis, angle);
        }

        private void DrawLinesBezier(ref List<Vector3> points, ref List<Vector3> handles)
        {
            //Create a list of points to interpolate
            Vector3[] connectLinePositions = GetBezierPoints();

            int posIndex = 0;
            for (int i = 0; i < PathPoints.Count; i++)
            {
                var point = PathPoints[i];
                if (!point.IsVisible)
                    continue;

                if (EditMode)
                {
                    //Draw the handles from current point to the handle point in world space
                    if (point.ControlPoint1.Transform.Position != Vector3.Zero)
                    {
                        handles.Add(connectLinePositions[posIndex]);
                        handles.Add(connectLinePositions[posIndex + 1]);
                    }
                    if (point.ControlPoint2.Transform.Position != Vector3.Zero)
                    {
                        handles.Add(connectLinePositions[posIndex]);
                        handles.Add(connectLinePositions[posIndex + 2]);
                    }
                }

                if (i == PathPoints.Count - 1 && !Loop)
                    break;

                //Check if any control points are used based on zeroed local space
                if (point.ControlPoint1.Transform.Position != Vector3.Zero ||
                    point.ControlPoint2.Transform.Position != Vector3.Zero)
                {
                    //Interpolate between the control points
                    Vector3 p0 = connectLinePositions[posIndex];
                    Vector3 p1 = connectLinePositions[posIndex + 2];
                    Vector3 p2 = connectLinePositions[posIndex + 4];
                    Vector3 p3 = connectLinePositions[posIndex + 3];

                    for (float t = 0f; t <= 1.0; t += 0.0625f)
                    {
                        float u = 1f - t;
                        float tt = t * t;
                        float uu = u * u;
                        float uuu = uu * u;
                        float ttt = tt * t;

                        var pt = (uuu * p0 +
                                        3 * uu * t * p1 +
                                        3 * u * tt * p2 +
                                            ttt * p3);

                        points.Add(pt);
                    }
                }
                else
                {
                    //Else display the direct lines between the current and next non handle point
                    points.Add(connectLinePositions[posIndex]);
                    points.Add(connectLinePositions[posIndex + 3]);
                }
                posIndex += 3;
            }
        }

        private Vector3[] GetBezierPoints()
        {
            int posIndex = 0;

            //Create a list of points to interpolate
            Vector3[] connectLinePositions = new Vector3[PathPoints.Count * 3];
            //Add the last point to interpolate back to at the end if looped
            if (this.Loop)
                connectLinePositions = new Vector3[(PathPoints.Count + 1) * 3];

            for (int i = 0; i < PathPoints.Count; i++)
            {
                var point = PathPoints[i];

                //Add position and control points 
                connectLinePositions[posIndex] = point.Transform.Position;
                connectLinePositions[posIndex + 1] = point.ControlPoint1.Transform.Position;
                connectLinePositions[posIndex + 2] =  point.ControlPoint2.Transform.Position;

                posIndex += 3;

                //Loop back to first point if needed
                if (i == PathPoints.Count - 1 && i != 0 && this.Loop)
                {
                    //Add position and control points 
                    connectLinePositions[posIndex] = PathPoints[0].Transform.Position;
                    connectLinePositions[posIndex + 1] = PathPoints[0].ControlPoint1.Transform.Position;
                    connectLinePositions[posIndex + 2] = PathPoints[0].ControlPoint2.Transform.Position;
                }
            }
            return connectLinePositions;
        }
    }
}
