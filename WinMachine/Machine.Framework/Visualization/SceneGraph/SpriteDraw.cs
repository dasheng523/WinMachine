using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

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

            // 使用 (0,0) 到 (w,h) 的坐标系
            if (!isVertical)
            {
                // 水平导轨：在 Y 方向居中的细槽
                var railH = MathF.Max(6, h * 0.22f);
                var railY = (h - railH) / 2f;
                var railX = pad;
                var railW = w - pad * 2;

                using var path = RoundedRect(railX, railY, railW, railH, railH * 0.35f);
                g.FillPath(railFill, path);
                g.DrawPath(railStroke, path);
            }
            else
            {
                // 垂直导轨：在 X 方向居中的细槽
                var railW = MathF.Max(6, w * 0.22f);
                var railX = (w - railW) / 2f;
                var railY = pad;
                var railH = h - pad * 2;

                using var path = RoundedRect(railX, railY, railW, railH, railW * 0.35f);
                g.FillPath(railFill, path);
                g.DrawPath(railStroke, path);
            }
        };
    }

    public static Action<Graphics, float, float> CreateSlideCarriageDraw()
    {
        return (g, w, h) =>
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var stroke = MathF.Max(1.5f, MathF.Min(w, h) * 0.06f);
            using var blockStroke = new Pen(Color.FromArgb(230, 210, 230, 245), stroke);
            using var shadow = new SolidBrush(Color.FromArgb(60, 0, 0, 0));

            // Shadow
            g.FillRectangle(shadow, 2, 2, w, h);

            using (var path = RoundedRect(0, 0, w, h, MathF.Min(w, h) * 0.18f))
            {
                // Metallic gradient
                using var lg = new LinearGradientBrush(new RectangleF(0, 0, w, h), 
                    Color.FromArgb(100, 140, 180), Color.FromArgb(60, 90, 120), 45f);
                g.FillPath(lg, path);
                g.DrawPath(blockStroke, path);
            }

            // Mounting holes
            var holeR = MathF.Max(2, MathF.Min(w, h) * 0.08f);
            using var holeInner = new SolidBrush(Color.FromArgb(160, 20, 25, 35));
            g.FillEllipse(holeInner, w * 0.25f - holeR, h * 0.30f - holeR, holeR * 2, holeR * 2);
            g.FillEllipse(holeInner, w * 0.75f - holeR, h * 0.30f - holeR, holeR * 2, holeR * 2);
            g.FillEllipse(holeInner, w * 0.25f - holeR, h * 0.70f - holeR, holeR * 2, holeR * 2);
            g.FillEllipse(holeInner, w * 0.75f - holeR, h * 0.70f - holeR, holeR * 2, holeR * 2);
        };
    }

    public static Action<Graphics, float, float> CreateGripperDraw(Func<double> getValue, bool isReversed)
    {
        return (g, w, h) =>
        {
            var t = (float)Math.Clamp(getValue() / 100.0, 0, 1);
            if (isReversed) t = 1 - t;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Base Body
            var baseH = h * 0.35f;
            using var baseFill = new LinearGradientBrush(new RectangleF(-w/2, 0, w, baseH), 
                Color.FromArgb(140, 150, 160), Color.FromArgb(80, 90, 100), 90f);
            g.FillRectangle(baseFill, -w/2, 0, w, baseH);
            g.DrawRectangle(Pens.Gray, -w/2, 0, w, baseH);

            // Jaws (Two L-shaped parts moving inward)
            var jawW = w * 0.22f;
            var jawH = h * 0.65f;
            var openGap = w * 0.45f;
            var currentGap = openGap * (1 - t); 

            void DrawJaw(float x, bool isRight)
            {
                using var jawFill = new SolidBrush(Color.FromArgb(190, 40, 50, 65));
                using var accent = new Pen(Color.FromArgb(120, 200, 210, 220), 1);
                g.FillRectangle(jawFill, x, baseH, jawW, jawH);
                g.DrawRectangle(accent, x, baseH, jawW, jawH);

                // Toe (the L-hook)
                var toeW = w * 0.3f;
                var toeH = h * 0.12f;
                var toeX = isRight ? x - toeW : x + jawW;
                g.FillRectangle(jawFill, toeX, baseH + jawH - toeH, toeW, toeH);
                g.DrawRectangle(accent, toeX, baseH + jawH - toeH, toeW, toeH);
            }

            DrawJaw(-currentGap / 2 - jawW, false);
            DrawJaw(currentGap / 2, true);
        };
    }

    public static Action<Graphics, float, float> CreateSuctionPenDraw(Func<double> getValue)
    {
        return (g, w, h) =>
        {
            var t = (float)Math.Clamp(getValue() / 100.0, 0, 1); 

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Shank
            var tubeW = w * 0.2f;
            var tubeH = h * 0.55f;
            using var tubeFill = new LinearGradientBrush(new RectangleF(-tubeW/2, 0, tubeW, tubeH), 
                Color.Silver, Color.Gray, 0f);
            g.FillRectangle(tubeFill, -tubeW/2, 0, tubeW, tubeH);
            g.DrawRectangle(Pens.DimGray, -tubeW/2, 0, tubeW, tubeH);

            // Bellows Tip (Orange/Amber)
            var cupR = w * 0.4f;
            var cupH = h * 0.45f;
            var cupColor = t > 0.5f ? Color.FromArgb(255, 120, 40) : Color.FromArgb(180, 80, 30);
            using var cupFill = new SolidBrush(cupColor);

            for (int i = 0; i < 3; i++)
            {
                var ly = tubeH + i * (cupH / 3.5f);
                var lw = cupR * 2 * (0.7f + 0.3f * (i / 2f));
                g.FillEllipse(cupFill, -lw / 2, ly, lw, cupH / 2.5f);
                g.DrawEllipse(Pens.Black, -lw / 2, ly, lw, cupH / 2.5f);
            }
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

    public static void DrawLinearAxis(Graphics g, float w, float h)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // 1. Carriage Body (More vibrant metallic)
        using var bodyStroke = new Pen(Color.FromArgb(200, 220, 230, 240), 1.5f);
        using var lg = new LinearGradientBrush(new RectangleF(-w/2, -h/2, w, h), 
            Color.FromArgb(110, 130, 150), Color.FromArgb(70, 90, 110), 45f);
        
        g.FillRectangle(lg, -w/2, -h/2, w, h);
        g.DrawRectangle(bodyStroke, -w/2, -h/2, w, h);

        // 2. Linear Rails shadow (Indicate it sits on a rail)
        using var railPen = new Pen(Color.FromArgb(80, 0, 0, 0), 2);
        g.DrawLine(railPen, -w/2 + 3, -h/2, -w/2 + 3, h/2);
        g.DrawLine(railPen, w/2 - 3, -h/2, w/2 - 3, h/2);

        // 3. Mounting details
        var r = Math.Min(w, h) * 0.1f;
        using var hole = new SolidBrush(Color.FromArgb(160, 20, 25, 30));
        g.FillEllipse(hole, -w/4 - r, -h/4 - r, r*2, r*2);
        g.FillEllipse(hole, w/4 - r, -h/4 - r, r*2, r*2);
        g.FillEllipse(hole, -w/4 - r, h/4 - r, r*2, r*2);
        g.FillEllipse(hole, w/4 - r, h/4 - r, r*2, r*2);
    }

    public static void DrawRotaryAxis(Graphics g, float radius)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // 1. Main Disk
        using var diskFill = new LinearGradientBrush(new RectangleF(-radius, -radius, radius * 2, radius * 2), 
            Color.FromArgb(160, 170, 180), Color.FromArgb(90, 100, 110), 45f);
        g.FillEllipse(diskFill, -radius, -radius, radius * 2, radius * 2);
        g.DrawEllipse(Pens.DimGray, -radius, -radius, radius * 2, radius * 2);

        // 2. Central Flange (Shaft)
        var flangeR = radius * 0.35f;
        using var flangeFill = new LinearGradientBrush(new RectangleF(-flangeR, -flangeR, flangeR * 2, flangeR * 2), 
            Color.FromArgb(220, 230, 240), Color.FromArgb(140, 150, 160), 135f);
        g.FillEllipse(flangeFill, -flangeR, -flangeR, flangeR * 2, flangeR * 2);
        g.DrawEllipse(Pens.Gray, -flangeR, -flangeR, flangeR * 2, flangeR * 2);

        // 3. Scale markings / Index holes
        using var markPen = new Pen(Color.FromArgb(100, 20, 25, 30), 2);
        for (int i = 0; i < 8; i++)
        {
            var angle = i * 45;
            var holeR = radius * 0.08f;
            var dist = radius * 0.7f;
            var hx = (float)(Math.Cos(angle * Math.PI / 180) * dist);
            var hy = (float)(Math.Sin(angle * Math.PI / 180) * dist);
            g.FillEllipse(Brushes.Black, hx - holeR, hy - holeR, holeR * 2, holeR * 2);
        }

        // 4. Highlight current direction
        using var dirPen = new Pen(Color.FromArgb(200, 255, 100, 0), 2);
        g.DrawLine(dirPen, 0, 0, radius, 0);
    }

    public static Action<Graphics, float, float> CreateMotorRailDraw(bool isVertical, double min, double max)
    {
        return (g, w, h) =>
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 使用 (0,0) 到 (w,h) 的坐标系，与 PivotY=0 的 SpriteNode 配合
            // 视觉余量：在真实行程两端增加 15 像素的基座延伸
            float padding = 15;
            float padLeft = isVertical ? 0 : padding;
            float padTop = isVertical ? padding : 0;
            float drawW = isVertical ? w : w + padding * 2;
            float drawH = isVertical ? h + padding * 2 : h;
            float offsetX = isVertical ? 0 : -padding;
            float offsetY = isVertical ? -padding : 0;

            // 1. Rail Base
            using var railFill = new LinearGradientBrush(
                new RectangleF(offsetX, offsetY, drawW, drawH), 
                Color.FromArgb(90, 100, 110), Color.FromArgb(50, 60, 70), 
                isVertical ? 0f : 90f);
            using var path = RoundedRect(offsetX, offsetY, drawW, drawH, Math.Min(w, h) * 0.2f);
            g.FillPath(railFill, path);
            g.DrawPath(Pens.Black, path);

            // 2. Linear guides (Bright steel lines)
            using var guidePen = new Pen(Color.FromArgb(180, 200, 210, 220), 1.5f);
            if (!isVertical)
            {
                g.DrawLine(guidePen, offsetX, h * 0.25f, offsetX + drawW, h * 0.25f);
                g.DrawLine(guidePen, offsetX, h * 0.75f, offsetX + drawW, h * 0.75f);
            }
            else
            {
                g.DrawLine(guidePen, w * 0.25f, offsetY, w * 0.25f, offsetY + drawH);
                g.DrawLine(guidePen, w * 0.75f, offsetY, w * 0.75f, offsetY + drawH);
            }

            // 3. Markings (Logical positions)
            using var font = new Font("Segoe UI", MathF.Max(8, MathF.Min(w, h) * 0.35f), FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
            
            void DrawLabel(string text, float val)
            {
                float ratio = (float)((val - min) / (max - min));
                float gx = isVertical ? w / 2 : w * ratio;
                float gy = isVertical ? h * ratio : h / 2;
                
                var size = g.MeasureString(text, font);
                float tx = isVertical ? -size.Width - 4 : gx - size.Width / 2;
                float ty = isVertical ? gy - size.Height / 2 : h + 4;

                g.DrawString(text, font, textBrush, tx, ty);
                
                using var tickPen = new Pen(Color.Orange, 2);
                if (isVertical) g.DrawLine(tickPen, 0, gy, 6, gy);
                else g.DrawLine(tickPen, gx, h, gx, h - 6);
            }

            DrawLabel("-", (float)min);
            if (min < 0 && max > 0) DrawLabel("0", 0);
            DrawLabel("+", (float)max);

            // 4. Heavy Duty End Caps
            using var capBrush = new SolidBrush(Color.FromArgb(240, 30, 35, 40));
            if (!isVertical)
            {
                g.FillRectangle(capBrush, offsetX, -2, 8, h + 4);
                g.FillRectangle(capBrush, offsetX + drawW - 8, -2, 8, h + 4);
            }
            else
            {
                g.FillRectangle(capBrush, -2, offsetY, w + 4, 8);
                g.FillRectangle(capBrush, -2, offsetY + drawH - 8, w + 4, 8);
            }
        };
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
