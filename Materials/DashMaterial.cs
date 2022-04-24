using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace GLFrameworkEngine
{
    public class DashMaterial
    {
        public Vector4 Color = new Vector4(1.0f);

        public Matrix4 ModelMatrix = Matrix4.Identity;
        public Matrix4 MatrixCamera = Matrix4.Identity;

        public float DashFactor = 5.0f;
        public float DashWidth = 5.0f;

        public void Render(GLContext context)
        {
            context.CurrentShader = GlobalShaders.GetShader("LINE_DASHED");
            context.CurrentShader.SetVector4("color", Color);
            context.CurrentShader.SetMatrix4x4("mtxCam", ref MatrixCamera);
            context.CurrentShader.SetMatrix4x4("mtxMdl", ref ModelMatrix);

            context.CurrentShader.SetVector2("viewport_size", new Vector2(context.Width, context.Height));
            context.CurrentShader.SetFloat("dash_factor", DashFactor);
            context.CurrentShader.SetFloat("dash_width", DashWidth);
        }
    }
}
