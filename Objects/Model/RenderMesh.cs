using System;
using System.Collections.Generic;
using System.Reflection;
using OpenTK.Graphics.OpenGL;
using System.Linq;

namespace GLFrameworkEngine
{
    public class RenderMesh<TVertex> : RenderMeshBase where TVertex : struct
    {
        private VertexBufferObject vao;

        public RenderMesh() { }

        public RenderMesh(TVertex[] vertices, PrimitiveType primitiveType) : base(primitiveType) {
            Init(vertices);
        }

        public RenderMesh(TVertex[] vertices, int[] indices, PrimitiveType primitiveType) : base(primitiveType)
        {
            Init(vertices, indices);
        }

        protected override void BindVAO()
        {
            vao.Use();
        }

        protected override void PrepareAttributes(ShaderProgram shader)
        {
            vao.Enable(shader);
        }

        protected void Init(TVertex[] vertices, int[] indices = null, BufferUsageHint usage = BufferUsageHint.StaticDraw)
        {
            int? indexBuffer = null;

            //Search for attributes in the given vertex type
            attributes = RenderAttribute.GetAttributes<TVertex>();
            //Set the vertex stride
            var vertexStride = attributes.Sum(x => x.Size);
            //Set the draw count
            DrawCount = vertices.Length;

            //Init the buffers into gl data
            buffers = new BufferObject[1];
            InitVertexData(vertices, 0);
            if (indices != null) {
                indexBufferData = new BufferObject(BufferTarget.ElementArrayBuffer);
                indexBufferData.SetData(indices, usage);
                indexBuffer = indexBufferData.ID;

                DrawCount = indices.Length;
            }

            //Init the attributes into gl data
            vao = new VertexBufferObject(buffers[0].ID, indexBuffer);
            foreach (var att in attributes)
            {
                if (!string.IsNullOrEmpty(att.Name))
                    vao.AddAttribute(att.Name, att.ElementCount, att.Type, att.Normalized, vertexStride, att.Offset.Value);
                else
                    vao.AddAttribute(att.Location, att.ElementCount, att.Type, att.Normalized, vertexStride, att.Offset.Value);
            }
            vao.Initialize();
        }

        public void UpdateVertexData(TVertex[] vertices, BufferUsageHint usage = BufferUsageHint.StaticDraw) {
            buffers[0].SetData(vertices, usage);

            if (indexBufferData == null)
                DrawCount = vertices.Length;
        }

        /// <summary>
        /// Updates the existing instanced data. 
        /// </summary>
        public void UpdateInstanceData<TInstance>(TInstance[] vertices, BufferUsageHint usage = BufferUsageHint.StaticDraw) where TInstance : struct {
            instancedBuffers[0].SetData(vertices, usage);
        }

        /// <summary>
        /// Loads instanced data into the current vertex array object.
        /// </summary>
        public void LoadInstanceAttributes<TInstance>(TInstance[] vertices) where TInstance : struct
        {
            //Search for attributes in the given vertex type
            attributes = RenderAttribute.GetAttributes<TInstance>();
            var vertexStride = attributes.Sum(x => x.Size);

            foreach (var att in attributes)
            {
                if (!string.IsNullOrEmpty(att.Name))
                    vao.AddInstancedAttribute(att.Name, att.ElementCount, att.Type, att.Normalized, vertexStride, att.Offset.Value);
                else
                    vao.AddInstancedAttribute(att.Location, att.ElementCount, att.Type, att.Normalized, vertexStride, att.Offset.Value);
            }

            if (instancedBuffers.Length == 0) {
                instancedBuffers = new BufferObject[1];
                UpdateInstanceData(vertices);
            }
            vao.UpdateInstanceBuffer(instancedBuffers[0].ID);
        }

        private void InitVertexData(TVertex[] vertices, int index) {
            buffers[index] = new BufferObject(BufferTarget.ArrayBuffer);
            buffers[index].SetData(vertices, BufferUsageHint.StaticDraw);
        }

        private void InitInstancedData<TInstance>(TInstance[] vertices, int index) where TInstance : struct {
            instancedBuffers[index] = new BufferObject(BufferTarget.ArrayBuffer);
            instancedBuffers[index].SetData(vertices, BufferUsageHint.StaticDraw);
        }

        public override void Dispose()
        {
            vao.Dispose();
            base.Dispose();
        }
    }
}
