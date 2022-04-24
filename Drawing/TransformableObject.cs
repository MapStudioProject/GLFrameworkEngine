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

        public bool EnableFrustumCulling => true;
        public bool InFrustum { get; set; }

        public bool DrawCube = true;

        public BoundingNode Boundings = new BoundingNode()
        {
            Center = new Vector3(0, 0, 0),
            Box = new BoundingBox(new Vector3(-10), new Vector3(10)),
        };

        public bool IsInsideFrustum(GLContext context) {
            return context.Camera.InFustrum(Boundings);
        }

        public TransformableObject(NodeBase parent) : base(parent)
        {
            //Update boundings on transform changed
            this.Transform.TransformUpdated += delegate {
                Boundings.UpdateTransform(this.Transform.TransformMatrix);
            };
            UINode.Tag = this;
        }

        public void DrawColorPicking(GLContext context)
        {
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

        private void Prepare()
        {
            if (CubeRenderer == null || CubeRenderer.IsDisposed)
                CubeRenderer = new UVCubeRenderer(10);
            if (AxisObject == null && !DrawCube) {
                AxisObject = new AxisLines(60);
            }
        }

        public override void Dispose()
        {
            CubeRenderer?.Dispose();
        }
    }
}
