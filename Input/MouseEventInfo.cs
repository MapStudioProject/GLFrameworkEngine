using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;

namespace GLFrameworkEngine
{
    public class MouseEventInfo
    {
        public static MouseEventInfo State = new MouseEventInfo();

        public static bool CursorHiddenMode = false;

        public bool HasValue { get; set; } = true;

        public int X => Position.X;
        public int Y => Position.Y;

        public Point Position { get; set; }

        public static Vector2 FullPosition { get; set; }
        public static Vector2 PreviousPosition { get; set; }

        public ButtonState RightButton { get; set; } = (ButtonState)3;
        public ButtonState LeftButton { get; set; } = (ButtonState)3;
        public ButtonState MiddleButton { get; set; } = (ButtonState)3;

        public float Delta { get; set; }

        public float WheelPrecise { get; set; }

        public static Cursor MouseCursor = Cursor.Arrow;

        public enum Cursor
        {
            Arrow,
            Eraser,
            EyeDropper,
            ResizeEW,
            None,
        }
    }
}
