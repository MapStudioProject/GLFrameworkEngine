using System;
using System.Collections.Generic;
using System.Text;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Used to determine objects linked to another.
    /// Will display in the 3D scene when both objects are visible.
    /// </summary>
    public interface IObjectLink
    {
        List<ITransformableObject> DestObjectLinks { get; set; }
        List<ITransformableObject> SourceObjectLinks { get; set; }

        Action<ITransformableObject> OnObjectLink { get; set; }
        Action<ITransformableObject> OnObjectUnlink { get; set; }
    }
}
