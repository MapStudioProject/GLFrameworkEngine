using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace GLFrameworkEngine
{
    [Serializable]
    public class AssetBase
    {
        public virtual string Name { get; set; }

        [NonSerialized]
        public EventHandler ThumbnailUpdated;

        public virtual void Draw(GLContext control)
        {

        }
    }
}
