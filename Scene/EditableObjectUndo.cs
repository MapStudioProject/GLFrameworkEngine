using System.Collections.Generic;

namespace GLFrameworkEngine
{
    public class EditableObjectDeletedUndo : IRevertable
    {
        List<ObjectInfo> Info = new List<ObjectInfo>();

        public EditableObjectDeletedUndo(List<ObjectInfo> info) {
            this.Info = info;
        }

        public EditableObjectDeletedUndo(GLScene scene, IEnumerable<IDrawable> objects)
        {
            foreach (var ob in objects)
                Info.Add(new ObjectInfo(scene, ob));
        }

        public IRevertable Revert()
        {
            for (int i = 0; i < Info.Count; i++) {
                Info[i].Scene.AddRenderObject(Info[i].Node, false);
            }
            return new EditableObjectAddUndo(Info);
        }
    }

    public class EditableObjectAddUndo : IRevertable
    {
        List<ObjectInfo> Info = new List<ObjectInfo>();

        public EditableObjectAddUndo(List<ObjectInfo> info)
        {
            this.Info = info;
        }

        public EditableObjectAddUndo(GLScene scene, IEnumerable<IDrawable> objects)
        {
            foreach (var ob in objects)
                Info.Add(new ObjectInfo(scene, ob));
        }

        public IRevertable Revert()
        {
            for (int i = 0; i < Info.Count; i++) {
                Info[i].Scene.RemoveRenderObject(Info[i].Node, false);
            }
            return new EditableObjectDeletedUndo(Info);
        }
    }

    public class ObjectInfo
    {
        public GLScene Scene;
        public IDrawable Node;

        public ObjectInfo(GLScene scene, IDrawable node)
        {
            Scene = scene;
            Node = node;
        }
    }
}
