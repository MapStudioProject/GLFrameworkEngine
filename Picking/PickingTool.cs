using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFrameworkEngine
{
    public class PickingTool
    {
        public bool Enabled = true;

        /// <summary>
        /// An event for when an object has been picked.
        /// </summary>
        public EventHandler OnObjectPicked;

        /// <summary>
        /// Determines to enable the eye dropper tool.
        /// This will run OnObjectPicked event when the user picks a model.
        /// </summary>
        public bool UseEyeDropper
        {
            get { return useEyeDropper; }
            set
            {
                if (useEyeDropper != value) {
                    useEyeDropper = value;
                    if (value)
                        MouseEventInfo.MouseCursor = MouseEventInfo.Cursor.EyeDropper;
                    else
                        MouseEventInfo.MouseCursor = MouseEventInfo.Cursor.Arrow;
                }
            }
        }

        private bool hoverSelection = false;
        private bool useEyeDropper = false;

        public void OnMouseDown(GLContext context, MouseEventInfo e)
        {
            //Use the eye dropper tool if enabled and skip the normal picking mode.
            if (UseEyeDropper) {
                UseEyeDropPickerTool(context, e);
                return;
            }

            if (e.LeftButton == OpenTK.Input.ButtonState.Pressed)
                PickScene(context, e, true);
        }

        public void OnMouseMove(GLContext context, MouseEventInfo e)
        {
            if (hoverSelection)
                PickScene(context, e, false);
        }

        private void UseEyeDropPickerTool(GLContext context, MouseEventInfo e)
        {
            Vector2 position = new Vector2(e.Position.X, context.Height - e.Position.Y);
            var pickable = context.Scene.FindPickableAtPosition(context, position);
            //When the eye picker tool is used, this event should be subscribed to use the tool.
            if (pickable != null)
                OnObjectPicked?.Invoke(pickable, EventArgs.Empty);
            //Disable the eye dropper tool after use.
            UseEyeDropper = false;
        }

        public void OnMouseUp(GLContext context, MouseEventInfo e) { }

        public void PickScene(GLContext context, MouseEventInfo e, bool selectAction)
        {
            //Don't pick during a selection tool or an active transformation
            if (context.SelectionTools.IsActive || context.TransformTools.IsActive || !this.Enabled)
                return;

            //Deselect unless a hovered pick occurs or the user is holding down left control
            if (selectAction && !KeyEventInfo.State.KeyCtrl && !KeyEventInfo.State.KeyShift)
            {
                context.Scene.DeselectAll(context);
            }
            else if (!selectAction)
            {
                //Deselect all objects first before picking during a selection action.
                foreach (var obj in context.Scene.GetSelectableObjects())
                    obj.IsHovered = false;
            }

            //Skip color picking if it isn't enabled
            if (!context.ColorPicker.EnablePicking)
                return;

            Vector2 position = new Vector2(e.Position.X, context.Height - e.Position.Y);
            var pickable = (ITransformableObject)context.Scene.FindPickableAtPosition(context, position);
            if (pickable != null)
            {
                pickable.IsHovered = true;
                if (selectAction && pickable.CanSelect)
                {
                    pickable.IsSelected = true;
                }
                OnObjectPicked?.Invoke(pickable, EventArgs.Empty);
            }

            if (selectAction)
            {
                if (!(pickable != null && !pickable.CanSelect))
                    context.Scene.OnSelectionChanged(context, pickable);

                //Update the transform handler 
                if (context.TransformTools.ActiveActions.Count > 0)
                    context.TransformTools.OnMouseDown(context, e, true);
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, context.Width, context.Height);
        }
    }
}
