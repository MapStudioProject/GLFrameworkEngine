using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLFrameworkEngine
{
    public interface IDrawable
    {
        bool IsVisible { get; set; }

        void DrawModel(GLContext control, Pass pass);

        void Dispose();
    }
}
