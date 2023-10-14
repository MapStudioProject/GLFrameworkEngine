using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    /// <summary>
    /// A tool to help make boxes. Generally used for bounding regions and areas.
    /// </summary>
    public class BoxCreationTool
    {
        public EventHandler BoxCreated = null;

        /// <summary>
        /// Determines if the tool is active or not.
        /// </summary>
        public bool IsActive = false;
        
        /// <summary>
        /// Gets the vertices of the box.
        /// </summary>
        public Vector3[] GetVertices()
        {
            var box = GetBox();
            return box.GetVertices();
        }

        /// <summary>
        /// Gets the center of the box.
        /// </summary>
        public Vector3 GetCenter()
        {
            var box = GetBox();
            return box.GetCenter();
        }

        /// <summary>
        /// Gets the scale factor of the box.
        /// </summary>
        public Vector3 GetScale()
        {
            var box = GetBox();
            return new Vector3((box.Max - box.Min) * 0.5f);
        }

        /// Gets the rotation of the box.
        /// </summary>
        public Quaternion GetRotation()
        {
            return Rotation.ExtractRotation();
        }

        //Gets a bounding box represented by the start/end points
        public BoundingBox GetBox()
        {
           /* var rotationInv = Rotation.Inverted();
            //Convert to local space
            var startPoint = Vector3.Transform(StartPoint, rotationInv);
            var endPoint = Vector3.Transform(EndPoint, rotationInv);
            */
            BoundingBox.CalculateMinMax(new Vector3[] { StartPoint, EndPoint }, out Vector3 min, out Vector3 max);
           return BoundingBox.FromMinMax(min, max);
        }

        //Setup a marker for a visual to place the starting point at
        static SpawnMarker marker = new SpawnMarker() { IsVisible = true };

        //Starting position
        Vector3 StartPoint = Vector3.Zero;
        //Ending position
        Vector3 EndPoint = Vector3.Zero;
        //End position applied from bottom/top
        Vector3 EndPointTB = Vector3.Zero;
        //Rotaiton of box for resizing at an angle
        Matrix3 Rotation = Matrix3.Identity;
        //previous move position
        Vector3 previousPosition = Vector3.Zero;
        //Total delta offset for dragging
        Vector3 offset = Vector3.Zero;
        //Edit state of the box
        State BoxState = State.None;

        public void Render(GLContext context)
        {
            if (!IsActive)
                return;

            //Create marker to spawn the corners of the box
            if (BoxState == State.None)
            {
                marker.SetCursor(context);
                marker.DrawModel(context, Pass.OPAQUE);
            }
            else //Draw the vertices of the box itself (in lines)
            {
                BoundingBox.CalculateMinMax(new Vector3[] { StartPoint, EndPoint }, out Vector3 min, out Vector3 max);

                var scaleMtx = Matrix4.CreateScale((max - min) * 0.5f);
                var transMtx = Matrix4.CreateTranslation((min + max) * 0.5f);
                var rotMtx = new Matrix4(Rotation);

                var mat = new StandardMaterial();
                mat.Color = new Vector4(GLConstants.SelectColor.Xyz, 1f);
                mat.ModelMatrix = scaleMtx * rotMtx * transMtx;
                mat.Render(context);

                GL.LineWidth(3);

                BoundingBoxRender.Draw(context, new Vector3(-1), new Vector3(1));
            }
        }

        public void OnMouseDown(GLContext context, MouseEventInfo e)
        {
            if (!IsActive || e.LeftButton != OpenTK.Input.ButtonState.Pressed)
                return;

            //Start box action if not started yet
            if (BoxState == State.None)
                Start(context);
            else if (BoxState == State.CreateWidth)
            {
                //Previous movement
                previousPosition = Vector3.Zero;
                //Total offset to shift corner height
                offset = Vector3.Zero;
                //Height Y plane
                if (!context.Camera.Is2D)
                    BoxState = State.CreateHeight;
                else //Height creation skip (used for 2D view)
                {
                    BoxState = State.None;
                    Apply();
                }
            }
            else if (BoxState == State.CreateHeight)
            {
                BoxState = State.None;
                Apply();
            }
        }

        public void Reset()
        {
            BoxState = State.None;
            //Previous movement
            previousPosition = Vector3.Zero;
            //Total offset to shift corner height
            offset = Vector3.Zero;
        }

        public void Start(GLContext context)
        {
            marker.SetCursor(context);

            //Previous movement
            previousPosition = Vector3.Zero;
            //Total offset to shift corner
            offset = Vector3.Zero;
            //Starting position
            StartPoint = marker.Transform.Position;
            //Rotation of box
            //    Rotation = Matrix3.CreateFromQuaternion(marker.Transform.Rotation);
            //Create X/Z plane first
            BoxState = State.CreateWidth;
        }

        private void Apply()
        {
            if (!IsActive)
                return;

            IsActive = false;
            BoxCreated?.Invoke(this, EventArgs.Empty);
        }

        public void OnMouseMove(GLContext context, MouseEventInfo e)
        {
            if (!IsActive)
                return;

            if (BoxState == State.CreateWidth)
            {
                //Set the width
                var point = new Vector2(e.X, e.Y);
                var ray = context.PointScreenRay((int)point.X, (int)point.Y);
                var localMatrix = Rotation;

                //Move on the X/Z plane
                var localX = Vector3.Transform(Vector3.UnitX, localMatrix);
                var localZ = Vector3.Transform(Vector3.UnitZ, localMatrix);
                var plane_normal = Vector3.Cross(localX, localZ);

                if (ray.IntersectsPlane(plane_normal, StartPoint, out float intersectDist))
                {
                    Vector3 hitPos = ray.Origin.Xyz + (ray.Direction * intersectDist);

                    Vector3 newPosition = Vector3.Zero;
                    //Move on the X/Z plane
                    newPosition += localX * Vector3.Dot(hitPos, localX);
                    newPosition += localZ * Vector3.Dot(hitPos, localZ);

                    //Set the previous position if not set yet to find the differences in movement
                    if (previousPosition == Vector3.Zero)
                        previousPosition = newPosition;

                    //Drag the new point from the start point
                    Vector3 localDelta = newPosition - previousPosition;
                    offset += localDelta;
                    previousPosition = newPosition;

                    EndPoint = StartPoint + offset;
                    EndPointTB = EndPoint;
                }
            }
            if (BoxState == State.CreateHeight)
            {
                //Set the width
                var point = new Vector2(e.X, e.Y);
                var ray = context.PointScreenRay((int)point.X, (int)point.Y);
                var localMatrix = Rotation;

                Vector3 distance =  context.Camera.GetViewPostion();
                Vector3 axis_vector = Vector3.Transform(Vector3.UnitY, localMatrix);
                Vector3 plane_tangent = Vector3.Cross(axis_vector, distance);
                //Move on the Y plane
                Vector3 plane_normal = Vector3.Cross(axis_vector, plane_tangent);
                if (ray.IntersectsPlane(plane_normal, EndPointTB, out float intersectDist))
                {
                    Vector3 hitPos = ray.Origin.Xyz + (ray.Direction * intersectDist);

                    Vector3 newPosition = Vector3.Zero;
                    //Move on the Y plane
                    newPosition += axis_vector * Vector3.Dot(hitPos, axis_vector);

                    //Set the previous position if not set yet to find the differences in movement
                    if (previousPosition == Vector3.Zero)
                        previousPosition = newPosition;

                    //Drag the new point from the start point
                    Vector3 localDelta = newPosition - previousPosition;
                    offset += localDelta;
                    previousPosition = newPosition;

                    EndPoint = EndPointTB + offset;
                }
            }
        }

        public void OnMouseUp(GLContext context, MouseEventInfo e)
        {
            if (!IsActive || e.LeftButton != OpenTK.Input.ButtonState.Released)
                return;

            if (BoxState == State.CreateWidth)
            {
                //Previous movement
                previousPosition = Vector3.Zero;
                //Total offset to shift corner height
                offset = Vector3.Zero;
                //Height Y plane
                if (!context.Camera.Is2D)
                    BoxState = State.CreateHeight;
                else //Height creation skip (used for 2D view)
                {
                    BoxState = State.None;
                    Apply();
                }
            }
        }

        enum State
        {
            None,
            CreateWidth,
            CreateHeight,
        }
    }
}
