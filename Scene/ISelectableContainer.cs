using System;
using System.Collections.Generic;
using System.Text;

namespace GLFrameworkEngine
{
    /// <summary>
    /// A list of selectable, transformable objects.
    /// </summary>
    public interface ISelectableContainer
    {
        IEnumerable<ITransformableObject> Selectables { get; }
    }
}
