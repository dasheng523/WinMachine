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
        public virtual void Update(double stateValue) { }
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

        public override void Update(double pos)
        {
            // 根据设备状态更新局部变换
            if (IsRotary)
            {
                LocalRotation = (float)pos;
            }
            else
            {
                if (IsVertical) LocalY = (float)pos;
                else LocalX = (float)pos;
            }
        }

        protected override void Draw(Graphics g)
        {
            // 绘制轴本身的样子（可选，通常只是为了调试或者显示导轨）
            // 如果是旋转台，可能画个圆
            using var pen = new Pen(Color.Gray, 1);
            if (IsRotary)
            {
                float r = Width; // Use Width as radius
                g.DrawEllipse(pen, -r, -r, r * 2, r * 2);
                // 画个十字指示方向
                g.DrawLine(pen, 0, 0, r, 0); 
            }
            else
            {
               // 只是一个虚拟的导轨节点，通常看不见，看见的是滑块（子节点）
               // 但在这里，AxisNode 本身就是运动部分？
               // 不，通常 Axis 是静止的导轨，Slider 是动的。
               // 在我们的简化模型里，AxisNode 代表 *那个会动的部件* (如旋转台面)。
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
}
