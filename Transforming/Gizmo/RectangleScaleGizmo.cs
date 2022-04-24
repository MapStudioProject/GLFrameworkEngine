using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFrameworkEngine
{
    public class RectangleScaleGizmo : IGizmoRender
    {
        public RectangleScaleGizmo()
        {
        }

        static SphereRender SphereRender = null;
        static ConeRenderer ConeRenderer = null;
        static LineRender LineRender = null;

        static BoundingBox[] Boundings = new BoundingBox[6];

        static readonly Vector4[] _rotations = new Vector4[6] {
            new Vector4(1, 0, 0, -90), //Top
            new Vector4(1, 0, 0, 90), //Bottom
            new Vector4(0, 0, 1, 90), //Right
            new Vector4(0, 0, 1, -90), //Left
            new Vector4(0, 0, 1, 0), //Front
            new Vector4(0, 0, 1, -180), //Back
        };

        void Init()
        {
            SphereRender = new SphereRender(0.25f);
            ConeRenderer = new ConeRenderer(0.5f, 0, 1);
            LineRender = new LineRender(PrimitiveType.LineLoop);
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

            //Find selected axis
            for (int i = 0; i < Boundings.Length; i++)
            {
                if (Boundings[i] == null)
                    continue;

                if (GLMath.RayIntersectsAABB(localRay,
                    Boundings[i].Min,
                    Boundings[i].Max, out float dist))
                {
                    if (i == 0) return TransformEngine.Axis.X; //Right
                    if (i == 1) return TransformEngine.Axis.Y; //Top
                    if (i == 2) return TransformEngine.Axis.Z; //Front
                    if (i == 3) return TransformEngine.Axis.XN; //Left
                    if (i == 4) return TransformEngine.Axis.YN; //Bottom
                    if (i == 5) return TransformEngine.Axis.ZN; //Back
                }
            }
            return axis;
        }

        public void Render(GLContext context, BoundingBox box, Vector3 position, Quaternion rotation, float scale, bool isMoving, bool[] isSelected, bool[] isHovered)
        {
            if (ConeRenderer == null)
                Init();

            GL.Disable(EnableCap.DepthTest);

            var translationMtx = Matrix4.CreateTranslation(position);
            var scaleMtx = Matrix4.CreateScale(scale);
            var rotationMtx = Matrix4.CreateFromQuaternion(rotation);
            var transform = rotationMtx * translationMtx;

            DrawLines(context, transform, box);

            //Place arrows in all 6 spots of box
            Vector3[] faceOrigins = new Vector3[6];
            for (int i = 0; i < 6; i++)
                faceOrigins[i] = box.GetFaceOrigin(i);

            for (int i = 0; i < 6; i++) {
                Boundings[i] = new BoundingBox(faceOrigins[i] + new Vector3(-(scale * 0.25f)), faceOrigins[i] + new Vector3(scale * 0.25f));
            }

            var shader = GlobalShaders.GetShader("GIZMO");
            context.CurrentShader = shader;

            for (int i = 0; i < faceOrigins.Length; i++)
                DrawAxisArrow(context, scaleMtx, transform, isHovered[i], _rotations[i], faceOrigins[i]);
            context.CurrentShader = null;
            GL.Enable(EnableCap.DepthTest);
        }

        //Draws lines in the center of the edges of the bounding rectangle
        private void DrawLines(GLContext context, Matrix4 transform, BoundingBox box)
        {
            var mat = new DashMaterial();
            mat.MatrixCamera = context.Camera.ViewProjectionMatrix;
            mat.ModelMatrix = transform.ClearScale();
            mat.Render(context);

            var top = box.GetQuadFace(1);
            var bottom = box.GetQuadFace(4);
            var right = box.GetQuadFace(0);
            var left = box.GetQuadFace(3);

            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < 4; i++)
            points.Add(bottom[i] + (top[i] - bottom[i]) * new Vector3(1, 0.5f, 1));

            GL.LineWidth(2);
            LineRender.Draw(points, true);

            points.Clear();
            for (int i = 0; i < 4; i++)
                points.Add(left[i] + (right[i] - left[i]) * new Vector3(0.5f, 1, 1));

            GL.LineWidth(2);
            LineRender.Draw(points, true);

            GL.LineWidth(1);
        }

        private void DrawAxisArrow(GLContext context, Matrix4 scaleMtx, Matrix4 transform, bool hovered, Vector4 rotation, Vector3 point)
        {
            var rotate = Matrix4.CreateFromAxisAngle(rotation.Xyz, MathHelper.DegreesToRadians(rotation.W));
            var rotate90 = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(90));
            var output = rotate * rotate90 * scaleMtx * Matrix4.CreateTranslation(point) * transform;

            context.CurrentShader.SetVector4("color", new Vector4(1, 1, 1, 1.0f));
            if (hovered)
                context.CurrentShader.SetVector4("color", new Vector4(1, 1, 0, 1.0f));

            context.CurrentShader.SetMatrix4x4("mtxMdl", ref output);

            SphereRender.Draw(context);
        }
    }
}
