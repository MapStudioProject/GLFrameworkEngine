using System;
using System.Collections.Generic;
using System.Text;

namespace GLFrameworkEngine
{
    public interface ITransformableObject
    {
        GLTransform Transform { get; set; }

        bool IsHovered { get; set; }

        bool IsSelected { get; set; }

        bool CanSelect { get; set; }
    }
}
