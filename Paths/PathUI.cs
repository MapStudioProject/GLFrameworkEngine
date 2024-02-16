using System;
using System.Collections.Generic;
using System.Numerics;
using Toolbox.Core.ViewModels;

namespace GLFrameworkEngine
{
    public partial class RenderablePath : IDrawable, IEditModeObject, IColorPickable, ITransformableObject
    {
        //For rendering paths in the tree
        public class PathNode : NodeBase
        {
            public override string Header
            {
                get
                {
                    if (GetHeader != null)
                        return GetHeader(); 
                    return Path.Name;
                }
            }

            public List<string> Icons = new List<string>();

            public RenderablePath Path;

            public override bool IsChecked
            {
                get => Path.IsVisible;
                set => Path.IsVisible = value;
            }

            public override bool IsSelected
            {
                get => Path.IsSelected;
                set
                {
                    if (Path.IsSelected != value) {
                        Path.IsSelected = value;
                        base.IsSelected = value;

                        GLContext.ActiveContext.Scene.OnSelectionChanged(GLContext.ActiveContext);
                    }
                }
            }

            public PathNode(RenderablePath path) {
                Path = path;
                HasCheckBox = true;
            }

            public override void OnDoubleClicked()
            {
                //Focus the camera on the double clicked object attached to the node
                var context = GLContext.ActiveContext;
                context.Camera.FocusOnObject(Path.Transform);
            }
        }

        //For rendering points in the tree
        public class PointNode : NodeBase
        {
            public override string Header
            {
                get
                {
                    if (GetHeader != null)
                        return GetHeader();
                    return Point.Name;
                }
                set
                {

                }
            }

            public RenderablePathPoint Point;

            public override bool IsChecked
            {
                get => Point.IsVisible;
                set => Point.IsVisible = value;
            }

            public override bool IsSelected
            {
                get => Point.IsSelected;
                set
                {
                    if (Point.IsSelected != value) {
                        Point.IsSelected = value;
                        GLContext.ActiveContext.Scene.OnSelectionChanged(GLContext.ActiveContext, Point);
                        OnSelected?.Invoke(this, EventArgs.Empty);
                    }
                }
            }

            public PointNode(RenderablePathPoint point)
            {
                Point = point;
                HasCheckBox = true;
            }

            public override void OnDoubleClicked()
            {
                //Focus the camera on the double clicked object attached to the node
                var context = GLContext.ActiveContext;
                context.Camera.FocusOnObject(Point.Transform);
            }
        }
    }
}
