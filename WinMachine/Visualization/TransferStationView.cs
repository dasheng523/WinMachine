using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinMachine;

internal sealed class TransferStationView : UserControl
{
    private const int SeatCount = 4;

    private double _slidePos; // -120..120
    private double _leftRotate; // 0..180
    private double _rightRotate; // 0..180

    private bool _leftLiftUp;
    private bool _rightLiftUp;

    private bool _leftGripClosed;
    private bool _rightGripClosed;

    private TransferStationModel? _model;

    public TransferStationView()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        BackColor = Color.FromArgb(18, 18, 18);
        ForeColor = Color.FromArgb(230, 230, 230);
        Font = new Font("Segoe UI", 9F);
    }

    public void ResetModel(TransferStationModel? model)
    {
        _model = model;
        Invalidate();
    }

    public void ApplyDomainEvent(SimulationDomainEvent ev)
    {
        if (_model == null) return;
        if (ev is TransferGripEvent te)
        {
            _model.Apply(te);
            Invalidate();
        }
    }

    public void SetSlide(double pos) { _slidePos = pos; Invalidate(); }
    public void SetLeftRotate(double angle) { _leftRotate = angle; Invalidate(); }
    public void SetRightRotate(double angle) { _rightRotate = angle; Invalidate(); }
    public void SetLeftLift(bool up) { _leftLiftUp = up; Invalidate(); }
    public void SetRightLift(bool up) { _rightLiftUp = up; Invalidate(); }
    public void SetLeftGrip(bool closed) { _leftGripClosed = closed; Invalidate(); }
    public void SetRightGrip(bool closed) { _rightGripClosed = closed; Invalidate(); }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        DrawBackground(g);

        var pad = 24;
        var bounds = Rectangle.Inflate(ClientRectangle, -pad, -pad);

        // 布局：左模块 | 中心推拉 | 右模块
        var leftArea = new Rectangle(bounds.Left, bounds.Top, (int)(bounds.Width * 0.26), bounds.Height);
        var centerArea = new Rectangle(leftArea.Right + 16, bounds.Top, (int)(bounds.Width * 0.48), bounds.Height);
        var rightArea = new Rectangle(centerArea.Right + 16, bounds.Top, bounds.Right - (centerArea.Right + 16), bounds.Height);

        DrawSideModule(g, leftArea, TransferSide.Left);
        DrawCenterSlide(g, centerArea);
        DrawSideModule(g, rightArea, TransferSide.Right);

        DrawLegend(g, bounds);
    }

    private void DrawBackground(Graphics g)
    {
        using var brush = new LinearGradientBrush(ClientRectangle, Color.FromArgb(14, 14, 16), Color.FromArgb(26, 26, 28), 90f);
        g.FillRectangle(brush, ClientRectangle);

        // 工业风：轻微网格
        using var gridPen = new Pen(Color.FromArgb(28, 255, 255, 255), 1f);
        for (int x = 0; x < Width; x += 32) g.DrawLine(gridPen, x, 0, x, Height);
        for (int y = 0; y < Height; y += 32) g.DrawLine(gridPen, 0, y, Width, y);
    }

    private void DrawCenterSlide(Graphics g, Rectangle area)
    {
        DrawTitle(g, area, "扫码座推拉气缸 (Slide)");

        var track = new Rectangle(area.Left + 24, area.Top + 70, area.Width - 48, 62);
        DrawMetalRounded(g, track, radius: 18, baseDark: Color.FromArgb(52, 55, 60));

        // 轨道端盖
        using (var p = new Pen(Color.FromArgb(60, 0, 0, 0), 2f))
        {
            g.DrawLine(p, track.Left + 12, track.Bottom - 10, track.Right - 12, track.Bottom - 10);
        }

        var carriageW = 220;
        var carriageH = 100;
        var cx = track.Left + track.Width / 2 + MapToPixels(_slidePos, -120, 120, track.Width - carriageW);
        var carriage = new Rectangle(cx - carriageW / 2, track.Bottom + 26, carriageW, carriageH);

        DrawMetalRounded(g, carriage, radius: 22, baseDark: Color.FromArgb(44, 47, 52));

        // 4个扫码座（左右各2）
        var seatGap = 10;
        var seatW = (carriage.Width - seatGap * 5) / 4;
        var seatH = 50;

        for (int i = 0; i < SeatCount; i++)
        {
            var seatRect = new Rectangle(
                carriage.Left + seatGap + i * (seatW + seatGap),
                carriage.Top + 24,
                seatW,
                seatH);

            DrawSeat(g, seatRect, isTest: false, index: i);
        }

        // 位置读数
        var txt = $"Slide = {_slidePos:0.0}";
        using var textBrush = new SolidBrush(Color.FromArgb(210, 220, 220, 220));
        g.DrawString(txt, Font, textBrush, new PointF(carriage.Left, carriage.Bottom + 10));
    }

    private void DrawSideModule(Graphics g, Rectangle area, TransferSide side)
    {
        var title = side == TransferSide.Left ? "左侧搬运模块" : "右侧搬运模块";
        DrawTitle(g, area, title);

        var isLeft = side == TransferSide.Left;
        var liftUp = isLeft ? _leftLiftUp : _rightLiftUp;
        var rotate = isLeft ? _leftRotate : _rightRotate;
        var gripClosed = isLeft ? _leftGripClosed : _rightGripClosed;

        var module = new Rectangle(area.Left + 10, area.Top + 70, area.Width - 20, area.Height - 90);
        DrawMetalRounded(g, module, radius: 24, baseDark: Color.FromArgb(38, 40, 44));

        // 测试座（固定）两座
        var testSeatTop = module.Top + 28;
        var testSeatW = (module.Width - 36) / 2;
        var testSeatH = 56;

        var testAIndex = isLeft ? 0 : 2;
        var testBIndex = isLeft ? 1 : 3;

        var testA = new Rectangle(module.Left + 14, testSeatTop, testSeatW, testSeatH);
        var testB = new Rectangle(testA.Right + 8, testSeatTop, testSeatW, testSeatH);
        DrawSeat(g, testA, isTest: true, index: testAIndex);
        DrawSeat(g, testB, isTest: true, index: testBIndex);

        // 升降导轨
        var rail = new Rectangle(module.Left + module.Width / 2 - 10, testA.Bottom + 20, 20, module.Height - 180);
        DrawMetalRounded(g, rail, radius: 10, baseDark: Color.FromArgb(30, 32, 36));

        // 升降头位置
        var downY = rail.Bottom - 70;
        var upY = rail.Top + 20;
        var headY = liftUp ? upY : downY;

        var head = new Rectangle(module.Left + 22, headY, module.Width - 44, 64);

        // 旋转台（头部）
        DrawRotaryHead(g, head, rotate, gripClosed, side);

        // 状态读数
        var txt = $"Lift={(liftUp ? "Up" : "Down")}  Rot={rotate:0}°  Grip={(gripClosed ? "Closed" : "Open")}";
        using var textBrush = new SolidBrush(Color.FromArgb(210, 220, 220, 220));
        g.DrawString(txt, Font, textBrush, new PointF(module.Left + 12, module.Bottom - 26));
    }

    private void DrawRotaryHead(Graphics g, Rectangle bounds, double angleDeg, bool gripClosed, TransferSide side)
    {
        DrawMetalRounded(g, bounds, radius: 18, baseDark: Color.FromArgb(46, 48, 54));

        // 旋转盘
        var center = new PointF(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f);
        var radius = Math.Min(bounds.Width, bounds.Height) * 0.32f;
        var discRect = new RectangleF(center.X - radius, center.Y - radius, radius * 2, radius * 2);

        using (var discBrush = new LinearGradientBrush(Rectangle.Round(discRect), Color.FromArgb(90, 120, 130, 140), Color.FromArgb(40, 20, 20, 22), 90f))
        {
            g.FillEllipse(discBrush, discRect);
        }
        using (var discPen = new Pen(Color.FromArgb(90, 255, 255, 255), 1.5f))
        {
            g.DrawEllipse(discPen, discRect);
        }

        // 指针
        var a = (float)((angleDeg - 90) * Math.PI / 180.0);
        var tip = new PointF(center.X + (float)Math.Cos(a) * radius * 0.9f, center.Y + (float)Math.Sin(a) * radius * 0.9f);
        using (var p = new Pen(Color.FromArgb(220, 255, 200, 80), 2.2f))
        {
            g.DrawLine(p, center, tip);
        }

        // 4夹爪 + 物料
        var jawGap = gripClosed ? 8 : 18;
        var jawLen = 22;
        var jawThk = 6;

        var leftJaw = new Rectangle((int)(center.X - jawGap - jawLen), (int)(center.Y - jawThk / 2), jawLen, jawThk);
        var rightJaw = new Rectangle((int)(center.X + jawGap), (int)(center.Y - jawThk / 2), jawLen, jawThk);
        var topJaw = new Rectangle((int)(center.X - jawThk / 2), (int)(center.Y - jawGap - jawLen), jawThk, jawLen);
        var botJaw = new Rectangle((int)(center.X - jawThk / 2), (int)(center.Y + jawGap), jawThk, jawLen);

        DrawJaw(g, leftJaw);
        DrawJaw(g, rightJaw);
        DrawJaw(g, topJaw);
        DrawJaw(g, botJaw);

        // 夹持物料：用 4 个圆点表示（2来自扫码座、2来自测试座）
        var held = side == TransferSide.Left ? _model?.LeftHeld : _model?.RightHeld;
        if (held != null)
        {
            var pts = new[]
            {
                new PointF(center.X - 34, center.Y - 14),
                new PointF(center.X - 34, center.Y + 14),
                new PointF(center.X + 34, center.Y - 14),
                new PointF(center.X + 34, center.Y + 14),
            };

            for (int i = 0; i < Math.Min(held.Length, pts.Length); i++)
            {
                if (held[i] == null) continue;
                DrawWorkpiece(g, pts[i], held[i]!.Color, held[i]!.Id);
            }
        }
    }

    private void DrawSeat(Graphics g, Rectangle rect, bool isTest, int index)
    {
        var baseDark = isTest ? Color.FromArgb(34, 37, 41) : Color.FromArgb(36, 39, 44);
        DrawMetalRounded(g, rect, radius: 16, baseDark: baseDark);

        // 上标签
        var label = isTest ? $"T{index + 1}" : $"S{index + 1}";
        using (var b = new SolidBrush(Color.FromArgb(190, 230, 230, 230)))
        {
            g.DrawString(label, Font, b, rect.Left + 10, rect.Top + 8);
        }

        Workpiece? wp = null;
        if (_model != null)
        {
            wp = isTest ? _model.TestSeats[index] : _model.ScanSeats[index];
        }

        if (wp != null)
        {
            var center = new PointF(rect.Left + rect.Width / 2f, rect.Top + rect.Height * 0.62f);
            DrawWorkpiece(g, center, wp.Color, wp.Id);
        }
        else
        {
            using var p = new Pen(Color.FromArgb(80, 255, 255, 255), 1f);
            g.DrawEllipse(p, rect.Left + rect.Width / 2 - 10, rect.Top + rect.Height / 2 - 2, 20, 20);
        }
    }

    private static void DrawWorkpiece(Graphics g, PointF center, Color color, string id)
    {
        var r = 10f;
        var rc = new RectangleF(center.X - r, center.Y - r, r * 2, r * 2);
        using (var b = new LinearGradientBrush(Rectangle.Round(rc), ControlPaint.Light(color, 0.2f), ControlPaint.Dark(color, 0.2f), 90f))
        {
            g.FillEllipse(b, rc);
        }
        using (var p = new Pen(Color.FromArgb(180, 255, 255, 255), 1f))
        {
            g.DrawEllipse(p, rc);
        }

        using var tb = new SolidBrush(Color.FromArgb(220, 10, 10, 10));
        using var f = new Font("Segoe UI", 7F, FontStyle.Bold);
        var sz = g.MeasureString(id, f);
        g.DrawString(id, f, tb, center.X - sz.Width / 2f, center.Y - sz.Height / 2f);
    }

    private static void DrawJaw(Graphics g, Rectangle rect)
    {
        DrawMetalRounded(g, rect, radius: 4, baseDark: Color.FromArgb(68, 72, 78));
    }

    private static void DrawMetalRounded(Graphics g, Rectangle rect, int radius, Color baseDark)
    {
        using var path = RoundedRect(rect, radius);
        using var shadowBrush = new SolidBrush(Color.FromArgb(80, 0, 0, 0));
        using var mainBrush = new LinearGradientBrush(rect, ControlPaint.Light(baseDark, 0.25f), ControlPaint.Dark(baseDark, 0.25f), 90f);
        using var outline = new Pen(Color.FromArgb(90, 255, 255, 255), 1f);

        var shadowRect = rect;
        shadowRect.Offset(0, 2);
        using (var shadowPath = RoundedRect(shadowRect, radius))
            g.FillPath(shadowBrush, shadowPath);

        g.FillPath(mainBrush, path);
        g.DrawPath(outline, path);

        // 高光
        var hi = Rectangle.Inflate(rect, -6, -6);
        hi.Height = Math.Max(6, hi.Height / 3);
        using var hiPath = RoundedRect(hi, Math.Max(6, radius - 4));
        using var hiBrush = new LinearGradientBrush(hi, Color.FromArgb(70, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 90f);
        g.FillPath(hiBrush, hiPath);
    }

    private void DrawTitle(Graphics g, Rectangle area, string title)
    {
        using var b = new SolidBrush(Color.FromArgb(220, 235, 235, 235));
        using var f = new Font("Segoe UI", 10F, FontStyle.Bold);
        g.DrawString(title, f, b, area.Left + 6, area.Top + 6);
    }

    private void DrawLegend(Graphics g, Rectangle area)
    {
        using var b = new SolidBrush(Color.FromArgb(170, 220, 220, 220));
        g.DrawString("扫码座(S1..S4)随推拉移动；测试座(T1..T4)固定；夹爪闭合时抓取并在张开时互换。", Font, b, area.Left + 6, area.Bottom - 18);
    }

    private static int MapToPixels(double val, double min, double max, int span)
    {
        if (max <= min) return 0;
        var t = (val - min) / (max - min);
        if (t < 0) t = 0;
        if (t > 1) t = 1;
        return (int)((t - 0.5) * span);
    }

    private static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;

        path.AddArc(r.Left, r.Top, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();

        return path;
    }
}
