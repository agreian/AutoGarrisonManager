using System.Windows.Forms;

namespace AutoGarrisonMissions.InputSimulator
{
    public static class KeyboardInputSimulator
    {
        #region Public Methods

        public static void SimulateSpace()
        {
            SendKeys.SendWait(" ");
        }

        #endregion
    }
}