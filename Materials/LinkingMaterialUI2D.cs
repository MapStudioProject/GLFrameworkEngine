using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace GLFrameworkEngine
{
    public class LinkingMaterialUI2D
    {
        public Vector4 Color = new Vector4(1.0f);

        public Matrix4 ModelMatrix = Matrix4.Identity;
        public Matrix4 MatrixCamera = Matrix4.Identity;

        public float Time;

        public void Render(GLContext context)
        {
            context.CurrentShader = GlobalShaders.GetShader("LINKING");
            context.CurrentShader.SetMatrix4x4("matVP", ref MatrixCamera);
            context.CurrentShader.SetMatrix4x4("matGeo", ref ModelMatrix);
            context.CurrentShader.SetFloat("Time", Time);
        }
    }
}
