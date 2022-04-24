using System;
using System.Collections.Generic;
using System.Text;

namespace GLFrameworkEngine
{
    public class UndoableProperty : IRevertable
    {
        object Object;
        string PropertyName;
        object NewValue;
        object PrevValue;

        public UndoableProperty(object obj, string propertyName, object prevvalue, object newvalue)
        {
            Object = obj;
            PropertyName = propertyName;
            PrevValue = prevvalue;
            NewValue = newvalue;
        }

        public IRevertable Revert()
        {
            SetValue(PrevValue);
            return new UndoableProperty(Object, PropertyName, NewValue, PrevValue);
        }

        private void SetValue(object value) {
            Object.GetType().GetProperty(PropertyName).SetValue(Object, value);
        }
    }
}
