using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFrameworkEngine
{
    public class StandardMaterial
    {
        public bool DisplaySelection = false;

        public bool IsSRGB = false;

        public bool DirectionalLighting = true;

        public Vector4 Color = Vector4.One;

        public bool HalfLambertShading = false;
        public bool hasVertexColors = false;
        public bool displayOnlyVertexColors = false;

        public Matrix4 ModelMatrix = Matrix4.Identity;
        public Matrix4 CameraMatrix
        {
            get { return _cameraMatrix; }
            set { _cameraMatrix = value; }
        }

        public int DiffuseTextureID = -1;

        private Matrix4 _cameraMatrix = Matrix4.Zero;

        public void Render(GLContext context)
        {
            //Check for a custom set camera matrix. If none is set, apply the given context one
            Matrix4 mtxCam = CameraMatrix;
            if (_cameraMatrix == Matrix4.Zero)
                mtxCam = context.Camera.ViewProjectionMatrix;

            context.CurrentShader = GlobalShaders.GetShader("BASIC");
            context.CurrentShader.SetVector4("color", Color);
            context.CurrentShader.SetBoolToInt("halfLambert", HalfLambertShading);
            context.CurrentShader.SetBoolToInt("hasVertexColors", hasVertexColors);
            context.CurrentShader.SetBoolToInt("displayVertexColors", displayOnlyVertexColors);
            context.CurrentShader.SetMatrix4x4(GLConstants.ModelMatrix, ref ModelMatrix);
            context.CurrentShader.SetMatrix4x4(GLConstants.ViewProjMatrix, ref mtxCam);
            context.CurrentShader.SetVector4("highlight_color", Vector4.Zero);
            context.CurrentShader.SetInt("hasTextures", 0);
            context.CurrentShader.SetBoolToInt("isSRGB", IsSRGB);
            context.CurrentShader.SetBoolToInt("directionalLighting", DirectionalLighting);

            if (DisplaySelection)
                context.CurrentShader.SetVector4("highlight_color", new Vector4(GLConstants.SelectColor.Xyz, 0.5f));

            if (HalfLambertShading)
            {
                Vector3 dir = Vector3.TransformNormal(new Vector3(0f, 0f, -1f), context.Camera.ViewProjectionMatrix.Inverted()).Normalized();

                context.CurrentShader.SetVector3("difLightDirection", dir);
            }

            if (DiffuseTextureID != -1)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + 1);
                GL.BindTexture(TextureTarget.Texture2D, DiffuseTextureID);
                context.CurrentShader.SetInt("textureMap", 1);
                context.CurrentShader.SetInt("hasTextures", 1);
            }
        }
    }
}
