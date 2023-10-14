using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    /// <summary>
    /// A cubic render of the camera fustrum using the view and projection matrix data
    /// </summary>
    public class CameraRenderer : RenderMesh<Vector3>
    {
        public Camera Camera { get; set; }

        public Matrix4 Transform = Matrix4.Identity;

        public CameraRenderer() :
            base(GetVertices(CreateDefault()), Indices, PrimitiveType.Lines)
        {

        }

        public CameraRenderer(Camera camera) :
            base(GetVertices(camera), Indices, PrimitiveType.Lines)
        {
            Camera = camera;
        }

        public void Update(Camera camera) {
            Camera = camera;
            this.UpdateVertexData(GetVertices(camera));
        }

        static Camera CreateDefault()
        {
            Camera Camera = new Camera();
            Camera.TargetPosition = new Vector3();
            Camera.RotationX = 0;
            Camera.RotationY = 0;
            Camera.ZNear = 1.0f;
            Camera.ZFar = 100.0f;
            Camera.Width = 100;
            Camera.Height = 50;
            Camera.UpdateMatrices();
            return Camera;
        }

        public static Vector3[] GetVertices(Camera camera)
        {
            float farPlane = MathF.Min(camera.ZFar, 100);
            float nearPlane = camera.ZNear;
            float tan = (float)Math.Tan(camera.Fov / 2);
            float aspect = camera.AspectRatio;

            float nearHeight = nearPlane * tan;
            float nearWidth = nearHeight * aspect;
            float farHeight = farPlane * tan;
            float farWidth = farHeight * aspect;

            Vector3[] vertices = new Vector3[8];
            // near bottom left
            vertices[2][0] = -nearWidth; vertices[2][1] = -nearHeight; vertices[2][2] = -nearPlane;
            // near bottom right
            vertices[3][0] = nearWidth; vertices[3][1] = -nearHeight; vertices[3][2] = -nearPlane;

            // near top left
            vertices[1][0] = -nearWidth; vertices[1][1] = nearHeight; vertices[1][2] = -nearPlane;
            // near top right
            vertices[0][0] = nearWidth; vertices[0][1] = nearHeight; vertices[0][2] = -nearPlane;

            // far bottom left
            vertices[6][0] = -farWidth; vertices[6][1] = -farHeight; vertices[6][2] = -farPlane;
            // far bottom right
            vertices[7][0] = farWidth; vertices[7][1] = -farHeight; vertices[7][2] = -farPlane;

            // far top left
            vertices[4][0] = -farWidth; vertices[5][1] = farHeight; vertices[5][2] = -farPlane;
            // far top right
            vertices[5][0] = farWidth; vertices[4][1] = farHeight; vertices[4][2] = -farPlane;
            return vertices;
        }

        public static int[] Indices = new int[]
        {
            0, 1, 2, 3, //Bottom & Top
            4, 5, 6, 7, //Bottom & Top -Z
            0, 2, 1, 3, //Bottom to Top
            4, 6, 5, 7, //Bottom to Top -Z
            0, 4, 6, 2, //Bottom Z to -Z
            1, 5,  3, 7 //Top Z to -Z
        };
    }
}
