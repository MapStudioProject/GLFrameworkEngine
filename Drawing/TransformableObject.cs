using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core.ViewModels;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents a default transformable object with a standard cube drawn.
    /// </summary>
    public class TransformableObject : EditableObject, IColorPickable, IFrustumCulling
    {
        public Vector4 Color = new Vector4(1, 1, 1, 1);

        AxisLines AxisObject = null;
        UVCubeRenderer CubeRenderer = null;
        StandardMaterial Material = new StandardMaterial();

        //2D
        SphereRender Sphere2DRenderer = null;
        StandardMaterial Material2D = new StandardMaterial();
        public Vector4 Color2D = new Vector4(1, 1, 1, 1);
        public float MinScale2D = 1;

        public bool EnableFrustumCulling => true;
        public bool InFrustum { get; set; }

        public bool DrawCube = true;

        private float Scale = 1.0f;

        public BoundingNode Boundings = new BoundingNode()
        {
            Center = new Vector3(0, 0, 0),
            Box = new BoundingBox(new Vector3(-10), new Vector3(10)),
        };

        public bool IsInsideFrustum(GLContext context) {
            return context.Camera.InFustrum(Boundings);
        }

        public TransformableObject(NodeBase parent, float scale = 1.0f) : base(parent)
        {
            Boundings.Box.Min *= scale;
            Boundings.Box.Max *= scale;

            Scale = scale;
            //Update boundings on transform changed
            this.Transform.TransformUpdated += delegate {
                Boundings.UpdateTransform(this.Transform.TransformMatrix);
            };
            UINode.Tag = this;
        }

        public virtual void DrawColorPicking(GLContext context)
        {
            if (context.Camera.Is2D)
            {
                DrawColorPicking2D(context);
                return;
            }

            Prepare();

            CubeRenderer.DrawPicking(context, this, Transform.TransformMatrix);
        }

        public override void DrawModel(GLContext context, Pass pass)
        {
            if (pass != Pass.OPAQUE || !this.InFrustum)
                return;

            base.DrawModel(context, pass);

            Prepare();
            if (DrawCube)
            {
                Material.DiffuseTextureID = RenderTools.TexturedCubeTex.ID;
                Material.DisplaySelection = IsSelected | IsHovered;
                Material.Color = this.Color;
                Material.HalfLambertShading = false;
                Material.ModelMatrix = Transform.TransformMatrix;
                Material.Render(context);

                CubeRenderer.DrawWithSelection(context, IsSelected || IsHovered);
            }
            else //axis line drawer
            {
                bool sel = IsSelected || IsHovered;

                Boundings.Box.DrawSolid(context, Matrix4.Identity, sel ? GLConstants.SelectColor : Vector4.One);

                AxisObject.Transform = this.Transform;
                AxisObject.DrawModel(context, Pass.OPAQUE);
            }
        }

        public virtual void DrawColorPicking2D(GLContext context)
        {
            if (Sphere2DRenderer == null)
                Sphere2DRenderer = new SphereRender(10);

            float scale = context.Camera.ScaleByCameraDistance(this.Transform.Position);
            scale = MathF.Max(scale, MinScale2D);

            var mat = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(this.Transform.Position);

            Sphere2DRenderer.DrawPicking(context, this, mat);
        }

        public virtual void DrawModel2D(GLContext context)
        {
            if (!CanSelect)
                return;

            if (Sphere2DRenderer == null)
                Sphere2DRenderer = new SphereRender(10);

            float scale = context.Camera.ScaleByCameraDistance(this.Transform.Position);
            scale = MathF.Max(scale, MinScale2D);
            var color = IsSelected ? GLConstants.SelectColor : this.Color2D;
            color.W = 1.0f;

            GL.Disable(EnableCap.DepthTest);

            Material2D.Color = color;
            Material2D.ModelMatrix = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(this.Transform.Position);
            Material2D.Render(context);
            Sphere2DRenderer.Draw(context);

            GL.Enable(EnableCap.DepthTest);
        }

        private void Prepare()
        {
            if (CubeRenderer == null || CubeRenderer.IsDisposed)
                CubeRenderer = new UVCubeRenderer(10 * Scale);
            if (AxisObject == null && !DrawCube) {
                AxisObject = new AxisLines(60 * Scale);
            }
        }

        public override void Dispose()
        {
            CubeRenderer?.Dispose();
        }
    }
}
