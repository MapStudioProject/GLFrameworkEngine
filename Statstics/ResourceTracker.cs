using System;
using System.Collections.Generic;
using System.Text;

namespace GLFrameworkEngine
{
    public class ResourceTracker
    {
        public static int NumDrawTriangles;
        public static int NumDrawCalls;
        public static int NumShadowDrawCalls;

        public static int NumEffectDrawCalls;
        public static int NumEffectTriangles;
        public static int NumEffectInstances;

        public static void ResetStats()
        {
            ResourceTracker.NumDrawCalls = 0;
            ResourceTracker.NumDrawTriangles = 0;
            ResourceTracker.NumShadowDrawCalls = 0;
            ResourceTracker.NumEffectDrawCalls = 0;
            ResourceTracker.NumEffectTriangles = 0;
            ResourceTracker.NumEffectInstances = 0;         
        }
    }
}
