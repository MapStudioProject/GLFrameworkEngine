using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents a shadow box used to project shadows inside the camera frustum.
    /// </summary>
    public class ShadowBox
    {
        /// <summary>
        /// The frustum planes of the shadow box.
        /// </summary>
        public Vector4[] ShadowFrustumPlanes;

        /// <summary>
        /// The unit scale of the shadow map.
        /// </summary>
        public static float UnitScale = 1.0f;

        /// <summary>
        /// The draw distance of the shadow map.
        /// </summary>
        public static float Distance = 2500;

        /// <summary>
        /// The combined shadow matrix with applied unit scale
        /// </summary>
        public Matrix4 ShadowMatrix;

        //Box min and max in local space
        private Vector3 Min;
        private Vector3 Max;

        //View/Project light space matrices
        private Matrix4 ViewMatrix;
        private Matrix4 ProjectionMatrix;

        //The offset of the box distance length
        private const float OFFSET = 10;

        //Get the light view matrix based on the camera placement in the scene and light direction
        private Matrix4 getLightViewMatrix(Camera camera, Vector3 lightDir)
        {
            var ld = lightDir.Normalized();
            var pitch = MathF.Acos(ld.Xz.Length);
            var yaw = MathF.Atan(ld.X / ld.Z);

            yaw = ld.Z > 0 ? yaw - MathF.PI : yaw;

            return Matrix4.CreateTranslation(-camera.GetViewPostion()) *
                Matrix4.CreateRotationY(-yaw) *
                Matrix4.CreateRotationX(pitch);
        }

        public void Update(Camera camera, Vector3 direction)
        {
            //Method based on https://github.com/larsjarlvik/larx/blob/48647b2a4b76daed34317cb3d0a67ce75fce7528/src/Shadows/ShadowBox.cs

            //Create the light space matrix based on the light direction and the look at eye
            ViewMatrix = getLightViewMatrix(camera, direction);
            //Create a projection matrix for extracting the camera frustrum
            var projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathF.PI / 4f, camera.AspectRatio, camera.ZNear, Math.Max(Distance, 1.1f));
            //Extract the 8 points of the camera frustum using clip space matrix
            //Get the 8 corner points of the frustum
            var points = FrustumHelper.GetFrustumCorners(FrustumHelper.ExtractFrustum(ViewMatrix * projectionMatrix));

            //Create a bounding region of the min and max points
            Min = new Vector3(float.MaxValue);
            Max = new Vector3(float.MinValue);
            foreach (Vector3 point in points)
            {
                Min.X = MathF.Min(Min.X, point.X);
                Min.Y = MathF.Min(Min.Y, point.Y);
                Min.Z = MathF.Min(Min.Z, point.Z);
                Max.X = MathF.Max(Max.X, point.X);
                Max.Y = MathF.Max(Max.Y, point.Y);
                Max.Z = MathF.Max(Max.Z, point.Z);
            }
            //Offset the z distance
            Max.Z += OFFSET;

            //Create the projection matrix to use in light space
            //This represents the boxes region in ortho view
            ProjectionMatrix = Matrix4.Identity;
            ProjectionMatrix.M11 = 2.0f / (Max.X - Min.X);
            ProjectionMatrix.M22 = 2.0f / (Max.Y - Min.Y);
            ProjectionMatrix.M33 = -2.0f / (Max.Z - Min.Z);
            ProjectionMatrix.M44 = 1.0f;

            //Scale the output based on uint scale
            Matrix4 scaleMatrix = Matrix4.CreateScale(UnitScale);
            //Combine into one final shadow matrix
            ShadowMatrix = ViewMatrix * ProjectionMatrix * scaleMatrix;

            ShadowFrustumPlanes = FrustumHelper.ExtractFrustum(ViewMatrix * ProjectionMatrix);
        }
    }
}
