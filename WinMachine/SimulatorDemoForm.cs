using System;
using System.Drawing;
using System.Windows.Forms;
using Machine.Framework.Core.Hardware.Interfaces;
using Machine.Framework.Core.Hardware.Models;
using Machine.Framework.Devices.Implementations.Simulator;
using Machine.Framework.Devices.Implementations.Simulator.Visuals;

namespace WinMachine
{
    // Demo Types
    public enum DemoAxis { X, Z }
    public enum DemoIO { PenA }

    public partial class SimulatorDemoForm : Form
    {

        public SimulatorDemoForm()
        {
            InitializeComponent();
            
        }

    }
}
