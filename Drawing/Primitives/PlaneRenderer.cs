using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class PlaneRenderer : RenderMesh<VertexPositionNormalTexCoord>
    {
        public PlaneRenderer(float size = 1.0f, PrimitiveType type = PrimitiveType.Triangles) : base(DrawingHelper.GetPlaneVertices(size), Indices, type)
        {

        }

        public void DrawBillboardSprite(GLContext context, int textureID, Matrix4 transform)
        {
            var shader = GlobalShaders.GetShader("BILLBOARD");
            context.CurrentShader = shader;

            GL.ActiveTexture(TextureUnit.Texture0 + 1);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            shader.SetInt("textureMap", 1);
            shader.SetInt("useTexture", 1);

            var viewMatrix = context.Camera.ViewMatrix;
            var projMatrix = context.Camera.ProjectionMatrix;

            shader.SetVector3("scale", transform.ExtractScale());
            shader.SetMatrix4x4(GLConstants.ModelMatrix, ref transform);
            shader.SetMatrix4x4(GLConstants.ViewMatrix, ref viewMatrix);
            shader.SetMatrix4x4(GLConstants.ProjMatrix, ref projMatrix);

            //Make sprites use translucent materials
          //  GLMaterialBlendState.Translucent.RenderBlendState();

            Draw(context);

           // GLMaterialBlendState.Opaque.RenderBlendState();
        }

        public static int[] Indices = new int[]
        {
            0, 1, 2, 2, 3, 0,
        };
    }
}
