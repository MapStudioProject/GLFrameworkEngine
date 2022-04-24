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
        void OnMouseDown(MouseEventInfo mouseInfo);
        void OnMouseUp(MouseEventInfo mouseInfo);
        void OnMouseMove(MouseEventInfo mouseInfo);
        void OnKeyDown(KeyEventInfo keyInfo);
    }
}
