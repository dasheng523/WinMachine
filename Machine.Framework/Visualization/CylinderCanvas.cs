using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Machine.Framework.Visualization;

internal sealed class CylinderCanvas : Control
{
    public CylinderCanvas()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = Color.FromArgb(220, 230, 230, 230);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public CylinderVisualStyle Style { get; set; } = CylinderVisualStyle.SlideBlock;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Vertical { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Reversed { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double Value { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double OpenWidth { get; set; } = 18;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double CloseWidth { get; set; } = 6;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double Diameter { get; set; } = 10;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double? BlockSize { get; set; }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Color.FromArgb(24, 24, 24));

        // 获取原始 t (0.0 - 1.0)
        float t = (float)Math.Clamp(Value, 0, 1);
        
        // 处理反向逻辑：如果 Reversed 为 true，则坐标反向映射
        float drawT = Reversed ? 1 - t : t;

        switch (Style)
        {
            case CylinderVisualStyle.Gripper:
                DrawGripper(g, drawT);
                break;
            case CylinderVisualStyle.SuctionPen:
                DrawSuction(g, drawT);
                break;
            default:
                DrawSlideBlock(g, drawT);
                break;
        }
    }

    private void DrawSlideBlock(Graphics g, float t)
    {
        var rect = ClientRectangle;
        var padding = 10f;
        var size = (float)(BlockSize ?? (Vertical ? rect.Width : rect.Height) * 0.7f);

        if (Vertical)
        {
            // 垂直轨道
            float trackWidth = 4f;
            float trackX = rect.Width / 2f - trackWidth / 2f;
            using var trackBrush = new SolidBrush(Color.FromArgb(60, 60, 60));
            g.FillRectangle(trackBrush, trackX, padding, trackWidth, rect.Height - padding * 2);

            float startY = padding;
            float endY = rect.Height - padding - size;
            float currentY = startY + (endY - startY) * t;

            var blockRect = new RectangleF(rect.Width / 2f - size / 2f, currentY, size, size);
            DrawBlock(g, blockRect, t);
        }
        else
        {
            // 水平轨道
            float trackHeight = 4f;
            float trackY = rect.Height / 2f - trackHeight / 2f;
            using var trackBrush = new SolidBrush(Color.FromArgb(60, 60, 60));
            g.FillRectangle(trackBrush, padding, trackY, rect.Width - padding * 2, trackHeight);

            float startX = padding;
            float endX = rect.Width - padding - size;
            float currentX = startX + (endX - startX) * t;

            var blockRect = new RectangleF(currentX, rect.Height / 2f - size / 2f, size, size);
            DrawBlock(g, blockRect, t);
        }
    }

    private void DrawBlock(Graphics g, RectangleF rect, float t)
    {
        // 扁平化色块：根据状态切换颜色
        Color blockColor = t > 0.99 ? Color.FromArgb(0, 120, 215) : // 伸出：蓝色
                           t < 0.01 ? Color.FromArgb(100, 100, 100) : // 缩回：深灰
                                      Color.FromArgb(0, 153, 188);    // 运动中：亮蓝

        using var brush = new SolidBrush(blockColor);
        g.FillRectangle(brush, rect);

        // 扁平化边框
        using var pen = new Pen(Color.FromArgb(80, 255, 255, 255), 1f);
        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
    }

    private void DrawGripper(Graphics g, float t)
    {
        var rect = ClientRectangle;
        var center = new PointF(rect.Width / 2f, rect.Height / 2f);
        var gap = (float)(CloseWidth + (OpenWidth - CloseWidth) * (1 - t));
        var jawLen = 16f;
        var jawThk = 5f;

        using var jawBrush = new SolidBrush(Color.FromArgb(120, 170, 200));

        var left = new RectangleF(center.X - gap - jawLen, center.Y - jawThk / 2f, jawLen, jawThk);
        var right = new RectangleF(center.X + gap, center.Y - jawThk / 2f, jawLen, jawThk);
        g.FillRectangle(jawBrush, left);
        g.FillRectangle(jawBrush, right);

        using var hubBrush = new SolidBrush(Color.FromArgb(80, 100, 130));
        g.FillEllipse(hubBrush, center.X - 8, center.Y - 8, 16, 16);
    }

    private void DrawSuction(Graphics g, float t)
    {
        var rect = ClientRectangle;
        var center = new PointF(rect.Width / 2f, rect.Height / 2f);
        var r = (float)Math.Max(6, Diameter / 2f);
        // var t = Math.Clamp(Value, 0, 1); // This line is removed as t is now passed as a parameter

        using var ringPen = new Pen(Color.FromArgb(140, 180, 200), 2f);
        g.DrawEllipse(ringPen, center.X - r, center.Y - r, r * 2, r * 2);

        using var fillBrush = new SolidBrush(Color.FromArgb((int)(60 + 120 * t), 120, 180, 220));
        g.FillEllipse(fillBrush, center.X - r + 2, center.Y - r + 2, r * 2 - 4, r * 2 - 4);
    }
}

internal enum CylinderVisualStyle
{
    SlideBlock,
    Gripper,
    SuctionPen
}
