using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace GLFrameworkEngine
{
    public class SelectionToolEngine
    {
        public bool IsActive => SelectionBox != null || SelectionCircle != null;

        public bool IsSelectionMode = false;

        SelectionBox SelectionBox = null;
        SelectionCircle SelectionCircle = null;

        public void OnKeyDown(GLContext context, KeyEventInfo e)
        {
            if (IsActive)
                return;

            //Shortcuts for selection tools
            if (!e.KeyCtrl && e.IsKeyDown(InputSettings.INPUT.Scene.SelectionBox))
                SelectionBox = new SelectionBox();
            if (!e.KeyCtrl && e.IsKeyDown(InputSettings.INPUT.Scene.SelectionCircle))
            {
                SelectionCircle = new SelectionCircle();
                SelectionCircle?.Start(context,
                    context.CurrentMousePoint.X,
                    context.CurrentMousePoint.Y);
            }
        }

        public void OnMouseWheel(GLContext context, MouseEventInfo e)
        {
            //Resize the selection tools
            SelectionCircle?.Resize(e.Delta);
        }

        public void OnMouseMove(GLContext context, MouseEventInfo e)
        {
            //During selection mode, the user can freely create a box during a mouse down/move
            if (IsSelectionMode && previouseMouseDown != Vector2.Zero && SelectionBox == null) {
                //Check for the mouse to actually move to create a selection box
                var delta = previouseMouseDown - new Vector2(e.X, e.Y);
                if (delta != Vector2.Zero)
                {
                    SelectionBox = new SelectionBox();
                    SelectionBox.StartSelection(context, e.X, e.Y);
                }
            }

            //Apply selection
            if (e.LeftButton == OpenTK.Input.ButtonState.Pressed)
                SelectionCircle?.Apply(context, e.X, e.Y, true);
            //Apply deselection
            if (e.MiddleButton == OpenTK.Input.ButtonState.Pressed)
                SelectionCircle?.Apply(context, e.X, e.Y, false);
        }

        private Vector2 previouseMouseDown;

        public void OnMouseDown(GLContext context, MouseEventInfo e)
        {
            if (IsSelectionMode && e.LeftButton == OpenTK.Input.ButtonState.Pressed)
                previouseMouseDown = new Vector2(e.X, e.Y);

            //Start deselection
            if (e.MiddleButton == OpenTK.Input.ButtonState.Pressed)
                SelectionBox?.StartDeselection(context, e.X, e.Y);
            //Start selection
            if (e.LeftButton == OpenTK.Input.ButtonState.Pressed)
                SelectionBox?.StartSelection(context, e.X, e.Y);

            //Disable selection tools
            if (e.RightButton == OpenTK.Input.ButtonState.Pressed)
            {
                SelectionBox = null;
                SelectionCircle = null;
            }
        }

        public void OnMouseUp(GLContext context, MouseEventInfo e)
        {
            previouseMouseDown = new Vector2(0, 0);

            //Apply then disable selection tools
            if (e.LeftButton == OpenTK.Input.ButtonState.Released)
            {
                SelectionBox?.Apply(context, e.X, e.Y);
            }
            SelectionBox = null;
        }

        public void Render(GLContext context, float x, float y)
        {
            //Draw the selection tools
            SelectionBox?.Render(context, x, y);
            SelectionCircle?.Render(context, x, y);
        }
    }
}
