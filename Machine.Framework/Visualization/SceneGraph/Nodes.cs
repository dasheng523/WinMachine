using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Machine.Framework.Core.Primitives;

namespace Machine.Framework.Visualization.SceneGraph
{
    // --- 核心节点抽象 ---
    public abstract class SceneNode
    {
        public string Name { get; set; } = string.Empty;
        public SceneNode? Parent { get; set; }
        public List<SceneNode> Children { get; } = new();

        // 局部变换 (相对于父级)
        public float LocalX { get; set; }
        public float LocalY { get; set; }
        public float LocalRotation { get; set; } // 角度
        public float ScaleX { get; set; } = 1.0f;
        public float ScaleY { get; set; } = 1.0f;

        // 绑定设备 (用于从 Interpreter 获取状态)
        public DeviceID? BoundDeviceId { get; set; }
        public double CurrentValue { get; set; }
        public Color? HighlightColor { get; set; }

        public void AddChild(SceneNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public void Render(Graphics g, Matrix parentTransform)
        {
            // 1. 复制父级变换矩阵
            using var currentTransform = parentTransform.Clone();

            // 2. 应用局部变换 (顺序：平移 -> 旋转 -> 缩放)
            currentTransform.Translate(LocalX, LocalY);
            currentTransform.Rotate(LocalRotation);
            currentTransform.Scale(ScaleX, ScaleY);

            // 3. 应用到 Graphics 上下文
            // 注意：因为我们要递归，所以不能直接修改 g.Transform 
            // 而是通过传递累积矩阵的方式，或者 Save/Restore GraphicsState
            var state = g.Save();
            g.Transform = currentTransform;

            // 4. 绘制自身
            Draw(g);

            // 5. 递归绘制子节点
            foreach (var child in Children)
            {
                // 注意：由于我们已经设置了 g.Transform 为 currentTransform，
                // 子节点只需要基于这个变换再进行叠加即可。
                // 但为了保持算法通用性，我们这里传递 currentTransform 下去
                child.Render(g, currentTransform);
            }

            // 6. 恢复上下文
            g.Restore(state);
        }

        protected virtual void Draw(Graphics g) { }

        // --- 状态更新 ---
        public virtual void Update(double stateValue) { CurrentValue = stateValue; }
    }

    // --- 容器节点 (Group) ---
    public class GroupNode : SceneNode { }

    // --- 轴节点 (Axis Node) ---
    // 轴节点根据状态改变自身的 LocalX/Y/Rotation
    public class AxisNode : SceneNode
    {
        public bool IsRotary { get; set; }
        public bool IsVertical { get; set; }
        
        // 视觉属性
        public float Length { get; set; } = 100;
        public float Width { get; set; } = 20;
        public float Height { get; set; } = 20; // 滑块尺寸

        // 行程范围：用于将轴位置映射到视觉长度
        public float TravelMin { get; set; } = 0;
        public float TravelMax { get; set; } = 100;

        public override void Update(double pos)
        {
            base.Update(pos);
            if (IsRotary)
            {
                LocalRotation = (float)pos;
            }
            else
            {
                // 如果 TravelMax 还是默认值 100，但 Length 已设置且不同，使用 Length 作为有效范围
                float effectiveTravelMax = TravelMax;
                if (TravelMax == 100 && Length > 0 && Length != 100)
                {
                    effectiveTravelMax = Length;
                }
                
                // 将轴位置从 [TravelMin, effectiveTravelMax] 映射到 [0, Length]
                float range = effectiveTravelMax - TravelMin;
                float normalizedPos = range > 0 ? (float)((pos - TravelMin) / range) : 0;
                float visualPos = normalizedPos * Length;
                
                if (IsVertical) LocalY = visualPos;
                else LocalX = visualPos;
            }
        }

        protected override void Draw(Graphics g)
        {
            if (IsRotary)
            {
                SpriteDraw.DrawRotaryAxis(g, Width);
            }
            else
            {
               float w = Width > 0 ? Width : 40;
               float h = Height > 0 ? Height : 40;
               SpriteDraw.DrawLinearAxis(g, w, h);
            }
        }
    }

    // --- 视觉元素节点 (Sprite) ---
    // 这是一个附着在某个运动节点上的具体的“样子”
    public class SpriteNode : SceneNode
    {
        public float Width { get; set; } = 20;
        public float Height { get; set; } = 20;
        public float PivotX { get; set; } = 0.5f; // 0~1
        public float PivotY { get; set; } = 0.5f; // 0~1
        
        public Color Color { get; set; } = Color.Blue;
        public Action<Graphics, float, float>? CustomDraw { get; set; }

        protected override void Draw(Graphics g)
        {
            // 计算绘制原点偏移
            float dx = -Width * PivotX;
            float dy = -Height * PivotY;

            if (CustomDraw != null)
            {
                // 转换坐标系让 (0,0) 对应 Pivot 点
                g.TranslateTransform(dx, dy); 
                CustomDraw(g, Width, Height);
                g.TranslateTransform(-dx, -dy);
            }
            else
            {
                using var brush = new SolidBrush(Color);
                g.FillRectangle(brush, dx, dy, Width, Height);
                using var border = new Pen(Color.Black, 1);
                g.DrawRectangle(border, dx, dy, Width, Height);
            }
        }
    }

    /// <summary>
    /// 线性行程驱动节点：根据 CurrentValue(0..100) 将自身在 X/Y 方向偏移，用于气缸滑台/升降台等输出端随行程移动。
    /// </summary>
    public sealed class StrokeNode : SceneNode
    {
        public bool IsVertical { get; set; }
        public bool IsReversed { get; set; }
        public float Stroke { get; set; }

        public float BaseX { get; set; }
        public float BaseY { get; set; }

        public override void Update(double stateValue)
        {
            base.Update(stateValue);

            var t = Math.Clamp(stateValue / 100.0, 0.0, 1.0);
            if (IsReversed) t = 1.0 - t;

            var delta = (float)(Stroke * t);
            LocalX = BaseX + (IsVertical ? 0 : delta);
            LocalY = BaseY + (IsVertical ? delta : 0);
        }
    }
}
