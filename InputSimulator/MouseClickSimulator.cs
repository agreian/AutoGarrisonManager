using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AutoGarrisonMissions.InputSimulator
{
    public static class MouseClickSimulator
    {
        #region Fields

        private const uint MOUSE_LEFT_DOWN = 0x02;
        private const uint MOUSE_LEFT_UP = 0x04;
        private const uint MOUSE_RIGHT_DOWN = 0x08;
        private const uint MOUSE_RIGHT_UP = 0x10;

        #endregion

        #region Public Methods

        public static void SimulateDoubleClick(int x, int y)
        {
            SimulateLeftClick(x, y);
            Thread.Sleep(50);
            SimulateLeftClick(x, y);
        }

        public static void SimulateLeftClick(int x, int y)
        {
            Simulate(MOUSE_LEFT_DOWN, MOUSE_LEFT_UP, x, y);
        }

        public static void SimulateRightClick(int x, int y)
        {
            Simulate(MOUSE_RIGHT_DOWN, MOUSE_RIGHT_UP, x, y);
        }

        #endregion

        #region Private Methods

        private static void Simulate(uint dwFlags1, uint dwFlags2, int intX, int intY)
        {
            var x = Convert.ToUInt32(intX);
            var y = Convert.ToUInt32(intY);

            Cursor.Position = new System.Drawing.Point(Convert.ToInt32(x), Convert.ToInt32(y));

            mouse_event(dwFlags1, x, y, 0, 0);
            Thread.Sleep(20);
            mouse_event(dwFlags2, x, y, 0, 0);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        #endregion
    }
}