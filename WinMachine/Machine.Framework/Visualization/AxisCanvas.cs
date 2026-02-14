using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Machine.Framework.Visualization;

internal sealed class AxisCanvas : Control
{
    public AxisCanvas()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = Color.FromArgb(220, 230, 230, 230);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AxisVisualStyle Style { get; set; } = AxisVisualStyle.LinearGuide;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Vertical { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Reversed { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double Min { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double Max { get; set; } = 100;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double Value { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double Length { get; set; } = 120;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double SliderWidth { get; set; } = 18;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double Radius { get; set; } = 20;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Color.FromArgb(24, 24, 24));

        switch (Style)
        {
            case AxisVisualStyle.RotaryTable:
                DrawRotary(g);
                break;
            default:
                DrawLinear(g);
                break;
        }
    }

    private void DrawLinear(Graphics g)
    {
        var rect = ClientRectangle;
        var pad = 8;
        var trackThickness = 6;

        var range = Math.Max(1, Max - Min);
        var t = (Value - Min) / range;
        if (t < 0) t = 0;
        if (t > 1) t = 1;

        if (Reversed) t = 1 - t;

        if (Vertical)
        {
            var x = rect.Width / 2 - trackThickness / 2;
            var y0 = pad;
            var y1 = rect.Height - pad;
            using var trackPen = new Pen(Color.FromArgb(80, 180, 200), trackThickness);
            g.DrawLine(trackPen, x + trackThickness / 2f, y0, x + trackThickness / 2f, y1);

            var sliderH = Math.Max(12, rect.Width / 2f);
            var sliderY = (float)(y1 - t * (y1 - y0) - sliderH / 2);
            var sliderRect = new RectangleF(rect.Width / 2f - (float)SliderWidth / 2f, sliderY, (float)SliderWidth, sliderH);
            using var sliderBrush = new SolidBrush(Color.FromArgb(90, 120, 200));
            g.FillRoundedRectangle(sliderBrush, sliderRect, 6);
        }
        else
        {
            var y = rect.Height / 2 - trackThickness / 2;
            var x0 = pad;
            var x1 = rect.Width - pad;
            using var trackPen = new Pen(Color.FromArgb(80, 180, 200), trackThickness);
            g.DrawLine(trackPen, x0, y + trackThickness / 2f, x1, y + trackThickness / 2f);

            var sliderW = Math.Max(12, rect.Height / 2f);
            var sliderX = (float)(x0 + t * (x1 - x0) - sliderW / 2);
            var sliderRect = new RectangleF(sliderX, rect.Height / 2f - (float)SliderWidth / 2f, sliderW, (float)SliderWidth);
            using var sliderBrush = new SolidBrush(Color.FromArgb(90, 120, 200));
            g.FillRoundedRectangle(sliderBrush, sliderRect, 6);
        }
    }

    private void DrawRotary(Graphics g)
    {
        var rect = ClientRectangle;
        var r = (float)Math.Min(rect.Width, rect.Height) / 2f - 6;
        var center = new PointF(rect.Width / 2f, rect.Height / 2f);

        using (var brush = new SolidBrush(Color.FromArgb(50, 60, 80)))
            g.FillEllipse(brush, center.X - r, center.Y - r, r * 2, r * 2);

        using (var pen = new Pen(Color.FromArgb(120, 200, 220), 2f))
            g.DrawEllipse(pen, center.X - r, center.Y - r, r * 2, r * 2);

        var range = Math.Max(1, Max - Min);
        var t = (Value - Min) / range;
        if (t < 0) t = 0;
        if (t > 1) t = 1;

        if (Reversed) t = 1 - t;

        var angle = (float)(t * 270 - 135);
        var rad = (float)(Math.PI * angle / 180.0);
        var tip = new PointF(center.X + (float)Math.Cos(rad) * (r - 6), center.Y + (float)Math.Sin(rad) * (r - 6));

        using var needle = new Pen(Color.FromArgb(220, 255, 180), 2.2f);
        g.DrawLine(needle, center, tip);
    }
}

internal enum AxisVisualStyle
{
    LinearGuide,
    RotaryTable
}

internal static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics g, Brush brush, RectangleF bounds, float radius)
    {
        using var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        g.FillPath(brush, path);
    }
}
