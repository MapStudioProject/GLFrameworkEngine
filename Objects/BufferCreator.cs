using OpenTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace GLFrameworkEngine
{
    public class BufferCreator
    {
        public List<byte> Buffer = new List<byte>();

        public int Size => Buffer.Count * sizeof(byte);

        public void Clear()
        {
            Buffer.Clear();
        }

        public void Add(byte[] value)
        {
            Buffer.AddRange(value);
        }

        public void Add(uint[] value)
        {
            for (int i = 0; i < value.Length; i++)
                Buffer.AddRange(BitConverter.GetBytes(value[i]));
        }

        public void Add(int[] value)
        {
            for (int i = 0; i < value.Length; i++)
                Buffer.AddRange(BitConverter.GetBytes(value[i]));
        }

        public void Add(float[] value)
        {
            for (int i = 0; i < value.Length; i++)
                AddFloat(value[i]);
        }

        public void Add(Vector2[] value)
        {
            for (int i = 0; i < value.Length; i++)
                Add(value[i]);
        }

        public void Add(Vector3[] value)
        {
            for (int i = 0; i < value.Length; i++)
                Add(value[i]);
        }

        public void Add(Vector4[] value)
        {
            for (int i = 0; i < value.Length; i++)
                Add(value[i]);
        }

        public void Add(float value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void AddFloat(float value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Buffer.AddRange(new byte[12]); //Padding
        }

        public void AddInt(int value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
            Buffer.AddRange(new byte[12]); //Padding
        }

        public void Add(Vector2 value)
        {
            Add(value.X);
            Add(value.Y);
        }

        public void Add(Vector3 value)
        {
            Add(value.X);
            Add(value.Y);
            Add(value.Z);
            Buffer.AddRange(new byte[4]); //Buffer aligned so make sure it's 16 bytes size
        }

        public void Add(Vector4 value)
        {
            Add(value.X);
            Add(value.Y);
            Add(value.Z);
            Add(value.W);
        }
    }
}
