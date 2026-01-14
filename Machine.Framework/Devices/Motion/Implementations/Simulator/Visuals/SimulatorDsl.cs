using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Machine.Framework.Devices.Motion.Implementations.Simulator.Models;
using Machine.Framework.Devices.Motion.Implementations.Simulator.Rendering;
using System.Reactive.Linq;
using System.Threading;

namespace Machine.Framework.Devices.Motion.Implementations.Simulator.Visuals
{
    public static class SimulatorDslExtensions
    {
        /// <summary>
        /// 将 Panel 渲染为仿真轴
        /// </summary>
        public static void RenderAxis(this Panel panel, ISimulatorAxis axis, Action<AxisVisualConfig> configAction)
        {
            // 1. 配置
            var config = new AxisVisualConfig();
            configAction(config);

            // 2. 注入渲染器
            // 清理旧控件 (防止重复调用)
            foreach (Control c in panel.Controls)
            {
                if (c is AxisRendererControl) 
                {
                    panel.Controls.Remove(c);
                    c.Dispose();
                }
            }

            var renderer = new AxisRendererControl(axis, config);
            renderer.Dock = DockStyle.Fill;
            // 将渲染器置于底层，让其他子控件(如果有)浮在上面
            panel.Controls.Add(renderer);
            panel.Controls.SetChildIndex(renderer, panel.Controls.Count - 1); 

            // 3. 处理硬件联动 (Attachments)
            // 当轴移动时，带动子Panel移动
            if (config.ChildAttachments.Count > 0)
            {
                var syncContext = SynchronizationContext.Current;
                
                axis.StateStream
                    .Sample(TimeSpan.FromMilliseconds(33))
                    .ObserveOn(syncContext ?? SynchronizationContext.Current)
                    .Subscribe(state =>
                    {
                        // 计算滑块在 Panel 内的像素位置
                        float pixelPos = CalculateSliderPosition(state.Position, axis, panel.Size, config.Direction);
                        
                        foreach (var att in config.ChildAttachments)
                        {
                            UpdateAttachmentPosition(att, pixelPos, panel, config.Direction);
                        }
                    });
            }
        }

        private static float CalculateSliderPosition(double logicalPos, ISimulatorAxis axis, Size size, FlowDirection dir)
        {
            double range = axis.TravelMax - axis.TravelMin;
            if (range <= 0) range = 100;
            double ratio = (logicalPos - axis.TravelMin) / range;
            
            // 限制范围
            if (ratio < 0) ratio = 0; 
            if (ratio > 1) ratio = 1;

            // 简化的几何计算，需与 Renderer 保持一致
            // 假设滑块中心点
            if (dir == FlowDirection.TopDown || dir == FlowDirection.BottomUp)
            {
                float availableH = size.Height - 40; // 假设 padding
                if (dir == FlowDirection.TopDown)
                    return (float)(10 + ratio * availableH + 10); // +10 offset for center
                else
                    return (float)(10 + (1 - ratio) * availableH + 10);
            }
            else
            {
                float availableW = size.Width - 50; // slider width 30 + padding
                if (dir == FlowDirection.LeftToRight)
                    return (float)(10 + ratio * availableW + 15);
                else
                    return (float)(10 + (1 - ratio) * availableW + 15);
            }
        }

        private static void UpdateAttachmentPosition(AttachmentConfig att, float sliderPixelPos, Panel parent, FlowDirection dir)
        {
            // 简单联动：修改子 Panel 的 Top 或 Left
            // 假设 ChildPanel 的父容器就是 parent (pnl_Z1)
            // 或者是 Form 上的兄弟节点? 
            // 如果是兄弟节点，坐标变换会很复杂。
            // 这里假设 ChildPanel 被拖进去放在 Parent Panel 里面了 (WinForms Designer 支持)
            
            if (att.ChildPanel.Parent != parent)
            {
                // 如果不是父子关系，暂不支持（或需要屏幕坐标变换）
                // 简单处理：尝试设为父子? 不，这会破坏 Designer 布局
                return;
            }

            if (dir == FlowDirection.TopDown || dir == FlowDirection.BottomUp)
            {
                att.ChildPanel.Top = (int)(sliderPixelPos + att.OffsetY);
                att.ChildPanel.Left = (int)(parent.Width / 2 - att.ChildPanel.Width / 2 + att.OffsetX);
            }
            else
            {
                att.ChildPanel.Left = (int)(sliderPixelPos + att.OffsetX);
                att.ChildPanel.Top = (int)(parent.Height / 2 - att.ChildPanel.Height / 2 + att.OffsetY);
            }
        }
    }

    // --- DSL 配置对象 ---

    public class AxisVisualConfig
    {
        public FlowDirection Direction { get; set; } = FlowDirection.LeftToRight;
        public List<AttachmentConfig> ChildAttachments { get; } = new();

        public AxisVisualConfig Layout(Action<AxisLayoutConfig> action)
        {
            var l = new AxisLayoutConfig();
            action(l);
            this.Direction = l.Direction;
            return this;
        }

        public AxisVisualConfig AttachChild(Panel child, Action<AttachmentConfig> attachAction = null)
        {
            var att = new AttachmentConfig { ChildPanel = child };
            attachAction?.Invoke(att);
            ChildAttachments.Add(att);
            return this;
        }
    }

    public class AxisLayoutConfig
    {
        public FlowDirection Direction { get; set; } = FlowDirection.LeftToRight;
        
        public void AutoFit(int padding = 0) { } // Placeholder
    }

    public class AttachmentConfig
    {
        public Panel ChildPanel { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        public void PhysicalOffset(int x, int y)
        {
            OffsetX = x;
            OffsetY = y;
        }
    }
}
