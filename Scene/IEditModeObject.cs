using System;
using System.Collections.Generic;
using System.Text;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Used to toggle edit mode for the object.
    /// </summary>
    public interface IEditModeObject
    {
        IEnumerable<ITransformableObject> Selectables { get; }

        bool EditMode { get; set; }
    }
}
