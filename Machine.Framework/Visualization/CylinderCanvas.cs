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
    public CylinderVisualStyle Style { get; set; } = CylinderVisualStyle.Slider;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Vertical { get; set; }

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

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Color.FromArgb(24, 24, 24));

        switch (Style)
        {
            case CylinderVisualStyle.Gripper:
                DrawGripper(g);
                break;
            case CylinderVisualStyle.SuctionPen:
                DrawSuction(g);
                break;
            default:
                DrawSlider(g);
                break;
        }
    }

    private void DrawSlider(Graphics g)
    {
        var rect = ClientRectangle;
        var pad = 8;
        var t = Math.Clamp(Value, 0, 1);

        if (Vertical)
        {
            var x = rect.Width / 2f - 8;
            var y0 = pad;
            var y1 = rect.Height - pad;
            using var bodyBrush = new SolidBrush(Color.FromArgb(70, 90, 120));
            g.FillRectangle(bodyBrush, x, y0, 16, y1 - y0);

            var rodH = (float)((y1 - y0) * t);
            using var rodBrush = new SolidBrush(Color.FromArgb(160, 200, 220));
            g.FillRectangle(rodBrush, x + 4, y1 - rodH, 8, rodH);
        }
        else
        {
            var y = rect.Height / 2f - 8;
            var x0 = pad;
            var x1 = rect.Width - pad;
            using var bodyBrush = new SolidBrush(Color.FromArgb(70, 90, 120));
            g.FillRectangle(bodyBrush, x0, y, x1 - x0, 16);

            var rodW = (float)((x1 - x0) * t);
            using var rodBrush = new SolidBrush(Color.FromArgb(160, 200, 220));
            g.FillRectangle(rodBrush, x0, y + 4, rodW, 8);
        }
    }

    private void DrawGripper(Graphics g)
    {
        var rect = ClientRectangle;
        var center = new PointF(rect.Width / 2f, rect.Height / 2f);
        var t = Math.Clamp(Value, 0, 1);
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

    private void DrawSuction(Graphics g)
    {
        var rect = ClientRectangle;
        var center = new PointF(rect.Width / 2f, rect.Height / 2f);
        var r = (float)Math.Max(6, Diameter / 2f);
        var t = Math.Clamp(Value, 0, 1);

        using var ringPen = new Pen(Color.FromArgb(140, 180, 200), 2f);
        g.DrawEllipse(ringPen, center.X - r, center.Y - r, r * 2, r * 2);

        using var fillBrush = new SolidBrush(Color.FromArgb((int)(60 + 120 * t), 120, 180, 220));
        g.FillEllipse(fillBrush, center.X - r + 2, center.Y - r + 2, r * 2 - 4, r * 2 - 4);
    }
}

internal enum CylinderVisualStyle
{
    Slider,
    Gripper,
    SuctionPen
}
