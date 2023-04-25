using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Core;
using Toolbox.Core.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents a generic material renderer used by a generic mesh.
    /// </summary>
    public class GenericMaterialRender
    {
        StandardMaterial MaterialData = new StandardMaterial();

        List<RenderAttribute> Attributes = new List<RenderAttribute>();

        public Pass Pass = Pass.OPAQUE;

        STGenericMaterial Material;

        public virtual void Init(GenericMeshRender meshRender, STGenericMesh mesh, STGenericMaterial material)
        {
            Material = material;
            MaterialData.HalfLambertShading = true;
            MaterialData.Color = new OpenTK.Vector4(0.5f, 0.5f, 0.5f, 1.0f);

            UpdateAttributes(meshRender);

            meshRender.Drawer.ClearAttributes();
            meshRender.Drawer.AddAttributes(Attributes.ToArray());
        }

        public virtual void Render(GLContext context, GenericModelRender parentModel, EditableObject obj)
        {
            MaterialData.DisplaySelection = obj.IsSelected || obj.IsHovered;
            MaterialData.ModelMatrix = obj.Transform.TransformMatrix;

            if (parentModel != null && Material != null) {
                foreach (var tex in Material.TextureMaps)
                {
                    if (!parentModel.Textures.ContainsKey(tex.Name))
                        continue;

                    var texture = parentModel.Textures[tex.Name];
                    if (tex.Type == STTextureType.Diffuse)
                    {
                        if (texture.RenderableTex == null)
                            texture.LoadRenderableTexture();
                        if (texture.RenderableTex != null)
                            MaterialData.DiffuseTextureID = texture.RenderableTex.ID;
                    }
                }
            }

            MaterialData.Render(context);
        }

        private void UpdateAttributes(GenericMeshRender meshRender)
        {
            Attributes.Clear();

            var vertex = meshRender.MeshData.Vertices[0];
            int bufferIndex = 0;

            AddAttribute(GLConstants.VPosition, 3, bufferIndex++);
            AddAttribute(GLConstants.VNormal, 3, bufferIndex++);

            int offset = 0;
            for (int i = 0; i < vertex.TexCoords?.Length; i++)
                AddAttribute($"vTexCoord{i}", 2, bufferIndex++);

            if (meshRender.MeshData.Vertices.Any(x => x.BoneIndices.Count > 0))
            {
                AddAttribute(GLConstants.VBoneWeight, 4, bufferIndex++);
                AddAttribute(GLConstants.VBoneIndex, 4, bufferIndex++, true);
            }
            if (vertex.Colors.Length > 0)
                AddAttribute(GLConstants.VColor, 4, bufferIndex++);
        }

        /// <summary>
        /// Updates the current vertex data with the set of vertices provided by the generic mesh.
        /// </summary>
        public virtual void UpdateVertexData(GenericMeshRender meshRender)
        {
            var vertex = meshRender.MeshData.Vertices[0];
            UpdateAttributes(meshRender);

            int bufferIndex = 0;

            //Positions
            var positions = meshRender.MeshData.Vertices.Select(x => x.Position).ToArray();
            meshRender.Drawer.SetData(positions, bufferIndex++);
            //Normals
            var normals = meshRender.MeshData.Vertices.Select(x => x.Normal).ToArray();
            meshRender.Drawer.SetData(normals, bufferIndex++);
            for (int i = 0; i < vertex.TexCoords?.Length; i++)
            {
                //TexCoords
                var texCoords = meshRender.MeshData.Vertices.Select(x => x.TexCoords[i]).ToArray();
                meshRender.Drawer.SetData(texCoords, bufferIndex++);
            }
            if (meshRender.MeshData.Vertices.Any(x => x.BoneIndices.Count > 0))
            {
                //Bone indices
                int[] boneIndices = new int[4 * meshRender.MeshData.Vertices.Count];
                float[] boneWeights = new float[4 * meshRender.MeshData.Vertices.Count];

                for (int i = 0; i < meshRender.MeshData.Vertices.Count; i++) {
                    var vert = meshRender.MeshData.Vertices[i];

                    for (int j = 0; j < vert.BoneIndices.Count; j++)
                        boneIndices[(i * 4) + j] = vert.BoneIndices[j];
                    for (int j = 0; j < vert.BoneWeights.Count; j++)
                        boneWeights[(i * 4) + j] = vert.BoneWeights[j];
                }
                meshRender.Drawer.SetData(boneWeights, bufferIndex++);
                meshRender.Drawer.SetData(boneIndices, bufferIndex++);
            }
            //Colors
            if (vertex.Colors.Length > 0)
            {
                var colors = meshRender.MeshData.Vertices.Select(x => x.Colors.FirstOrDefault()).ToArray();
                meshRender.Drawer.SetData(colors, bufferIndex++);
            }
            //Init buffers
            meshRender.Drawer.InitVertexBufferObject();
        }

        private void AddAttribute(string name, int count, int bufferIndex = 0, bool isInt = false) {
            var type = isInt ? VertexAttribPointerType.Int : VertexAttribPointerType.Float;
            Attributes.Add(new RenderAttribute(name, type, 0, count) { BufferIndex = bufferIndex });
        }

        private void AddAttribute(string name, ref int offset, int count, int bufferIndex = 0, bool isInt = false) {
            var type = isInt ? VertexAttribPointerType.Int : VertexAttribPointerType.Float;
            Attributes.Add(new RenderAttribute(name, type, offset, count) { BufferIndex = bufferIndex });

             offset += Attributes[0].Size;
        }
    }
}
