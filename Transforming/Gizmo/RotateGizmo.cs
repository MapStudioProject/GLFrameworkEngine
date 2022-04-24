using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFrameworkEngine
{
    public class RotateGizmo : IGizmoRender
    {
        LineRender[] LineRenders = new LineRender[3];

        const float FLT_EPSILON = 1.192092896e-07F;

        public Vector3[] _colors = new Vector3[3] {
             new Vector3(1.0f, 0, 0), new Vector3(0, 0, 1.0f), new Vector3(0, 1.0f, 0)
        };

        public Vector4[] _rotations = new Vector4[3] {
            new Vector4(0, 1, 0, 90),  new Vector4(1, 0, 0, 90), new Vector4(0, 0, 1, 90)
        };

        //Shapes for drawing axis
        static CylinderRenderer CylinderRenderer = null;
        static Circle2DRenderer CircleRenderer = null;
        static Circle2DRenderer CircleChangeSegmentRenderer = null;
        static SphereRender SphereRender = null;

        public RotateGizmo()
        {
        }

        public TransformEngine.Axis UpdateAxisSelection(GLContext context, Vector3 position, Quaternion rotation, Vector2 mousePos, TransformSettings settings)
        {
            Matrix4 transform = Matrix4.Identity;
            if (settings.IsLocal) {
                transform = (Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(position));
            }

            var axis = TransformEngine.Axis.None;

            var gimbalSize = settings.GizmoScale * 2;
            var modelViewPos = Vector3.TransformPosition(position, context.Camera.ViewMatrix);

            bool AxisGimbalHovered(int axis)
            {
                //The axis direction. Either in unit vectors or model matrix (local space)
                Vector3 axisVec = Vector3.Zero;
                if (axis == 0) axisVec = transform.Row0.Xyz.Normalized();
                if (axis == 1) axisVec = transform.Row1.Xyz.Normalized();
                if (axis == 2) axisVec = transform.Row2.Xyz.Normalized();

                //Get the intesected mouse point from the camera and axis normal
                var intersectPos = CameraRay.GetPlaneIntersection(mousePos, context.Camera, axisVec, position);

                //No intersection. Skip it.
                if (intersectPos == Vector3.Zero)
                    return false;

                //Check the view angle from the origin and intersected position. This will prevent unwanted hits.
                var intersectViewPos = Vector3.TransformPosition(intersectPos, context.Camera.ViewMatrix);
                if (MathF.Abs(modelViewPos.Z) - MathF.Abs(intersectViewPos.Z) < -FLT_EPSILON) {
                    return false;
                }
                //Get the local position relative to the origin. Scale it by the gizmo scale to test against the circle lines
                var localPos = (intersectPos - position).Normalized() * gimbalSize;
                localPos = Vector3.TransformPosition(localPos, Matrix4.CreateTranslation(position));
                //The distance in screen coordinates from the edge and mouse coordinates
                var dist = (context.WorldToScreen(localPos) - mousePos).Length;
                if (dist < 12.0f) //pixel size check
                    return true;

                return false;
            }

            for (int i = 0; i < 3; i++)
            {
                if (AxisGimbalHovered(i))
                {
                    if (i == 0) return TransformEngine.Axis.X;
                    if (i == 1) return TransformEngine.Axis.Y;
                    else        return TransformEngine.Axis.Z;
                }
            }

            var ray = context.PointScreenRay((int)mousePos.X, (int)mousePos.Y);
            //Check if ray is within the boundary of the sphere
            if (GLMath.RayIntersectsLineSphere(ray, position, settings.GizmoScale * 2.5f, out Vector3 result2)) 
                return TransformEngine.Axis.All;
            

            return axis;
        }

        void Init()
        {
            CylinderRenderer = new CylinderRenderer(0.04f, 2);
            CircleRenderer = new Circle2DRenderer(64);
            CircleChangeSegmentRenderer = new Circle2DRenderer(32, PrimitiveType.TriangleFan);
            SphereRender = new SphereRender(1, 32, 32);
        }

        public void Render(GLContext context, Vector3 position, Quaternion rotation, float scale, bool isMoving, bool[] isSelected, bool[] isHovered)
        {
            if (CylinderRenderer == null)
                Init();

            var shader = GlobalShaders.GetShader("GIZMO");
            context.CurrentShader = shader;

            GL.Disable(EnableCap.DepthTest);

            //Ignore the previous depth from the current drawn buffer.
            //This is to prevent depth from clipping in models.
            GL.Clear(ClearBufferMask.DepthBufferBit);

            var translateMtx = Matrix4.CreateTranslation(position);
            var scaleMtx = Matrix4.CreateScale(scale);
            var rotationMtx = Matrix4.CreateFromQuaternion(rotation);
            var transform = scaleMtx * rotationMtx * translateMtx;

            bool displayGizmo = true;
            //No display for multi axis
            if (context.TransformTools.TransformSettings.IsMultiAxis())
                return;

            if (displayGizmo)
            {
                GL.Enable(EnableCap.LineSmooth);
                GL.LineWidth(5);
                GL.PushAttrib(AttribMask.DepthBufferBit);

                transform = scaleMtx * translateMtx;

                if (!isMoving)
                    DrawOrb(context, ref transform, isMoving, isHovered[6]);

                transform = scaleMtx * rotationMtx * translateMtx;

                for (int i = 0; i < 3; i++)
                {
                    if (isMoving && !isSelected[i])
                        continue;

                    DrawAxis(context, i, isMoving && isSelected[i], isHovered[i], ref transform, _rotations[i], _colors[i]);
                }

                GL.PopAttrib();

                if (!isMoving && !isSelected.Any(x => x))
                    GizmoCenterRender.Draw(context, position, scale, new Vector4(1));
            }
            GL.LineWidth(1);

            context.CurrentShader = null;
            GL.Enable(EnableCap.DepthTest);
        }

        public void DrawOrb(GLContext context, ref Matrix4 transform, bool isMoving, bool isHovered)
        {
            var matrix = Matrix4.CreateScale(2) * new Matrix4(context.Camera.InverseRotationMatrix) * transform;
            var matrix2 = Matrix4.CreateScale(2.5f) * new Matrix4(context.Camera.InverseRotationMatrix) * transform;

            context.CurrentShader.SetMatrix4x4("mtxMdl", ref matrix);
            context.CurrentShader.SetVector4("selectionColor", Vector4.Zero);

            GL.Enable(EnableCap.DepthTest);

            GLMaterialBlendState.Translucent.RenderBlendState();
            context.CurrentShader.SetVector4("color", new Vector4(0.7f, 0.7f, 0.7f, 0.15f));
            SphereRender.Draw(context);

            GLMaterialBlendState.Opaque.RenderBlendState();

            GL.Disable(EnableCap.DepthTest);

            context.CurrentShader.SetVector4("color", new Vector4(0.4f, 0.4f, 0.4f, 1));
            CircleRenderer.Draw(context);

            context.CurrentShader.SetMatrix4x4("mtxMdl", ref matrix2);

            context.CurrentShader.SetVector4("color", new Vector4(1, 1, 1, 1));
            if (isHovered)
                context.CurrentShader.SetVector4("color", new Vector4(1, 1, 0, 1));
            CircleRenderer.Draw(context);

            GL.Enable(EnableCap.DepthTest);
        }

        public void DrawAxis(GLContext context, int index, bool isMoving, bool isSelected, ref Matrix4 transform, Vector4 rotation, Vector3 color)
        {
            var rotationMatrix = Matrix4.CreateFromAxisAngle(rotation.Xyz, MathHelper.DegreesToRadians(rotation.W));
            var scaleMatrix = Matrix4.CreateScale(2);
            var matrix = scaleMatrix * rotationMatrix * transform;

            context.CurrentShader.SetMatrix4x4("mtxMdl", ref matrix);
            context.CurrentShader.SetVector4("color", new Vector4(color, 1.0f));
            context.CurrentShader.SetVector4("selectionColor", Vector4.Zero);

            //Draw a filled circle if in a moving action
            if (isMoving)
            {
                GL.Disable(EnableCap.DepthTest);
                CircleRenderer.Draw(context);

                var angle = context.TransformTools.TransformSettings.RotationAngle;
                var angleStartVec = context.TransformTools.TransformSettings.RotationStartVector;
                var axis = context.TransformTools.TransformSettings.RotationStartAxis;

                transform = transform.ClearRotation();
                matrix = scaleMatrix * transform;

                context.CurrentShader.SetMatrix4x4("mtxMdl", ref matrix);

                context.CurrentShader.SetVector4("color", new Vector4(1, 0, 0, 0.6f));
                CircleChangeSegmentRenderer.SetCustomSegment(angleStartVec, axis, angle, 32);

                GLMaterialBlendState.TranslucentAlphaOne.RenderBlendState();

                GL.Disable(EnableCap.CullFace);
                CircleChangeSegmentRenderer.Draw(context);
                GL.Enable(EnableCap.CullFace);

                GLMaterialBlendState.Opaque.RenderBlendState();
            }
            else
            {
                if (isSelected)
                    context.CurrentShader.SetVector4("selectionColor", new Vector4(1, 1, 0, 1));

                CircleRenderer.Draw(context);
            }
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
