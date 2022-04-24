using System;
using System.Collections.Generic;
using System.Text;

namespace GLFrameworkEngine
{
    public interface IFrustumCulling
    {
        bool EnableFrustumCulling { get; }

        bool InFrustum { get; set; }

        bool IsInsideFrustum(GLContext context);
    }
}
