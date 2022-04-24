using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace GLFrameworkEngine
{
    public class BillboardMaterial
    {
        /// <summary>
        /// The color of the material output.
        /// </summary>
        public Vector4 Color = new Vector4(1.0f);

        /// <summary>
        /// The scale of the model.
        /// </summary>
        public float Scale = 0.38f;

        /// <summary>
        /// The model matrix of the model.
        /// </summary>
        public Matrix4 ModelMatrix = Matrix4.Identity;

        /// <summary>
        /// Determines to scale the model based on the distance the camera is from the model matrix.
        /// </summary>
        public bool ScaleByCameraDistance = false;

        /// <summary>
        /// Displays the material with a selection color.
        /// </summary>
        public bool DisplaySelection = false;

        /// <summary>
        /// The sprite texture ID of the billboarded material.
        /// </summary>
        public int TextureID = -1;

        public float CameraScale = 1.0f;

        public void Render(GLContext context)
        {
            var mtxView = context.Camera.ViewMatrix;
            var mtxProj = context.Camera.ProjectionMatrix;

            CameraScale = !ScaleByCameraDistance ? 1.0f : context.Camera.ScaleByCameraDistance(ModelMatrix.ExtractTranslation(), 0.04f);

            context.CurrentShader = GlobalShaders.GetShader("BILLBOARD");
            context.CurrentShader.SetVector4("color", Color);
            context.CurrentShader.SetBoolToInt("useTexture", TextureID != -1);
            context.CurrentShader.SetMatrix4x4("mtxMdl", ref ModelMatrix);
            context.CurrentShader.SetMatrix4x4("mtxView", ref mtxView);
            context.CurrentShader.SetMatrix4x4("mtxProj", ref mtxProj);
            context.CurrentShader.SetVector3("scale", new Vector3(Scale * CameraScale));
            context.CurrentShader.SetVector4(GLConstants.SelectionColorUniform, Vector4.Zero);

            if (TextureID != -1)
                context.CurrentShader.SetTexture2D("textureMap", TextureID, 1);
            if (DisplaySelection) {
                context.CurrentShader.SetVector4(GLConstants.SelectionColorUniform, GLConstants.SelectColor);
            }
        }
    }
}
