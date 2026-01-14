using System;
using System.Windows.Forms;
using Machine.Framework.Devices.Motion.Implementations.Simulator;

namespace WinMachine;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new SimulatorDemoForm());
    }
}
