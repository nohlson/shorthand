using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Shorthand.Windows
{
    public class GlobalHotkey : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private int currentId;
        private bool disposed = false;

        public event EventHandler? HotkeyPressed;

        public GlobalHotkey()
        {
            currentId = 0;
        }

        public bool RegisterHotkey(Keys key, ModifierKeys modifiers)
        {
            UnregisterHotkey();
            currentId = unchecked(DateTime.Now.Ticks.GetHashCode());
            return RegisterHotKey(IntPtr.Zero, currentId, (uint)modifiers, (uint)key);
        }

        public void UnregisterHotkey()
        {
            if (currentId != 0)
            {
                UnregisterHotKey(IntPtr.Zero, currentId);
                currentId = 0;
            }
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == currentId)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    UnregisterHotkey();
                }
                disposed = true;
            }
        }

        #region Win32 API

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        #endregion
    }

    [Flags]
    public enum ModifierKeys : uint
    {
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Windows = 0x0008,
        NoRepeat = 0x4000
    }
}
