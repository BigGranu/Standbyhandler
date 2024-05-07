using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Standbyhandler
{
    public partial class Form1 : Form
    {
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

            _menuItemStart.Text = @"Start";
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
}
