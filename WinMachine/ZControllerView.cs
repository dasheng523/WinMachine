using System;
using System.Windows.Forms;
using Devices.Motion.Abstractions;
using Devices.Shared;

namespace WinMachine
{
    public partial class ZControllerView : Form
    {
        private readonly IMotionController<int, int, int> _motion;
        private const int TARGET_AXIS = 0;

        public ZControllerView(IMotionController<int, int, int> motion)
        {
            _motion = motion;
            InitializeComponent();
        }

        private void OnJogMouseDown(MotionDirection dir)
        {
            try
            {
                // 先设置一个默认速度，如果没设置过的话
                _motion.SetSpeed(TARGET_AXIS, new AxisSpeed(100, 500, 0.1, 0.1, 100, 0.05));
                _motion.Move_JOG(TARGET_AXIS, dir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"JOG 失败: {ex.Message}");
            }
        }

        private void OnJogMouseUp()
        {
            _motion.Stop(TARGET_AXIS);
        }

        private void OnStopClick()
        {
            _motion.Stop(TARGET_AXIS);
        }

        private void OnToggleOutput()
        {
            int bit = (int)numOutputIndex.Value;
            int current = _motion.GetOutput(bit);
            Level next = (current == 1) ? Level.Off : Level.On;
            _motion.SetOutput(bit, next);
        }

        private void OnTimerTick()
        {
            try
            {
                // 刷新坐标
                double pos = _motion.GetCommandPos(TARGET_AXIS);
                lblPosition.Text = pos.ToString("F3");

                // 刷新输入状态
                int inputBit = (int)numInputIndex.Value;
                Level inLevel = _motion.GetInput(inputBit);
                lblInStatus.Text = inLevel == Level.On ? "状态: 高电平 [ON]" : "状态: 低电平 [OFF]";
                lblInStatus.ForeColor = inLevel == Level.On ? Color.Red : Color.Black;
            }
            catch
            {
                // 忽略刷新异常
            }
        }
    }
}
