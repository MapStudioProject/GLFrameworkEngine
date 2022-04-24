using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.ViewModels;

namespace GLFrameworkEngine
{
    public interface IRenderNode
    {
        NodeBase UINode { get; set; }
    }
}
