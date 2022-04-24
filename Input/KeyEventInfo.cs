using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;

namespace GLFrameworkEngine
{
    public class KeyEventInfo
    {
        public static KeyEventInfo State = new KeyEventInfo();

        public List<string> KeyChars { get; set; } = new List<string>();

        public bool HasKeyDown() => KeyChars.Count > 0;

        public bool KeyShift { get; set; }
        public bool KeyCtrl { get; set; }
        public bool KeyAlt { get; set; }

        public bool IsKeyDown(string key) {
            if (KeyCtrl && key.StartsWith("Ctrl+"))
                return KeyChars.Contains(key.Split("+").Last());
            if (KeyShift && key.StartsWith("Shift+"))
                return KeyChars.Contains(key.Split("+").Last());
            if (KeyAlt && key.StartsWith("Alt+"))
                return KeyChars.Contains(key.Split("+").Last());

            return KeyChars.Contains(key);
        }
    }
}
