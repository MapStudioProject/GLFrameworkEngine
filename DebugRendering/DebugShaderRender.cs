using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class DebugShaderRender
    {
        /// <summary>
        /// Toggles debug shader rendering in 3D view.
        /// </summary>
        public static DebugRender DebugRendering = DebugRender.Default;

        public enum DebugRender
        {
            Default,
            Normal,
            Lighting,
            Diffuse,
            VertexColors,
            UVCoords,
            UVTestPattern,
            Weights,
            Tangents,
            Bitangents,

            Specular,
            NormalMap,
            Emission,
            AO,
            Shadow,
            Lightmap,
            LightmapAlpha,
        }

        public static void RenderMaterial(GLContext context)
        {
            var debugShader = context.CurrentShader;
            debugShader.SetInt("debugShading", (int)DebugShaderRender.DebugRendering);
            debugShader.SetInt("weightRampType", 2);
            debugShader.SetInt("selectedBoneIndex", Runtime.SelectedBoneIndex);

            int slot = 10;
            debugShader.SetTexture(RenderTools.uvTestPattern, "UVTestPattern", slot++);
            debugShader.SetTexture(RenderTools.boneWeightGradient, "weightRamp1", slot++);
            debugShader.SetTexture(RenderTools.boneWeightGradient2, "weightRamp2", slot++);

            var viewProjMatInv = context.Camera.ViewProjectionMatrix.Inverted();
            debugShader.SetMatrix4x4("mtxViewProjInv", ref viewProjMatInv);

            if (context.Scene.ShadowRenderer != null)
            {
                var lightVP = context.Scene.ShadowRenderer.GetLightSpaceViewProjMatrix();
                var shadowMap = context.Scene.ShadowRenderer.GetProjectedShadow();
                var lightDir = context.Scene.ShadowRenderer.GetLightDirection();

                debugShader.SetMatrix4x4("mtxLightVP", ref lightVP);
                debugShader.SetVector3("lightDir", lightDir);
                debugShader.SetTexture(shadowMap, "shadowMap", slot++);
            }
        }
    }
}
