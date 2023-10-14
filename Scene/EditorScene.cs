using System;
using System.Collections.Generic;
using System.Linq;

namespace GLFrameworkEngine
{
    public partial class GLScene
    {
        public List<EditableObject> GetEditObjects()
        {
            List<EditableObject> objects = new List<EditableObject>();
            foreach (var ob in this.Objects)
            {
                if (ob is EditableObject && ((EditableObject)ob).IsSelected)
                    objects.Add((EditableObject)ob);
            }
            return objects;
        }

        public void OnKeyDown(GLContext context, KeyEventInfo e, bool isRepeat)
        {
            if (e.IsKeyDown(InputSettings.INPUT.Scene.Undo))
                Undo();
            if (e.IsKeyDown(InputSettings.INPUT.Scene.Redo))
                Redo();

            if (!isRepeat)
            {
                if (e.IsKeyDown(InputSettings.INPUT.Scene.SelectAll))
                    SelectAll(context);
                if (e.IsKeyDown(InputSettings.INPUT.Scene.EditMode))
                    ToggleEditMode(context);
                if (e.IsKeyDown(InputSettings.INPUT.Scene.Copy))
                    CopySelected();
                if (e.IsKeyDown(InputSettings.INPUT.Scene.Paste))
                    PasteSelected();
                if (e.IsKeyDown(InputSettings.INPUT.Scene.Delete))
                    DeleteSelected();
                if (!e.KeyCtrl && !e.KeyShift && !e.KeyAlt && e.IsKeyDown(InputSettings.INPUT.Camera.FocusOnSelectedObject))
                {
                    var selected = GetSelected();
                    if (selected.Count > 0)
                        context.Camera.FocusOnObject(selected.Last().Transform);
                }

                foreach (IDrawableInput ob in Objects.Where(x => x is IDrawableInput))
                    ob.OnKeyDown(context, e);
            }
        }

        public void DeleteSelected() {
            var selected = GetEditObjects();
            AddToUndo(new EditableObjectDeletedUndo(this, selected));
            //Remove edit object types
            foreach (var ob in selected) {
                this.RemoveRenderObject(ob);
            }
            //Remove path types
            foreach (var ob in this.Objects)
            {
                if (ob is RenderablePath && ((RenderablePath)ob).EditMode)
                    ((RenderablePath)ob).RemoveSelected();
            }

            GLContext.ActiveContext.UpdateViewport = true;
        }

        List<EditableObject> copiedObjects = new List<EditableObject>();

        public void CopySelected() {
            var selected = GetEditObjects();

            copiedObjects.Clear();
            foreach (EditableObject obj in selected)
            {
                if (obj is ICloneable)
                    copiedObjects.Add(obj);
            }
        }

        public void PasteSelected()
        {
            if (copiedObjects.Count > 0)
                this.AddToUndo(new EditableObjectAddUndo(this, copiedObjects));

            foreach (var obj in copiedObjects)
            {
                EditableObject copy = ((ICloneable)obj).Clone() as EditableObject;
                AddRenderObject(copy);
            }
        }

        /// <summary>
        /// Toggles edit mode for an editable part object.
        /// </summary>
        public void ToggleEditMode(GLContext context)
        {
            EditModeObjects.Clear();
            foreach (var obj in this.GetSelectableObjects())
            {
                if (obj is IEditModeObject)
                {
                    if (!((IEditModeObject)obj).CanToggleEditMode)
                        continue;

                    bool editMode = ((IEditModeObject)obj).EditMode;
                    if (!editMode && obj.IsSelected)
                    {
                        EditModeObjects.Add(obj);
                        ((IEditModeObject)obj).EditMode = true;
                    }
                    else if (editMode)
                    {
                        obj.IsSelected = true;
                        EditModeObjects.Remove(obj);
                        ((IEditModeObject)obj).EditMode = false;
                    }
                }
            }
            context.TransformTools.InitAction(GetSelected());
        }

        public void DisableEditMode(ITransformableObject obj)
        {
            obj.IsSelected = false;
            if (EditModeObjects.Contains(obj))
                EditModeObjects.Remove(obj);
            ((IEditModeObject)obj).EditMode = false;
        }
    }
}
