using System;
using System.Collections.Generic;
using System.Text;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Parameters for how a rendered frame is displayed
    /// </summary>
    public class RenderFrameArgs
    {
        public bool DisplayCursor3D = true;
        public bool Display2DSprites = false;
        public bool DisplayAlpha = false;
        public bool DisplayBackground = true;
        public bool DisplayFloor = true;
        public bool DisplayGizmo = true;
        public bool DisplayOrientationGizmo = true;
    }
}
