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
        private SimulatorMotionController<DemoAxis, int, int> _controller;

        public SimulatorDemoForm()
        {
            InitializeComponent();
            
            // 初始化控制器
            _controller = new SimulatorMotionController<DemoAxis, int, int>();
        }

        private void SimulatorDemoForm_Load(object sender, EventArgs e)
        {
            // 1. 获取轴对象
            // 假设 X轴行程 500mm, 速度 300mm/s
            var axisX = _controller.GetAxis(DemoAxis.X); 
            // 假设 Z轴行程 200mm
            var axisZ = _controller.GetAxis(DemoAxis.Z);

            // 2. DSL 配置: 将 Panel 渲染为硬件
            
            // --> X轴 (水平)
            pnl_X.RenderAxis(axisX, config => config
                .Layout(layout => layout.Direction = FlowDirection.LeftToRight)
            );

            // --> Z轴 (垂直) + 物理挂载 (pen)
            // 当 Z 轴移动时，pnl_Pen 会跟随
            pnl_Z.RenderAxis(axisZ, config => config
                .Layout(layout => layout.Direction = FlowDirection.TopDown)
                .AttachChild(pnl_Pen, att => att.PhysicalOffset(x: -20, y: 0)) // 虽然Pen在Form上是独立的，但RenderAxis会尝试接管它的位置
            );

            // 将 Pen 放到 Z轴 Panel 内部或者合适的位置以便观察联动
            // 由于 WinForms Layout 限制，最好的联动效果通常是 Parent-Child。
            // 我们的 DSL 在 UpdateAttachmentPosition 中处理了简单的 Top/Left 更新。
            // 注意: 为了 Demo 效果，确保 pnl_Pen 在 pnl_Z 旁边，且 Parent 相同 (都是 Form)。
        }

        private void btn_MoveX_Click(object sender, EventArgs e)
        {
            // 移动到 80% (行程 1000, 80% = 800)
            _controller.Move_Absolute(DemoAxis.X, 800);
        }

        private void btn_MoveZ_Click(object sender, EventArgs e)
        {
            // 移动到 50% (500)
            _controller.Move_Absolute(DemoAxis.Z, 500);
        }

        private void btn_Home_Click(object sender, EventArgs e)
        {
            _controller.GoBackHome(DemoAxis.X);
            _controller.GoBackHome(DemoAxis.Z);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _controller.Dispose();
            base.OnFormClosed(e);
        }
    }
}
