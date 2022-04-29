using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core;

namespace GLFrameworkEngine
{
    public class DrawableInfiniteFloor
    {
        /// <summary>
        /// Determines to use a solid or grid based floor.
        /// </summary>
        public static bool IsSolid = false;

        /// <summary>
        /// Determines to use a spotlight lighting on the floor (solid only).
        /// </summary>
        public static bool IsSpotlight = false;

        PlaneRenderer PlaneRender;

        public void Draw(GLContext control)
        {
            if (!DrawableGridFloor.Display)
                return;

            if (PlaneRender == null) {
                PlaneRender = new PlaneRenderer();
            }

            var gridShaderProgram = GlobalShaders.GetShader("GRID_INFINITE");
            control.CurrentShader = gridShaderProgram;

            var mtxView = control.Camera.ViewMatrix;
            var mtxProj = control.Camera.ProjectionMatrix;
            gridShaderProgram.SetMatrix4x4("mtxView", ref mtxView);
            gridShaderProgram.SetMatrix4x4("mtxProj", ref mtxProj);
            gridShaderProgram.SetFloat("znear", 0.01f);
            gridShaderProgram.SetFloat("zfar", 10);
            gridShaderProgram.SetBoolToInt("solidFloor", IsSolid);
            gridShaderProgram.SetBoolToInt("spotLight", IsSpotlight);
            gridShaderProgram.SetVector3("gridColor", new Vector3(
                DrawableGridFloor.GridColor.X,
                DrawableGridFloor.GridColor.Y,
                DrawableGridFloor.GridColor.Z));

            //Make sure to support transparency as the grid is a drawn quad with alpha used
            GLMaterialBlendState.Translucent.RenderBlendState();

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            PlaneRender.Draw(control);

            GLMaterialBlendState.Opaque.RenderBlendState();
        }
    }
}