using System;
using System.Collections.Generic;
using System.Text;

namespace GLFrameworkEngine
{
    public interface IRevertable
    {
        IRevertable Revert();
    }
}
