using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLFrameworkEngine
{
    /// <summary>
    /// Represents a base for calculating data like animations.
    /// </summary>
    public class ActorBase
    {
        public virtual string Name { get; set; } = "";

        public int CreateIdx;
        public int Age;

        public bool UpdateCalc = false;
        public virtual void Init()
        {

        }

        public virtual void BeginFrame()
        {

        }

        public virtual void Draw(GLContext context)
        {

        }

        public virtual void Calc()
        {
        }

        public virtual void Dispose()
        {
        }

        public override string ToString() => Name;
    }
}
