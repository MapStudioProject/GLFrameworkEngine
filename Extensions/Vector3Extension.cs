using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace GLFrameworkEngine
{
    public static class Vector3Extension
    {
        public static bool IsUniform(this Vector3 value) {
            return value.X == value.Y && value.Y == value.Z;
        }

        public static Vector3 Snap(this Vector3 value, Vector3 snap)
        {
            if (snap.Length > 0.0f)
                return new Vector3(
                    MathF.Floor(value.X / snap.X) * snap.X,
                    MathF.Floor(value.Y / snap.Y) * snap.Y,
                    MathF.Floor(value.Z / snap.Y) * snap.Z);
            return value;
        }

        public static Vector3 Snap(this Vector3 value, float snap)
        {
            if (snap > 0.0f)
                return new Vector3(
                    MathF.Floor(value.X / snap) * snap,
                    MathF.Floor(value.Y / snap) * snap,
                    MathF.Floor(value.Z / snap) * snap);
            return value;
        }
    }
}
