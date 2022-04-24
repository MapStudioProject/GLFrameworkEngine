using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core;

namespace GLFrameworkEngine
{
    public class GenericModelRender : EditableObject, ISelectableContainer, IColorPickable
    {
        public List<GenericMeshRender> Meshes = new List<GenericMeshRender>();

        public SkeletonRenderer Skeleton;

        public override bool IsSelected 
        { 
            get => base.IsSelected; 
            set
            {
                 base.IsSelected = value;
                foreach (var mesh in Meshes)
                {
                    mesh.IsSelected = value;
                    mesh.IsHovered = value;
                }
            }
        }

        public Dictionary<string, STGenericTexture> Textures = new Dictionary<string, STGenericTexture>();

        public IEnumerable<ITransformableObject> Selectables => Meshes;

        private STGenericModel Model;

        public GenericModelRender(STGenericModel model) : base(null)
        {
            Model = model;
            Skeleton = new SkeletonRenderer(model.Skeleton);
            foreach (var mesh in model.Meshes)
                Meshes.Add(new GenericMeshRender(this, mesh));
            foreach (var tex in model.Textures)
                Textures.Add(tex.Name, tex);
        }

        public void DrawColorPicking(GLContext context)
        {
            foreach (var mesh in Meshes)
                mesh.DrawColorPicking(context);
        }

        public override void DrawModel(GLContext context, Pass pass)
        {
            Skeleton.DrawModel(context, pass);
            foreach (var mesh in Meshes)
                mesh.DrawModel(context, pass);
        }
    }
}
