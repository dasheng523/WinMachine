using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Machine.Framework.Visualization.SceneGraph;

internal static class SpriteDraw
{
    public static Action<Graphics, float, float> CreateDefaultCylinderDraw(
        Func<double> getValue,
        bool isVertical,
        bool isReversed)
    {
        return (g, w, h) =>
        {
            var t = (float)Math.Clamp(getValue() / 100.0, 0, 1);
            if (isReversed) t = 1 - t;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Work in top-left coordinates: (0,0) .. (w,h)
            var pad = MathF.Max(2, MathF.Min(w, h) * 0.08f);
            var stroke = MathF.Max(1.5f, MathF.Min(w, h) * 0.06f);

            using var bodyFill = new SolidBrush(Color.FromArgb(70, 170, 190, 210));
            using var bodyStroke = new Pen(Color.FromArgb(220, 200, 220, 235), stroke);
            using var rodFill = new SolidBrush(Color.FromArgb(160, 230, 235, 240));
            using var rodStroke = new Pen(Color.FromArgb(220, 200, 220, 235), MathF.Max(1, stroke * 0.7f));
            using var accent = new SolidBrush(Color.FromArgb(140, 80, 110, 140));

            if (!isVertical)
            {
                DrawHorizontalCylinder(g, w, h, pad, t, bodyFill, bodyStroke, rodFill, rodStroke, accent);
            }
            else
            {
                // Rotate the same horizontal drawing into vertical direction.
                // We rotate the canvas around center, then reuse horizontal logic.
                var state = g.Save();
                g.TranslateTransform(w / 2f, h / 2f);
                g.RotateTransform(-90);
                g.TranslateTransform(-h / 2f, -w / 2f);
                DrawHorizontalCylinder(g, h, w, pad, t, bodyFill, bodyStroke, rodFill, rodStroke, accent);
                g.Restore(state);
            }
        };
    }

    public static Action<Graphics, float, float> CreateSlideBlockDraw(
        Func<double> getValue,
        bool isVertical,
        bool isReversed)
    {
        return (g, w, h) =>
        {
            var t = (float)Math.Clamp(getValue() / 100.0, 0, 1);
            if (isReversed) t = 1 - t;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            var pad = MathF.Max(2, MathF.Min(w, h) * 0.08f);
            var stroke = MathF.Max(1.5f, MathF.Min(w, h) * 0.06f);

            using var railFill = new SolidBrush(Color.FromArgb(55, 150, 160, 175));
            using var railStroke = new Pen(Color.FromArgb(200, 190, 205, 220), MathF.Max(1, stroke * 0.8f));
            using var blockFill = new SolidBrush(Color.FromArgb(110, 70, 140, 200));
            using var blockStroke = new Pen(Color.FromArgb(230, 210, 230, 245), stroke);
            using var shadow = new SolidBrush(Color.FromArgb(40, 0, 0, 0));

            if (!isVertical)
            {
                DrawHorizontalSlideBlock(g, w, h, pad, t, railFill, railStroke, blockFill, blockStroke, shadow);
            }
            else
            {
                var state = g.Save();
                g.TranslateTransform(w / 2f, h / 2f);
                g.RotateTransform(-90);
                g.TranslateTransform(-h / 2f, -w / 2f);
                DrawHorizontalSlideBlock(g, h, w, pad, t, railFill, railStroke, blockFill, blockStroke, shadow);
                g.Restore(state);
            }
        };
    }

    public static Action<Graphics, float, float> CreateSlideRailDraw(bool isVertical)
    {
        return (g, w, h) =>
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var pad = MathF.Max(2, MathF.Min(w, h) * 0.08f);
            var stroke = MathF.Max(1.5f, MathF.Min(w, h) * 0.06f);

            using var railFill = new SolidBrush(Color.FromArgb(55, 150, 160, 175));
            using var railStroke = new Pen(Color.FromArgb(200, 190, 205, 220), MathF.Max(1, stroke * 0.8f));

            if (!isVertical)
            {
                DrawHorizontalRailOnly(g, w, h, pad, railFill, railStroke);
            }
            else
            {
                var state = g.Save();
                g.TranslateTransform(w / 2f, h / 2f);
                g.RotateTransform(-90);
                g.TranslateTransform(-h / 2f, -w / 2f);
                DrawHorizontalRailOnly(g, h, w, pad, railFill, railStroke);
                g.Restore(state);
            }
        };
    }

    public static Action<Graphics, float, float> CreateSlideCarriageDraw()
    {
        return (g, w, h) =>
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var stroke = MathF.Max(1.5f, MathF.Min(w, h) * 0.06f);
            using var blockFill = new SolidBrush(Color.FromArgb(110, 70, 140, 200));
            using var blockStroke = new Pen(Color.FromArgb(230, 210, 230, 245), stroke);
            using var shadow = new SolidBrush(Color.FromArgb(40, 0, 0, 0));

            var bx = 0f;
            var by = 0f;

            // Shadow
            g.FillRectangle(shadow, bx + 2, by + 2, w, h);

            using (var path = RoundedRect(bx, by, w, h, MathF.Min(w, h) * 0.18f))
            {
                g.FillPath(blockFill, path);
                g.DrawPath(blockStroke, path);
            }

            var holeR = MathF.Max(2, MathF.Min(w, h) * 0.08f);
            using var hole = new SolidBrush(Color.FromArgb(130, 30, 40, 55));
            g.FillEllipse(hole, bx + w * 0.25f - holeR, by + h * 0.30f - holeR, holeR * 2, holeR * 2);
            g.FillEllipse(hole, bx + w * 0.75f - holeR, by + h * 0.30f - holeR, holeR * 2, holeR * 2);
            g.FillEllipse(hole, bx + w * 0.25f - holeR, by + h * 0.70f - holeR, holeR * 2, holeR * 2);
            g.FillEllipse(hole, bx + w * 0.75f - holeR, by + h * 0.70f - holeR, holeR * 2, holeR * 2);
        };
    }

    private static void DrawHorizontalCylinder(
        Graphics g,
        float w,
        float h,
        float pad,
        float t,
        Brush bodyFill,
        Pen bodyStroke,
        Brush rodFill,
        Pen rodStroke,
        Brush accent)
    {
        // Reserve the right side for rod travel.
        var travel = MathF.Max(8, w * 0.30f);
        var bodyW = MathF.Max(10, w - travel - pad * 2);
        var bodyH = MathF.Max(10, h - pad * 2);

        var bodyX = pad;
        var bodyY = (h - bodyH) / 2f;

        // Rod start at body end, extend by t*travel
        var rodX = bodyX + bodyW;
        var rodY = bodyY + bodyH * 0.35f;
        var rodH = bodyH * 0.30f;
        var rodW = t * travel;

        // Piston head inside body follows rod motion
        var pistonW = MathF.Max(2, bodyW * 0.10f);
        var pistonX = bodyX + bodyW * (0.25f + 0.55f * t);
        var pistonY = bodyY + bodyH * 0.18f;
        var pistonH = bodyH * 0.64f;

        using (var path = RoundedRect(bodyX, bodyY, bodyW, bodyH, MathF.Min(bodyH, bodyW) * 0.18f))
        {
            g.FillPath(bodyFill, path);
            g.DrawPath(bodyStroke, path);
        }

        // Ports (small circles)
        var portR = MathF.Max(2, MathF.Min(bodyH, bodyW) * 0.06f);
        g.FillEllipse(accent, bodyX + bodyW * 0.20f - portR, bodyY + bodyH * 0.25f - portR, portR * 2, portR * 2);
        g.FillEllipse(accent, bodyX + bodyW * 0.20f - portR, bodyY + bodyH * 0.75f - portR, portR * 2, portR * 2);

        // Piston
        g.FillRectangle(accent, pistonX - pistonW / 2f, pistonY, pistonW, pistonH);

        // Rod + tip
        if (rodW > 0.5f)
        {
            g.FillRectangle(rodFill, rodX, rodY, rodW, rodH);
            g.DrawRectangle(rodStroke, rodX, rodY, rodW, rodH);

            var tipW = MathF.Max(2, rodH * 0.65f);
            g.FillRectangle(accent, rodX + rodW, rodY - (tipW - rodH) / 2f, tipW, tipW);
        }

        // End caps
        var capW = MathF.Max(2, bodyW * 0.06f);
        g.FillRectangle(accent, bodyX, bodyY, capW, bodyH);
        g.FillRectangle(accent, bodyX + bodyW - capW, bodyY, capW, bodyH);
    }

    private static void DrawHorizontalSlideBlock(
        Graphics g,
        float w,
        float h,
        float pad,
        float t,
        Brush railFill,
        Pen railStroke,
        Brush blockFill,
        Pen blockStroke,
        Brush shadow)
    {
        var railH = MathF.Max(6, h * 0.22f);
        var railY = (h - railH) / 2f;
        var railX = pad;
        var railW = w - pad * 2;

        using (var path = RoundedRect(railX, railY, railW, railH, railH * 0.35f))
        {
            g.FillPath(railFill, path);
            g.DrawPath(railStroke, path);
        }

        var blockW = MathF.Max(18, MathF.Min(railW * 0.30f, 60));
        var blockH = MathF.Max(railH * 1.8f, h * 0.55f);

        var blockTravel = MathF.Max(0, railW - blockW);
        var blockX = railX + blockTravel * t;
        var blockY = (h - blockH) / 2f;

        // Simple drop shadow
        g.FillRectangle(shadow, blockX + 2, blockY + 2, blockW, blockH);

        using (var path = RoundedRect(blockX, blockY, blockW, blockH, MathF.Min(blockW, blockH) * 0.18f))
        {
            g.FillPath(blockFill, path);
            g.DrawPath(blockStroke, path);
        }

        // Indicate mounting holes
        var holeR = MathF.Max(2, MathF.Min(blockW, blockH) * 0.06f);
        using var hole = new SolidBrush(Color.FromArgb(130, 30, 40, 55));
        g.FillEllipse(hole, blockX + blockW * 0.25f - holeR, blockY + blockH * 0.30f - holeR, holeR * 2, holeR * 2);
        g.FillEllipse(hole, blockX + blockW * 0.75f - holeR, blockY + blockH * 0.30f - holeR, holeR * 2, holeR * 2);
        g.FillEllipse(hole, blockX + blockW * 0.25f - holeR, blockY + blockH * 0.70f - holeR, holeR * 2, holeR * 2);
        g.FillEllipse(hole, blockX + blockW * 0.75f - holeR, blockY + blockH * 0.70f - holeR, holeR * 2, holeR * 2);
    }

    private static void DrawHorizontalRailOnly(
        Graphics g,
        float w,
        float h,
        float pad,
        Brush railFill,
        Pen railStroke)
    {
        var railH = MathF.Max(6, h * 0.22f);
        var railY = (h - railH) / 2f;
        var railX = pad;
        var railW = w - pad * 2;

        using var path = RoundedRect(railX, railY, railW, railH, railH * 0.35f);
        g.FillPath(railFill, path);
        g.DrawPath(railStroke, path);
    }

    private static GraphicsPath RoundedRect(float x, float y, float w, float h, float r)
    {
        var rr = MathF.Max(0, MathF.Min(r, MathF.Min(w, h) / 2f));

        var path = new GraphicsPath();
        if (rr <= 0.01f)
        {
            path.AddRectangle(new RectangleF(x, y, w, h));
            path.CloseFigure();
            return path;
        }

        var d = rr * 2;
        path.AddArc(x, y, d, d, 180, 90);
        path.AddArc(x + w - d, y, d, d, 270, 90);
        path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
        path.AddArc(x, y + h - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
