using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace GLFrameworkEngine
{
    public static class Matrix3Extension
    {
        public static Matrix3 Mat3FromEulerAnglesDeg(Vector3 eulerAngles) =>
           CreateRotationX(Math.PI * eulerAngles.X / 180.0) *
           CreateRotationY(Math.PI * eulerAngles.Y / 180.0) *
           CreateRotationZ(Math.PI * eulerAngles.Z / 180.0) *
               Matrix3.Identity;

        public static Matrix3 FromEulerAngles(Vector3 eulerAngles)
        {
         return CreateRotationX(eulerAngles.X) *
                CreateRotationY(eulerAngles.Y) *
                CreateRotationZ(eulerAngles.Z) *
                  Matrix3.Identity;
        }

        public static Vector3 ExtractEulerAngles(this Matrix3 mtx)
        {
            bool CompareEpsilon(float a, float b) => Math.Abs(a - b) <= float.Epsilon;

            double x, y, z;

            //0,1, 2
            //4,5, 6
            //8,9,10

            if (CompareEpsilon(mtx.M13, 1f))
            {
                x = Math.Atan2(-mtx.M21, -mtx.M31);
                y = -Math.PI / 2;
                z = 0.0;
            }
            else if (CompareEpsilon(mtx.M13, -1f))
            {
                x = Math.Atan2(mtx.M21, mtx.M31);
                y = Math.PI / 2;
                z = 0.0;
            }
            else
            {
                x = Math.Atan2(mtx.M23, mtx.M33);
                y = -Math.Asin(mtx.M13);
                z = Math.Atan2(mtx.M12, mtx.M11);
            }

            return new Vector3((float)x, (float)y, (float)z);
        }

        public static Matrix3 CreateRotationX(double angle)
        {
            Matrix3 result;
            float cos = (float)(Math.Round(Math.Cos(angle) * 256f) / 256.0);
            float sin = (float)(Math.Round(Math.Sin(angle) * 256f) / 256.0);

            result.Row0 = Vector3.UnitX;
            result.Row1 = new Vector3(0.0f, cos, sin);
            result.Row2 = new Vector3(0.0f, -sin, cos);
            return result;
        }

        public static Matrix3 CreateRotationY(double angle)
        {
            Matrix3 result;
            float cos = (float)(Math.Round(Math.Cos(angle) * 256f) / 256.0);
            float sin = (float)(Math.Round(Math.Sin(angle) * 256f) / 256.0);

            result.Row0 = new Vector3(cos, 0.0f, -sin);
            result.Row1 = Vector3.UnitY;
            result.Row2 = new Vector3(sin, 0.0f, cos);
            return result;
        }

        public static Matrix3 CreateRotationZ(double angle)
        {
            Matrix3 result;
            float cos = (float)(Math.Round(Math.Cos(angle) * 256f) / 256.0);
            float sin = (float)(Math.Round(Math.Sin(angle) * 256f) / 256.0);

            result.Row0 = new Vector3(cos, sin, 0.0f);
            result.Row1 = new Vector3(-sin, cos, 0.0f);
            result.Row2 = Vector3.UnitZ;
            return result;
        }

        public static float GetRotationX(Matrix3 mtx)
        {
            return (float)(180 * Math.Atan2(mtx.M23, mtx.M33) / Math.PI);
        }

        public static float GetRotationY(Matrix3 mtx)
        {
            return (float)(180 * Math.Atan2(mtx.M31, mtx.M11) / Math.PI);
        }

        public static float GetRotationZ(Matrix3 mtx)
        {
            return (float)(180 * Math.Atan2(mtx.M12, mtx.M22) / Math.PI);
        }
    }
}
