using System;
using System.Collections.Generic;
using System.Text;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents an input used from an IDrawable instance in the scene.
    /// </summary>
    public interface IDrawableInput
    {
        void OnMouseDown(GLContext context, MouseEventInfo mouseInfo);
        void OnMouseUp(GLContext context, MouseEventInfo mouseInfo);
        void OnMouseMove(GLContext context, MouseEventInfo mouseInfo);
        void OnKeyDown(GLContext context, KeyEventInfo keyInfo);
    }
}
