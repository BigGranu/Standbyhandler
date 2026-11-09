using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Standbyhandler
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer timer;
        private MenuItem display;
        private Point oldPosition;

        private readonly ToolStripMenuItem _menuItemStart;
        private readonly ToolStripMenuItem _menuItemExit;
        private readonly ToolStripMenuItem _display;

        private bool _sleep;

        public Form1()
        {
            InitializeComponent();

            var contextMenu = new ContextMenuStrip();
            _menuItemStart = new ToolStripMenuItem();
            _menuItemExit = new ToolStripMenuItem();
            _display = new ToolStripMenuItem();

            _sleep = true;
            _menuItemStart.Text = @"Stop";
            _menuItemStart.ToolTipText = @"Pc allways On";
            _menuItemStart.Click += menuItem1_Click;

            _menuItemExit.Text = @"E&xit";
            _menuItemExit.Click += menuItem3_Click;

            _display.Text = @"Need Display";
            _display.Checked = true;
            _display.Click += DisplayOnClick;
            _display.ToolTipText = @"Enable or Disable the Display if Sleep prohibited";

            contextMenu.Items.Insert(0, _menuItemStart);
            contextMenu.Items.Insert(1, _menuItemExit);
            contextMenu.Items.Insert(2, _display);

            notifyIcon1.ContextMenuStrip = contextMenu;
            notifyIcon1.Text = @"Standbyhandler";
            notifyIcon1.Visible = true;

            // Initialize Timer
            timer = new System.Windows.Forms.Timer();
            timer.Tick += TimerOnTick;
            timer.Interval = 10000;
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            if (!_sleep)
            {
                _menuItemStart.Text = @"Stop";
                _menuItemStart.ToolTipText = @"Pc can sleep";
                NativeMethods.ProhibitSleep(_display.Checked);
                _sleep = true;
            }
            else
            {
                _menuItemStart.Text = @"Start";
                _menuItemStart.ToolTipText = @"Pc allways On";
                NativeMethods.AllowSleep();
                _sleep = false;
            }
        }

        private void menuItem3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void DisplayOnClick(object sender, EventArgs e)
        {
            _display.Checked = !_display.Checked;
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            var fpi = Win32Input.GetCursorPosition();

            if (fpi == oldPosition)
            {
                var ret = Win32Input.Move(fpi.X + 1, fpi.Y + 1);
                System.Threading.Thread.Sleep(500);
                ret = Win32Input.Move(fpi.X, fpi.Y);
            }

            oldPosition = fpi;
        }
    }

    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern uint SetThreadExecutionState(ExecutionState esFlags);

        [Flags]
        public enum ExecutionState : uint
        {
            EsContinuous = 0x80000000,
            EsDisplayRequired = 0x00000002,
            EsSystemRequired = 0x00000001
        }

        public static void ProhibitSleep(bool display)
        {
            uint result;

            if (display)
            {
                result =
                    SetThreadExecutionState(ExecutionState.EsContinuous |
                                                          ExecutionState.EsSystemRequired |
                                                          ExecutionState.EsDisplayRequired);
            }
            else
            {
                result =
                    SetThreadExecutionState(ExecutionState.EsContinuous |
                                                          ExecutionState.EsSystemRequired);
            }

            if (result == 0)
            {
                MessageBox.Show(@"Error Prohibit Sleep:" + result, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void AllowSleep()
        {
            var result = SetThreadExecutionState(ExecutionState.EsContinuous);
            if (result == 0)
            {
                MessageBox.Show(@"Error Allow Sleep:" + result, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class Win32Input
    {
        [DllImport("user32.dll", EntryPoint = "SendInput", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", EntryPoint = "GetMessageExtraInfo", SetLastError = true)]
        static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out PointInter lpPoint);

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        static extern int SetCursorPos(int x, int y);

        private enum InputType
        {
            INPUT_MOUSE = 0
        }

        [Flags]
        private enum MOUSEEVENTF
        {
            MOVE = 0x0001,
            ABSOLUTE = 0x8000,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int mouseData;
            public MOUSEEVENTF dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public InputType type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PointInter
        {
            public int X;
            public int Y;
            public static explicit operator Point(PointInter point) => new Point(point.X, point.Y);
        }

        public static Point GetCursorPosition()
        {
            GetCursorPos(out var lpPoint);
            return (Point)lpPoint;
        }

        public static int Move(int x, int y)
        {
            INPUT inputMove = new INPUT
            {
                type = InputType.INPUT_MOUSE
            };
            inputMove.mi.mouseData = 0;
            inputMove.mi.dwFlags = (MOUSEEVENTF.MOVE | MOUSEEVENTF.ABSOLUTE);
            inputMove.mi.time = 0;
            inputMove.mi.dwExtraInfo = GetMessageExtraInfo();

            INPUT[] input = { inputMove };
            SendInput(1, input, Marshal.SizeOf(inputMove));

            return SetCursorPos(x, y);
        }
    }
}
