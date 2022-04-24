using System;
using System.Collections.Generic;
using System.Text;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents an instance capable of creating UI layouts from provided properties.
    /// </summary>
    public class UILayout
    {
        public void Prop(string label, object obj, string propertyName)
        {

        }

        public void Label()
        {

        }

        public class Column
        {

        }

        public class Property
        {
            public string Name;
            public object Object;

            public string Category;
        }

        public class FloatProperty
        {
            public float Value;
            public float Min = float.MinValue;
            public float Max = float.MaxValue;
        }

        public class UintProperty
        {
            public uint Value;
            public uint Min = uint.MinValue;
            public uint Max = uint.MaxValue;
        }

        public class IntProperty
        {
            public int Value;
            public int Min = int.MinValue;
            public int Max = int.MaxValue;
        }

        public class BoolProperty
        {
            public bool Value;
        }
    }
}
