using System;
using System.Runtime.InteropServices;

namespace Server.Exit
{
    public class WinExitSignal
    {
        public event EventHandler Exit;

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes ctrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        /// <summary>
        /// Need this as a member variable to avoid it being garbage collected.
        /// </summary>
        private readonly HandlerRoutine m_hr;

        public WinExitSignal()
        {
            m_hr = ConsoleCtrlCheck;

            SetConsoleCtrlHandler(m_hr, true);
        }

        /// <summary>
        /// Handle the ctrl types
        /// </summary>
        /// <param name="ctrlType"></param>
        /// <returns></returns>
        private bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                case CtrlTypes.CTRL_BREAK_EVENT:
                case CtrlTypes.CTRL_CLOSE_EVENT:
                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    Exit?.Invoke(this, EventArgs.Empty);
                    break;
            }

            return true;
        }


    }
}
