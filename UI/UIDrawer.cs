using System;
using System.Collections.Generic;
using System.Text;

namespace GLFrameworkEngine
{
    public class UIDrawer
    {
        public EventHandler OnRender;

        public UIDrawer()
        {

        }

        public void Render(GLContext context)
        {
            OnRender?.Invoke(this, EventArgs.Empty);
        }
    }
}
