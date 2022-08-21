using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents a billboarded sprite renderer.
    /// </summary>
    public class SpriteDrawer : Plane2DRenderer
    {
        /// <summary>
        /// The transform matrix of the drawable.
        /// </summary>
        public GLTransform Transform = new GLTransform();

        /// <summary>
        /// The material of the cursor.
        /// </summary>
        private readonly BillboardMaterial Material = new BillboardMaterial();

        /// <summary>
        /// Toggles visibility of the cursor.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Determines if the sprite is selected or not.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// The texture ID of the sprite.
        /// </summary>
        public int TextureID = -1;

        public bool XRay = true;

        //Default texture to draw as.
        static GLTexture DefaultTexture = null;

        public SpriteDrawer(float size = 2.0f) : base(size)
        {

        }

        public BoundingNode GetRayBounding()
        {
            return new BoundingNode() {
                Radius = 0.5F * Material.CameraScale,
            };
        }

        public void DrawModel(GLContext context)
        {
            if (DefaultTexture == null) {
                DefaultTexture = GLTexture2D.FromBitmap(Properties.Resources.ObjectSprite);
            }

            Material.TextureID = TextureID == -1 ? DefaultTexture.ID : TextureID;
            Material.ModelMatrix = Transform.TransformMatrix;
            Material.ScaleByCameraDistance = true;
            Material.DisplaySelection = IsSelected;
            Material.Render(context);

            if (XRay)
                GL.Disable(EnableCap.DepthTest);

            GL.DepthMask(false);
            GL.LineWidth(1.5f);

            GLMaterialBlendState.Translucent.RenderBlendState();

            Draw(context);

            GLMaterialBlendState.Opaque.RenderBlendState();

            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);

            GL.LineWidth(1);
        }
    }
}
