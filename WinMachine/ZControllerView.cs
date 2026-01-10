using System;
using System.Drawing;
using System.Windows.Forms;
using Devices.Motion.Abstractions;
using Common.Core;
using LanguageExt;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace WinMachine
{
    public partial class ZControllerView : Form
    {
        private readonly IMotionController<ushort, ushort, ushort> _motion;
        private const ushort TARGET_AXIS = 0;

        public ZControllerView(IMotionController<ushort, ushort, ushort> motion)
        {
            _motion = motion;
            InitializeComponent();
        }

        private void OnJogMouseDown(MotionDirection dir)
        {
            var flow =
                from _ in _motion.SetSpeed(TARGET_AXIS, new AxisSpeed(100, 500, 0.1, 0.1, 100, 0.05))
                from __ in _motion.Move_JOG(TARGET_AXIS, dir)
                select unit;

            _ = flow.Match(
                Succ: _ => unit,
                Fail: err =>
                {
                    MessageBox.Show($"JOG 失败: {err.Message}");
                    return unit;
                });
        }

        private void OnJogMouseUp()
        {
            _ = _motion.Stop(TARGET_AXIS).Match(
                Succ: _ => unit,
                Fail: err =>
                {
                    MessageBox.Show($"Stop 失败: {err.Message}");
                    return unit;
                });
        }

        private void OnStopClick()
        {
            _ = _motion.Stop(TARGET_AXIS).Match(
                Succ: _ => unit,
                Fail: err =>
                {
                    MessageBox.Show($"Stop 失败: {err.Message}");
                    return unit;
                });
        }

        private void OnToggleOutput()
        {
            ushort bit = (ushort)numOutputIndex.Value;

            var flow =
                from current in _motion.GetOutput(bit)
                let next = (current == Level.On) ? Level.Off : Level.On
                from _ in _motion.SetOutput(bit, next)
                select unit;

            _ = flow.Match(
                Succ: _ => unit,
                Fail: err =>
                {
                    MessageBox.Show($"切换输出失败: {err.Message}");
                    return unit;
                });
        }

        private void OnTimerTick()
        {
            _ = _motion.GetCommandPos(TARGET_AXIS).Match(
                Succ: pos =>
                {
                    lblPosition.Text = pos.ToString("F3");
                    return unit;
                },
                Fail: _ => unit);

            ushort inputBit = (ushort)numInputIndex.Value;
            _ = _motion.GetInput(inputBit).Match(
                Succ: inLevel =>
                {
                    lblInStatus.Text = inLevel == Level.On ? "状态: 高电平 [ON]" : "状态: 低电平 [OFF]";
                    lblInStatus.ForeColor = inLevel == Level.On ? Color.Red : Color.Black;
                    return unit;
                },
                Fail: _ => unit);
        }
    }
}
