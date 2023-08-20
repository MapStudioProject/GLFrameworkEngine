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
    public class StorageBuffer : GLObject
    {
        /// <summary>
        /// The usage for handling data for the uniform block.
        /// </summary>
        public BufferUsageHint BufferUsageHint = BufferUsageHint.DynamicDraw;

        /// <summary>
        /// 
        /// </summary>
        public BufferTarget Target = BufferTarget.ShaderStorageBuffer;

        /// <summary>
        /// A simple creator for setting up an array of bytes from primitive types.
        /// </summary>
        public BufferCreator BufferCreator = new BufferCreator();

        public int Offset;

        public int Size;

        public StorageBuffer() : base(GL.GenBuffer())
        {
        }

        /// <summary>
        /// Binds the uniform buffer.
        /// </summary>
        public void Bind()
        { 
            GL.BindBuffer(Target, ID);
        }

        public void SetLabel(string label)
        {
            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, ID, label.Length, label);
        }

        /// <summary>
        /// Binds the uniform block to the shader program for rendering via block name.
        /// </summary>
        public void Render(int programID, string name, int index, int binding = -1)
        {
            var handle = GL.GetProgramResourceIndex(programID, ProgramInterface.ShaderStorageBlock, name);

            GL.ShaderStorageBlockBinding(programID, handle, binding);
            GL.BindBufferRange((BufferRangeTarget)Target, binding, ID, (IntPtr)Offset, Size);
        }

        /// <summary>
        /// Binds the uniform block to the shader program for rendering via block index.
        /// </summary>
        public void Render(int programID, int index, int binding = -1)
        {
            if (index == -1)
                return;

            binding = binding != -1 ? binding : index;

            GL.ShaderStorageBlockBinding(programID, index, binding);
            GL.BindBufferRange((BufferRangeTarget)Target, index, ID, (IntPtr)Offset, Size);
        }

        public void SetData(byte[] data)
        {
            Bind();
            GL.BufferData(Target, data.Length, data, this.BufferUsageHint);
            GL.BindBuffer(Target, 0);

            Offset = 0;
            Size = data.Length;
        }

        public void SetData(byte[] data, int offset, int size)
        {
            Bind();
            GL.BufferSubData(Target, (IntPtr)offset, size, data);
            GL.BindBuffer(Target, 0);

            Offset = offset;
            Size = size;
        }

        public void SetData<T>(T data) where T : struct
        {
            Bind();
            GL.BufferData(Target, Marshal.SizeOf(typeof(T)), ref data, this.BufferUsageHint);
            GL.BindBuffer(Target, 0);

            Offset = 0;
            Size = Marshal.SizeOf(typeof(T));
        }

        public void SetData<T>(T data, int offset, int size) where T : struct
        {
            Bind();
            GL.BufferSubData(Target, (IntPtr)offset, size, ref data);
            GL.BindBuffer(Target, 0);
        }

        /// <summary>
        /// Updates the buffer with the data created from buffer creator.
        /// </summary>
        public void UpdateBufferData()
        {
            if (ID == -1)
                return;

            //Bind the data
            var buffer = BufferCreator.Buffer.ToArray();
            Size = buffer.Length;

            Bind();
            GL.BufferData(Target, buffer.Length, buffer, BufferUsageHint);
            GL.BindBuffer(Target, 0);
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
