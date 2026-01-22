using System;
using System.Drawing;
using System.Windows.Forms;
using Machine.Framework.Visualization.SceneGraph;

namespace Machine.Framework.Visualization.WinForms
{
    public class SceneGraphCanvas : Control
    {
        private SceneNode? _root;
        // 变换矩阵，用于支持平移/缩放视图 (Pan/Zoom)
        private System.Drawing.Drawing2D.Matrix _viewMatrix = new();

        public SceneGraphCanvas()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.BackColor = Color.FromArgb(30, 30, 30); // Dark theme background
        }

        public void SetRoot(SceneNode root)
        {
            _root = root;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (_root == null) return;

            // 应用视图变换 (为了让 (0,0) 在控件中心，或者跟随 Panel 的逻辑)
            // 简单起见，我们先将 (0,0) 设为左上角，或者稍微偏移一点以便看到负坐标
            g.TranslateTransform(50, 50); 
            // 如果需要 Pan/Zoom，可以在这里 MultiplyTransform(_viewMatrix)

            // 开始递归渲染
            // 初始矩阵为 Identity (或者当前的 g.Transform)
            _root.Render(g, g.Transform);
        }
    }
}
