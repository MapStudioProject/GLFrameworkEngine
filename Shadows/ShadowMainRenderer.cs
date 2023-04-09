using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Renders and handles shadow maps in projected space from the camera frustum.
    /// </summary>
    public class ShadowMainRenderer
    {
        /// <summary>
        /// Determines to display shadows or not.
        /// </summary>
        public static bool Display = false;

        //Draws a debug quad for displaying the shadow map
        public static bool DEBUG_QUAD = false;

        //The width and height of the projection texture
        static int WIDTH = 2048;
        static int HEIGHT = 2048;

        //Shadow box for frustum shadow handling
        private ShadowBox shadowBox;

        //Shadow framebuffer for drawing the shadow texture.
        private ShadowFrameBuffer ShadowFrameBuffer;
        //Light direction for shadow direction
        private Vector3 lightDir;

        public ShadowMainRenderer() {
            shadowBox = new ShadowBox();
            ShadowPrepassTexture = GLTexture2D.CreateWhiteTexture(4,4);
        }

        /// <summary>
        /// Gets the frustum planes used to check if a shadow mesh should draw or not.
        /// </summary>
        /// <returns></returns>
        public Vector4[] GetShadowFrustum() => shadowBox.ShadowFrustumPlanes;

        /// <summary>
        /// Gets the projected cascaded shadow map generated from the shadow renderer.
        /// </summary>
        /// <returns></returns>
        public DepthTexture GetProjectedShadow() => ShadowFrameBuffer?.GetShadowTexture();

        /// <summary>
        /// Gets the light space shadow matrix used to project shadows into.
        /// </summary>
        /// <returns></returns>
        public Matrix4 GetLightSpaceViewProjMatrix() => shadowBox.ShadowMatrix;

        /// <summary>
        /// Gets the light direction used for directional shadows.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetLightDirection() => lightDir;

        public GLTexture2D ShadowPrepassTexture { get; set; }

        public void Render(GLContext context, Vector3 lightDirection)
        {
            if (ShadowFrameBuffer == null)
                ShadowFrameBuffer = new ShadowFrameBuffer(WIDTH, HEIGHT);

            //Reset draw call stats
            ResourceTracker.NumShadowDrawCalls = 0;

            //Increase the default light direction to be used like a sun object
            lightDirection = lightDirection * new Vector3(1000, 1000, 1000);
            //Store the light direction as it is necessary for shadow calculations
            lightDir = lightDirection;

            //Update the light space matrix based on camera frustum
            shadowBox.Update(context.Camera, lightDirection);

            Start();
            {
                //Start to draw shadows projected onto a texture using the light view projection matrix
                var shader = GlobalShaders.GetShader("SHADOW");
                context.CurrentShader = shader;

                var lightSpaceMatrix = GetLightSpaceViewProjMatrix();
                shader.SetMatrix4x4("mtxLightVP", ref lightSpaceMatrix);

                //Poylgon offsets can slightly improve shadow quality
                GL.PolygonOffset(0.15f, 100);
                GL.ClearColor(1, 1, 1, 1);

                foreach (var obj in context.Scene.Objects)
                {
                    if (obj is GenericRenderer)
                    {
                        //Assign the model space of each render in the shadow shader
                        var mtxMdl = ((GenericRenderer)obj).Transform.TransformMatrix;
                        shader.SetMatrix4x4("mtxMdl", ref mtxMdl);
                        //Draw the shadow model pass
                        ((GenericRenderer)obj).DrawShadowModel(context);
                    }
                }
                //Reset back to normal
                GL.PolygonOffset(0, 0);
                context.CurrentShader = null;
            }
            Finish();
        }

        public void ResetShadowPrepass()
        {
            ShadowPrepassTexture?.Dispose();
            ShadowPrepassTexture = GLTexture2D.CreateWhiteTexture(4, 4);
        }

        public void DrawShadowPrepass(GLContext context, GLTexture2D gbuffer, DepthTexture depth)
        {
            if (ShadowPrepassTexture == null)
            {
                ShadowPrepassTexture = GLTexture2D.CreateUncompressedTexture(context.Width, context.Height,
                     PixelInternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte);
            }
            ShadowPrepassManager.CreateShadowPrepassTexture(context,
                 context.Scene.ShadowRenderer.GetProjectedShadow(),
                 gbuffer, depth, ShadowPrepassTexture);
        }

        /// <summary>
        /// Draws a debug quad for shadow pre pass
        /// </summary>
        public void DrawDebugShadowPrePass(GLContext control)
        {
            if (!DEBUG_QUAD)
                return;

            var shader = GlobalShaders.GetShader("SCREEN");
            control.CurrentShader = shader;

            //Bottom right corner, half the viewport size.
            GL.Viewport(control.Width / 2, 0, control.Width / 2, control.Height / 2);

            shader.SetTexture(ShadowPrepassTexture, "screenTexture", 1);
            ScreenQuadRender.Draw();

            GL.Viewport(0, 0, control.Width, control.Height);
        }

        /// <summary>
        /// Draws a debug quad for shadow projection map
        /// </summary>
        public void DrawDebugQuad(GLContext control)
        {
            if (!DEBUG_QUAD)
                return;

            //Bottom left corner, half the viewport size.
            GL.Viewport(0, 0, control.Width / 2, control.Height / 2);

            var shader = GlobalShaders.GetShader("SHADOWQUAD");
            control.CurrentShader = shader;

            shader.SetTexture(GetProjectedShadow(), "depthTexture", 1);
            ScreenQuadRender.Draw();

            GL.Viewport(0, 0, control.Width, control.Height);
        }

        private void Start()
        {
            GL.Enable(EnableCap.DepthTest);

            GL.Viewport(0, 0, WIDTH, HEIGHT);
            ShadowFrameBuffer.Bind();
            GL.Clear(ClearBufferMask.DepthBufferBit);
        }

        private void Finish() {
            ShadowFrameBuffer.Unbind();
        }
    }
}
