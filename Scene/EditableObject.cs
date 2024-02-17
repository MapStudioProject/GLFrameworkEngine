using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.ViewModels;
using GLFrameworkEngine.UI;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents a base class for editable objects. 
    /// This can be used for transforming/selecting objects, 
    /// attaching GUI nodes and rendering models.
    /// </summary>
    public class EditableObject : ITransformableObject, IRayCastPicking, IDrawable, IObjectLink, IRenderNode
    {
        private bool _isVisible = true;

        /// <summary>
        /// Determines if the object is visible.
        /// </summary>
        public bool IsVisible
        {
            get {
                if (IsVisibleCallback != null)
                    return _isVisible && IsVisibleCallback.Invoke();

                return _isVisible; }
            set
            {
                if (_isVisible != value) {
                    _isVisible = value;
                    VisibilityChanged?.Invoke(this, EventArgs.Empty);
                    GLContext.ActiveContext.UpdateViewport = true;
                }
            }
        }

        public Func<bool> IsVisibleCallback;

        public EventHandler VisibilityChanged;
        public EventHandler SelectedChanged;

        public virtual bool UsePostEffects { get; } = false;

        public List<ITransformableObject> DestObjectLinks { get; set; } = new List<ITransformableObject>();
        public List<ITransformableObject> SourceObjectLinks { get; set; } = new List<ITransformableObject>();

        public Action<ITransformableObject> OnObjectLink { get; set; }
        public Action<ITransformableObject> OnObjectUnlink { get; set; }

        /// <summary>
        /// The transform of the object.
        /// </summary>
        public GLTransform Transform { get; set; } = new GLTransform();

        /// <summary>
        /// Determines if the object is in a hovered state or not.
        /// </summary>
        public virtual bool IsHovered { get; set; }

        private bool _isSelected = false;
        /// <summary>
        /// Determines if the object is in a selected state or not.
        /// </summary>
        public virtual bool IsSelected { 
            get {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                SelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Determines if the object can select or not.
        /// </summary>
        public virtual bool CanSelect { get; set; } = true;

        /// <summary>
        /// The node of the object to display in the GUI.
        /// </summary>
        public NodeBase UINode { get; set; }

        /// <summary>
        /// The parent node of the UI node.
        /// </summary>
        public NodeBase ParentUINode { get; set; }

        /// <summary>
        /// A drawer for displaying UI sprites of the object when it is at a distance.
        /// </summary>
        public SpriteDrawer SpriteDrawer { get; set; }

        public OpenTK.Vector4 BoundingSphere = new OpenTK.Vector4();

        public virtual BoundingNode BoundingNode { get; } = new BoundingNode()
        {
            Radius = 10,
            Box = new BoundingBox(new OpenTK.Vector3(-10), new OpenTK.Vector3(10)),
        };

        public Type PropertyTag { get; set; }

        public EventHandler DrawOpaqueCallback;
        public EventHandler DrawTransparentCallback;
        public EventHandler DrawCallback;

        public EventHandler DisposeCallback;

        public EventHandler RemoveCallback;
        public EventHandler AddCallback;

        public EditableObject(NodeBase parent) {
            ParentUINode = parent;
            UINode = new EditableObjectNode(this);
        }

        public virtual BoundingNode GetRayBounding()
        {
            if (SpriteDrawer != null)
                return SpriteDrawer.GetRayBounding();

            return null;
        }

        public virtual void DrawModel(GLContext context, Pass pass)
        {
            DrawCallback?.Invoke(this, EventArgs.Empty);

            if (pass == Pass.OPAQUE)
                DrawOpaqueCallback?.Invoke(this, EventArgs.Empty);
            if (pass == Pass.TRANSPARENT)
                DrawTransparentCallback?.Invoke(this, EventArgs.Empty);

            OpenTK.Graphics.OpenGL.GL.ColorMask(1, true, true, true, true);
        }

        public void DrawSprite(GLContext context)
        {
            if (SpriteDrawer == null)
                SpriteDrawer = new SpriteDrawer();

            SpriteDrawer.Transform = this.Transform;
            SpriteDrawer.IsSelected = this.IsHovered || this.IsSelected;
            SpriteDrawer.DrawModel(context);
        }

        public virtual void OnRemoved() {
            if (ParentUINode != null)
                ParentUINode.Children.Remove(UINode);
            RemoveCallback?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnAdded() {
            if (ParentUINode != null)
                ParentUINode.AddChild(UINode);
            AddCallback?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Dispose()
        {
            DisposeCallback?.Invoke(this, EventArgs.Empty);
        }
    }
}
