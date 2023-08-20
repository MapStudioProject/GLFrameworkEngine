using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GLFrameworkEngine
{
    public class UniformBlock : GLObject
    {
        /// <summary>
        /// The usage for handling data for the uniform block.
        /// </summary>
        public BufferUsageHint BufferUsageHint = BufferUsageHint.DynamicDraw;

        /// <summary>
        /// A simple creator for setting up an array of bytes from primitive types.
        /// </summary>
        public BufferCreator BufferCreator = new BufferCreator();

        /// <summary>
        /// The uniform bind index.
        /// </summary>
        private int Index = -1;

        public UniformBlock() : base(GL.GenBuffer())
        {
        }

        public UniformBlock(int index) : base(GL.GenBuffer())
        {
            Index = index;
        }

        /// <summary>
        /// Binds the uniform buffer.
        /// </summary>
        public void Bind()
        { 
            GL.BindBuffer(BufferTarget.UniformBuffer, ID);
        }

        public void SetLabel(string label)
        {
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, ID, label.Length, label);
        }

        /// <summary>
        /// Binds the uniform block to the shader program for rendering via block name.
        /// </summary>
        public void Render(int programID, string name, int binding = -1)
        {
            var index = GL.GetUniformBlockIndex(programID, name);
            Render(programID, index, binding);
        }

        /// <summary>
        /// Binds the uniform block to the shader program for rendering via block index.
        /// </summary>
        public void Render(int programID, int binding = -1)
        {
            if (Index == -1)
                return;

            binding = binding != -1 ? binding : Index;

            GL.UniformBlockBinding(programID, Index, binding);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, binding, ID);
        }

        /// <summary>
        /// Binds the uniform block to the shader program for rendering via block index.
        /// </summary>
        public void Render(int programID, int index, int binding = -1)
        {
            if (index == -1)
                return;

            binding = binding != -1 ? binding : index;

            GL.UniformBlockBinding(programID, index, binding);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, binding, ID);
        }

        public void SetData(byte[] data)
        {
            Bind();
            GL.BufferData(BufferTarget.UniformBuffer, data.Length, data, this.BufferUsageHint);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }


        public void SetData(IntPtr data, int size)
        {
            Bind();
            GL.BufferData(BufferTarget.UniformBuffer, size, data, this.BufferUsageHint);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        public void SetData(byte[] data, int offset, int size)
        {
            Bind();
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset, size, data);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        public void SetData<T>(T data) where T : struct
        {
            Bind();
            GL.BufferData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(T)), ref data, this.BufferUsageHint);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        public void SetData<T>(T[] data) where T : struct
        {
            Bind();
            GL.BufferData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(T)) * data.Length, data, this.BufferUsageHint);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        public void SetData<T>(T data, int offset, int size) where T : struct
        {
            Bind();
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset, size, ref data);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        public void SetData(ref Matrix4 data, int offset, int size)
        {
            var mat1 = data.Column0;
            var mat2 = data.Column1;
            var mat3 = data.Column2;
            var mat4 = data.Column3;

            Bind();
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset + 0, 16, ref mat1);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset + 16, 16, ref mat2);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset + 32, 16, ref mat3);
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset + 48, 16, ref mat4);

            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        public void Init(int size) => SetData(new byte[size]);

        /// <summary>
        /// Updates the buffer with the data created from buffer creator.
        /// </summary>
        public void UpdateBufferCreatorData()
        {
            if (ID == -1)
                return;

            //Bind the data
            var buffer = BufferCreator.Buffer.ToArray();

            Bind();
            GL.BufferData(BufferTarget.UniformBuffer, buffer.Length, buffer, BufferUsageHint);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        /// <summary>
        /// Disposes the uniform block.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteBuffer(ID);
            BufferCreator.Clear();
        }
    }
}
