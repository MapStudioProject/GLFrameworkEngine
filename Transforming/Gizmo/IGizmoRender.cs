using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace GLFrameworkEngine
{
    public interface IGizmoRender
    {
        TransformEngine.Axis UpdateAxisSelection(GLContext context, Vector3 position, Quaternion rotation, Vector2 point, TransformSettings settings);
    }
}
