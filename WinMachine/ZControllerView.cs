using System;
using System.Drawing;
using System.Windows.Forms;
using Devices.Motion.Abstractions;
using Common.Core;
using LanguageExt;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;
using WinMachine.Services;

namespace WinMachine
{
    public partial class ZControllerView : Form
    {
        private readonly IMotionController<ushort, ushort, ushort> _motion;
        private readonly ushort _targetAxis;

        public ZControllerView(IMotionSystem motionSystem, IAxisResolver axisResolver)
        {
            _motion = motionSystem.Primary;
            _targetAxis = axisResolver.ResolveOnPrimary("Z1").Match(
                Succ: a => a,
                Fail: _ => (ushort)0);
            InitializeComponent();
        }

        public ZControllerView(IMotionSystem motionSystem)
        {
            _motion = motionSystem.Primary;
            _targetAxis = (ushort)0;
            InitializeComponent();
        }

        private void OnJogMouseDown(MotionDirection dir)
        {
            var flow =
                from _ in _motion.SetSpeed(_targetAxis, new AxisSpeed(100, 500, 0.1, 0.1, 100, 0.05))
                from __ in _motion.Move_JOG(_targetAxis, dir)
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
            _ = _motion.Stop(_targetAxis).Match(
                Succ: _ => unit,
                Fail: err =>
                {
                    MessageBox.Show($"Stop 失败: {err.Message}");
                    return unit;
                });
        }

        private void OnStopClick()
        {
            _ = _motion.Stop(_targetAxis).Match(
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
            _ = _motion.GetCommandPos(_targetAxis).Match(
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
