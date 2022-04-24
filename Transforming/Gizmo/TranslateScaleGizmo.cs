using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFrameworkEngine
{
    //https://github.com/Sage-of-Mirrors/WindEditor/blob/master/Editor/Editor/TransformGizmo.cs
    public class TranslateScaleGizmo : IGizmoRender
    {
        static bool DEBUG_MODE = false;

        public bool IsScale = false;

        public bool IsMultiGizmo = false;

        float AxisOffset = 0.0f;
        float MultiAxisOffset = 0.75f;
        float GizmoEndScale = 1;
        float MultiGizmoBoxScale = 0.4f;

        LineRender[] LineRenders = new LineRender[3];

        static readonly Vector3[] _colors = new Vector3[3] {
             GLConstants.AxisColorX,
             GLConstants.AxisColorY,
             GLConstants.AxisColorZ,
        };

        static readonly Vector4[] _rotations = new Vector4[3] {
            new Vector4(0, 1, 0, 90),  new Vector4(1, 0, 0, -90), new Vector4(0, 0, 1, -90)
        };

        static readonly Vector3[] _multiAxisColors = new Vector3[3] {
              _colors[2], _colors[0],  _colors[1]
        };

        static readonly Vector3[] _endPostions = new Vector3[3] {
            new Vector3(2, 0, 0),new Vector3(0, 2, 0),new Vector3(0, 0, 2)
        };

        public Vector3[] _multiAxisPositions = new Vector3[3] {
            new Vector3(1, 1, 0),
            new Vector3(0, 1, 1),
            new Vector3(1, 0, 1)
        };

        public readonly Vector4[] _multiAxisRotations = new Vector4[3] {
            new Vector4(0, 0, 1, -90),  new Vector4(0, 1, 0, -90), new Vector4(1, 0, 0, -90)
        };

        //AABB objects for ray hit checking when clicking an selecting parts of the gizmo object
        private List<BoundingBox> AxisOjects = new List<BoundingBox>();
        private BoundingBox CenterGizmoObject;

        //Shapes for drawing axis
        static ConeRenderer ConeRenderer = null;
        static CylinderRenderer CylinderRenderer = null;
        static PlaneRenderer PlaneRender = null;
        static PlaneRenderer PlaneLinesRender = null;
        static CubeRenderer CubeRenderer = null;

        public TranslateScaleGizmo() {
            SetupGizmo(false);
        }

        public void SetupGizmo(bool isMultiGizmo)
        {
            IsMultiGizmo = isMultiGizmo;
            AxisOffset = 0.0f;
            //Rectangle boxes for each axis object
            float boxLength = 2.0f + 1.0F;
            float boxHalfWidth = 0.25f;
            //Center of gizmo for free no restriction movement
            float centerSize = 0.40f;

            //Size for the 3 middle axis regions
            float midSize = 0.8f;
            float midThickness = 0.1f;
            float midOffset = 0.60f;
            float midLength = midOffset + midSize;

            MultiAxisOffset = 0.65f;
            GizmoEndScale = 1.0f;
            MultiGizmoBoxScale = 0.4f;

            if (isMultiGizmo)
            {
                MultiAxisOffset = 0.02f;
                midOffset = 0;
                midLength = 0.8f;

                if (!IsScale) {
                    AxisOffset = 1.5f;
                    GizmoEndScale = 1.0f;
                }
                else
                {
                    GizmoEndScale = 0.50f;
                    AxisOffset = -0.4f;
                }
            }
            CenterGizmoObject = new BoundingBox(new Vector3(-centerSize), new Vector3(centerSize));

            centerSize += AxisOffset;
            boxLength += AxisOffset;

            var xBounding = new BoundingBox(new Vector3(centerSize, -boxHalfWidth, -boxHalfWidth), new Vector3(boxLength, boxHalfWidth, boxHalfWidth));
            var yBounding = new BoundingBox(new Vector3(-boxHalfWidth, centerSize, -boxHalfWidth), new Vector3(boxHalfWidth, boxLength, boxHalfWidth));
            var zBounding = new BoundingBox(new Vector3(-boxHalfWidth, -boxHalfWidth, centerSize), new Vector3(boxHalfWidth, boxHalfWidth, boxLength));

            var xyBounding = new BoundingBox(new Vector3(midOffset, midOffset, -midThickness), new Vector3(midLength, midLength, midThickness));
            var yzBounding = new BoundingBox(new Vector3(-midThickness, midOffset, midOffset), new Vector3(midThickness, midLength, midLength));
            var xzBounding = new BoundingBox(new Vector3(midOffset, -midThickness, midOffset), new Vector3(midLength, midThickness, midLength));

            AxisOjects = new List<BoundingBox>() {
              xBounding, yBounding, zBounding,
              xyBounding,yzBounding,xzBounding,
            };
        }

        public void Init() {
            CubeRenderer = new CubeRenderer(0.30f);
            PlaneRender = new PlaneRenderer(1);
            PlaneLinesRender = new PlaneRenderer(1, PrimitiveType.LineLoop);
            ConeRenderer = new ConeRenderer(0.25f, 0, 1);
            CylinderRenderer = new CylinderRenderer(0.04f, 2);
            GlobalShaders.GetShader("GIZMO");
        }

        public TransformEngine.Axis UpdateAxisSelection(GLContext context, Vector3 position, Quaternion rotation, Vector2 point, TransformSettings settings)
        {
            var axis = TransformEngine.Axis.None;

            var ray = context.PointScreenRay((int)point.X, (int)point.Y);
            var gizmoScale = settings.GizmoScale;

            //Convert ray to local coordinates relative to the object's transform.
            CameraRay localRay = new CameraRay();
            localRay.Direction = Vector3.Transform(ray.Direction, rotation.Inverted());
            localRay.Origin = new Vector4(Vector3.Transform(ray.Origin.Xyz - position, rotation.Inverted()), localRay.Origin.W);

            if (!IsMultiGizmo && GLMath.RayIntersectsAABB(localRay, CenterGizmoObject.Min * gizmoScale, CenterGizmoObject.Max * gizmoScale, out float d))
            {
                axis = TransformEngine.Axis.All;
                return axis;
            }

            //Find selected axis
            for (int i = 0; i < AxisOjects.Count; i++)
            {
                if (GLMath.RayIntersectsAABB(localRay,
                    AxisOjects[i].Min * gizmoScale,
                    AxisOjects[i].Max * gizmoScale, out float dist))
                {
                    if (i == 0) return TransformEngine.Axis.X;
                    if (i == 1) return  TransformEngine.Axis.Y;
                    if (i == 2) return TransformEngine.Axis.Z;
                    if (i == 3)
                    {
                        axis |= TransformEngine.Axis.X;
                        axis |= TransformEngine.Axis.Y;
                        return axis;
                    }
                    if (i == 4)
                    {
                        axis |= TransformEngine.Axis.Y;
                        axis |= TransformEngine.Axis.Z;
                        return axis;
                    }
                    if (i == 5)
                    {
                        axis |= TransformEngine.Axis.X;
                        axis |= TransformEngine.Axis.Z;
                        return axis;
                    }
                }
            }
            return axis;
        }

        public void Render(GLContext context, Vector3 position, Quaternion rotation, float scale, bool isMoving, bool[] isSelected, bool[] isHovered) {
            if (ConeRenderer == null)
                Init();

            var shader = GlobalShaders.GetShader("GIZMO");
            context.CurrentShader = shader;

            GL.Disable(EnableCap.DepthTest);

            var translateMtx = Matrix4.CreateTranslation(position);
            var scaleMtx = Matrix4.CreateScale(scale);
            var rotationMtx = Matrix4.CreateFromQuaternion(rotation);
            var transform = scaleMtx * rotationMtx * translateMtx;

            if (!isMoving && !isSelected.Any(x => x)) {
                for (int i = 0; i < 3; i++) {
                    DrawAxis(context, isHovered[i], ref transform, _endPostions[i], _rotations[i], _colors[i]);
                }
                for (int i = 0; i < 3; i++) {
                    DrawMultiAxis(context, isHovered[i+3], ref transform, _multiAxisPositions[i], _multiAxisRotations[i], _multiAxisColors[i]);
                }
                
                Vector4 color = new Vector4(1);
                if (isHovered[6])
                    color = new Vector4(1, 1, 0, 1);

                if (!IsMultiGizmo)
                    GizmoCenterRender.Draw(context, position, scale, color);
            }

            //Draw lines for the selected axis objects during movement
            if (isMoving)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (!isSelected[i])
                        continue;

                    context.CurrentShader.SetVector4("color", new Vector4(_colors[i], 1.0f));
                    context.CurrentShader.SetVector4("selectionColor", Vector4.Zero);
                    context.CurrentShader.SetMatrix4x4("mtxMdl", ref transform);

                    if (LineRenders[i] == null) LineRenders[i] = new LineRender();

                    LineRenders[i].Draw(
                        GetSelectedAxisVector3(i) * context.Camera.ZFar,
                        -(GetSelectedAxisVector3(i) * context.Camera.ZFar),
                        new Vector4(1), false);
                }
            }

            if (DEBUG_MODE)
                DrawDebugBoundings(context, transform);

            context.CurrentShader = null;
            GL.Enable(EnableCap.DepthTest);
        }

        public void DrawDebugBoundings(GLContext context, Matrix4 transform)
        {
            GL.Disable(EnableCap.DepthTest);

            context.CurrentShader = GlobalShaders.GetShader("GIZMO");
            context.CurrentShader.SetMatrix4x4("mtxMdl", ref transform);

            for (int i = 0; i < AxisOjects.Count; i++) {
                DrawDebugBoundings(context, AxisOjects[i], Vector3.One);
            }
        }

        private void DrawDebugBoundings(GLContext context, BoundingBox bounding, Vector3 color)
        {
            context.CurrentShader.SetVector4("color", new Vector4(color, 1.0f));
            bounding.Draw(context);
        }

        public void DrawAxis(GLContext context, bool isSelected, ref Matrix4 transform, Vector3 endPosition, Vector4 rotation, Vector3 color)
        {
            var offset = endPosition * AxisOffset / 2.0f;

            var translateMtx = Matrix4.CreateTranslation(new Vector3(offset + endPosition * 0.15f));
            var rotate = Matrix4.CreateFromAxisAngle(rotation.Xyz, MathHelper.DegreesToRadians(rotation.W));
            var rotate90 = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(90));

            var output = rotate90 * rotate * translateMtx * transform;

            context.CurrentShader.SetVector4("color", new Vector4(color, 1.0f));
            context.CurrentShader.SetVector4("selectionColor", Vector4.Zero);
            if (isSelected)
                context.CurrentShader.SetVector4("selectionColor", new Vector4(1, 1, 0, 1));

            context.CurrentShader.SetMatrix4x4("mtxMdl", ref output);
            CylinderRenderer.Draw(context);

            translateMtx = Matrix4.CreateTranslation(offset + endPosition);
            rotate = Matrix4.CreateFromAxisAngle(rotation.Xyz, MathHelper.DegreesToRadians(rotation.W));
            var scaleMtx = Matrix4.CreateScale(GizmoEndScale);
            output = scaleMtx * rotate90 * rotate * translateMtx * transform;

            context.CurrentShader.SetMatrix4x4("mtxMdl", ref output);

            GL.Disable(EnableCap.CullFace);
            if (IsScale)
                CubeRenderer.Draw(context);
            else
                ConeRenderer.Draw(context);

            GL.Enable(EnableCap.CullFace);
        }

        public void DrawMultiAxis(GLContext context, bool isSelected, ref Matrix4 transform, Vector3 position, Vector4 rotation, Vector3 color)
        {
            var translateMtx = Matrix4.CreateTranslation(new Vector3(position * (MultiGizmoBoxScale + MultiAxisOffset)));
            var rotate = Matrix4.CreateFromAxisAngle(rotation.Xyz, MathHelper.DegreesToRadians(rotation.W));
            var scaleMtx = Matrix4.CreateScale(MultiGizmoBoxScale);

            var output = scaleMtx * rotate * translateMtx * transform;

            context.CurrentShader.SetVector4("selectionColor", Vector4.Zero);
            if (isSelected)
                context.CurrentShader.SetVector4("selectionColor", new Vector4(1, 1, 0, 1));

            GLMaterialBlendState.TranslucentAlphaOne.RenderBlendState();

            context.CurrentShader.SetVector4("color", new Vector4(color, 0.2f));
            context.CurrentShader.SetMatrix4x4("mtxMdl", ref output);
            PlaneRender.Draw(context);

            GLMaterialBlendState.Opaque.RenderBlendState();

            GL.LineWidth(2);

            context.CurrentShader.SetVector4("color", new Vector4(color, 1f));
            context.CurrentShader.SetMatrix4x4("mtxMdl", ref output);

            PlaneLinesRender.Draw(context);

            GL.LineWidth(1);
        }

        private Vector3 GetSelectedAxisVector3(int axis)
        {
            switch (axis)
            {
                case 0: return Vector3.UnitX;
                case 1: return Vector3.UnitY;
                case 2: return Vector3.UnitZ;
                default:
                    return Vector3.UnitZ;
            }
        }
    }
}
