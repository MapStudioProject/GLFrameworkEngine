using System;
using OpenTK;

namespace GLFrameworkEngine.Utils
{
    public class ColorUtility
    {
        public static Vector4 ToVector4(byte[] color)
        {
            if (color == null || color.Length != 4)
                throw new Exception("Invalid color length found! (ToVector4)");

            return new Vector4(color[0] / 255.0f,
                               color[1] / 255.0f,
                               color[2] / 255.0f,
                               color[3] / 255.0f);
        }
    }
}
