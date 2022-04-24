using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.ViewModels;

namespace GLFrameworkEngine.UI
{
    public interface UIEditToolMenu
    {
        List<MenuItemModel> GetToolMenuItems();
        List<MenuItemModel> GetEditMenuItems();
    }
}
