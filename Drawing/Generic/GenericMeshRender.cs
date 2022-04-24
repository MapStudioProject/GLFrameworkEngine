using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core;
using Toolbox.Core.ViewModels;
using OpenTK;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents a mesh renderer used by a generic mesh. 
    /// In most cases, these are used by file formats to draw their model data.
    /// Model formats use <c>STGenericModel</c> which consists of STGenericMesh meshes.
    /// </summary>
    public class GenericMeshRender : EditableObject, IColorPickable, IDragDropPicking
    {
        public STGenericMesh MeshData;

        public EventHandler OnVertexDataUpdated;

        //Mesh drawer
        public RenderMeshNonInterleaved Drawer;

        public List<PolygonRender> Polygons = new List<PolygonRender>();

        //Debug shading in tool
        DebugMaterial DebugMaterial = new DebugMaterial();
        //Bounding info
        BoundingBox BoundingBox { get; set; }

        BoundingBox BoundingBoxLocalSpace { get; set; }
        //Parent model used to store skeleton
        GenericModelRender ParentModel { get; set; }

        bool init = false;

        public override BoundingNode BoundingNode => new BoundingNode()
        {
            Box = BoundingBoxLocalSpace,
        };
       
        #region IDragDropPicking

        public void DragDroppedOnLeave()
        {

        }

        public void DragDroppedOnEnter()
        {

        }

        public void DragDropped(object droppedItem)
        {

        }

        #endregion

        public GenericMeshRender(int[] indices, PrimitiveType type) : base(null)
        {
            Drawer = new RenderMeshNonInterleaved(indices, type);
        }

        public void AssignMaterial(GenericMaterialRender materialRender)
        {
            foreach (var poly in Polygons)
                poly.Material = materialRender;
        }

        public GenericMeshRender(GenericModelRender parentRender, STGenericMesh mesh) : base(null)
        {
            ParentModel = parentRender;
            Init(mesh);
        }

        public GenericMeshRender(STGenericMesh mesh) : base(null)
        {
            Init(mesh);
        }

        private void Init(STGenericMesh mesh)
        {
            init = false;

            Drawer = new RenderMeshNonInterleaved(mesh.Faces.ToArray(), PrimitiveType.Triangles);

            UINode.Header = mesh.Name;
            UINode.Tag = mesh;
            UINode.ContextMenus.Add(new MenuItemModel("Smooth Normals", SmoothNormals));

            foreach (var poly in mesh.PolygonGroups)
                Polygons.Add(new PolygonRender(poly));

            MeshData = mesh;
            UpdateBoundingBox();
            RecalculateOrigin();
            ReloadMaterial();

            UpdateVertexData();

            init = true;
        }

        public void DrawColorPicking(GLContext context)
        {
            if (!IsVisible)
                return;

            var shader = GlobalShaders.GetShader("PICKING");
            context.CurrentShader = shader;

            shader.SetTransform(GLConstants.ModelMatrix, this.Transform);

            context.ColorPicker.SetPickingColor(this, shader);
            Drawer.Draw(context);
        }

        /// <summary>
        /// Reloads the material data along with the assigned buffer layouts.
        /// </summary>
        public void ReloadMaterial()
        {
            foreach (var poly in Polygons)
                poly.Material.Init(this, MeshData, poly.PolygonData.Material);
        }

        /// <summary>
        /// Updates the current vertex data with the set of vertices provided by the generic mesh.
        /// </summary>
        public void UpdateVertexData() {
            foreach (var poly in Polygons)
                poly.Material.UpdateVertexData(this);

            //Only update the event when the data is already loaded
            if (init)
                OnVertexDataUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Updates the boundings calculated by the vertex positions of the generic mesh.
        /// </summary>
        private void UpdateBoundingBox()
        {
            var positions = MeshData.Vertices.Select(x => x.Position);
            BoundingBox.CalculateMinMax(positions.ToArray(),
                  out Vector3 min, out Vector3 max);
            //Bounding box for frustum culling and rectangle tool
            BoundingBox = BoundingBox.FromMinMax(min, max);
            //Bounding sphere for frame capture
            BoundingSphere = Utils.BoundingSphereGenerator.GenerateBoundingSphere(positions.ToList());
        }

        /// <summary>
        /// Recalculates the vertex positions by turning it into local space from the current transform.
        /// This allows the vertices to use the current transform space.
        /// </summary>
        private void RecalculateOrigin()
        {
            this.Transform.Position = BoundingBox.GetCenter();
            this.Transform.UpdateMatrix(true);

            //Bounding box in non transform space.
            BoundingBoxLocalSpace = BoundingBox.FromMinMax(BoundingBox.Min, BoundingBox.Max);
            BoundingBoxLocalSpace.ApplyTransform(this.Transform.TransformMatrix.Inverted());

            Matrix4 invTransform = this.Transform.TransformMatrix.Inverted();
            for (int i = 0; i < MeshData.Vertices.Count; i++)
            {
                //Convert into local space for transform handling.
                Vector3 pos = Vector3.TransformPosition(MeshData.Vertices[i].Position, invTransform);
                Vector3 nrm = Vector3.TransformNormal(MeshData.Vertices[i].Normal, invTransform);

                MeshData.Vertices[i].Position = pos;
                MeshData.Vertices[i].Normal = nrm;
            }
        }

        public override void DrawModel(GLContext context, Pass pass)
        {
            if (!Polygons.Any(x => x.Material.Pass == pass) || !IsVisible)
                return;

            context.UseSRBFrameBuffer = false;

            if (DebugShaderRender.DebugRendering != DebugShaderRender.DebugRender.Default)
            {
                DebugMaterial.Render(context);
                context.CurrentShader.SetTransform(GLConstants.ModelMatrix, this.Transform);
                context.CurrentShader.SetVector4(GLConstants.SelectionColorUniform, Vector4.Zero);

                if (IsSelected || IsHovered)
                    context.CurrentShader.SetVector4(GLConstants.SelectionColorUniform, GLConstants.SelectColor);

                Drawer.Draw(context);
                return;
            }

            foreach (var poly in Polygons)
            {
                poly.Material.Render(context, ParentModel, this);
                Drawer.DrawWithSelection(context, this.IsSelected | this.IsHovered);

              //  Drawer.Draw(context.CurrentShader, poly.PolygonData.Faces.Count, poly.PolygonData.FaceOffset);
            }
        }

        private void SmoothNormals()
        {
            bool cancel = false;
            this.MeshData.SmoothNormals(ref cancel);
            UpdateVertexData();
        }

        private void FlipUVs()
        {
            this.MeshData.FlipUvsVertical();
            UpdateVertexData();
        }

        public class PolygonRender
        {
            public STPolygonGroup PolygonData;

            //Material to render on.
            public GenericMaterialRender Material = new GenericMaterialRender();

            public PolygonRender(STPolygonGroup poly) {
                PolygonData = poly;
            }
        }
    }
}
