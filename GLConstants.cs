using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace GLFrameworkEngine
{
    public class GLConstants
    {
        //Common Constants
        public const string ModelMatrix = "mtxMdl";
        public const string ViewMatrix = "mtxView";
        public const string ProjMatrix = "mtxProj";
        public const string ViewProjMatrix = "mtxCam";

        public const string VPosition = "vPosition";
        public const string VNormal = "vNormal";
        public const string VTexCoord0 = "vTexCoord0";
        public const string VTexCoord1 = "vTexCoord1";
        public const string VTexCoord2 = "vTexCoord2";
        public const string VColor = "vColor";
        public const string VBoneIndex = "vBoneIndex";
        public const string VBoneWeight = "vBoneWeight";
        public const string VTangent = "vTangent";
        public const string VBitangent = "vBitangent";

        public const string SelectionColorUniform = "highlight_color";
        public const float SelectionWidth = 6;

        public readonly static Vector3 AxisColorX = new Vector3(1, 0, 0);
        public readonly static Vector3 AxisColorY = new Vector3(0, 1, 0);
        public readonly static Vector3 AxisColorZ = new Vector3(0, 0, 1);

        public readonly static Vector4 SelectOutlineColor = new Vector4(1, 1, 1, 1);
        public readonly static Vector4 SelectColor = new Vector4(1, 1, 0.5f, 0.1f);
        public readonly static Vector4 HoveredColor = new Vector4(1, 1, 1, 0.1f);
    }
}
