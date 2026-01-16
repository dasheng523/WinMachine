using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reactive.Linq;
using System.Threading;
using Machine.Framework.Devices.Motion.Implementations.Simulator.Models;
using Machine.Framework.Devices.Motion.Implementations.Simulator.Visuals;

namespace Machine.Framework.Devices.Motion.Implementations.Simulator.Rendering
{
    public class AxisRendererControl : UserControl
    {
        private readonly ISimulatorAxis _axis;
        private readonly AxisVisualConfig _config;
        private IDisposable? _subscription;
        private AxisState? _lastState;

        private Brush _trackBrush = Brushes.Gray;
        private Brush _sliderBrush = Brushes.DodgerBlue;
        private Pen _borderPen = Pens.Black;

        public AxisRendererControl(ISimulatorAxis axis, AxisVisualConfig config)
        {
            _axis = axis;
            _config = config;
            
            // Turn on double buffering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | 
                          ControlStyles.UserPaint | 
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            var syncContext = SynchronizationContext.Current;
            if (syncContext != null)
            {
                _subscription = _axis.StateStream
                    .Sample(TimeSpan.FromMilliseconds(33)) // ~30 FPS
                    .ObserveOn(syncContext)
                    .Subscribe(state => 
                    {
                        _lastState = state;
                        this.Invalidate(); 
                    });
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var w = this.Width;
            var h = this.Height;
            var pos = _lastState?.Position ?? 0;
            var range = _axis.TravelMax - _axis.TravelMin;
            if (range <= 0) range = 100;

            float ratio = (float)((pos - _axis.TravelMin) / range);
            if (ratio < 0) ratio = 0;
            if (ratio > 1) ratio = 1;

            if (_config.Direction == FlowDirection.TopDown || _config.Direction == FlowDirection.BottomUp)
            {
                float trackX = w / 2.0f - 5;
                g.FillRectangle(_trackBrush, trackX, 10, 10, h - 20);

                float sliderY;
                float sliderH = 20;
                float availableH = h - 20 - sliderH;

                if (_config.Direction == FlowDirection.TopDown)
                    sliderY = 10 + ratio * availableH;
                else
                    sliderY = 10 + (1 - ratio) * availableH;

                g.FillRectangle(_sliderBrush, 2, sliderY, w - 4, sliderH);
                g.DrawRectangle(_borderPen, 2, sliderY, w - 4, sliderH);
                
                g.DrawString($"Pos: {pos:F1}", this.Font, Brushes.White, 5, sliderY + 4);
            }
            else
            {
                float trackY = h / 2.0f - 5;
                g.FillRectangle(_trackBrush, 10, trackY, w - 20, 10);

                float sliderW = 30;
                float sliderX;
                float availableW = w - 20 - sliderW;

                if (_config.Direction == FlowDirection.LeftToRight)
                    sliderX = 10 + ratio * availableW;
                else
                    sliderX = 10 + (1 - ratio) * availableW;

                g.FillRectangle(_sliderBrush, sliderX, 2, sliderW, h - 4);
                g.DrawRectangle(_borderPen, sliderX, 2, sliderW, h - 4);

                g.DrawString($"{pos:F0}", this.Font, Brushes.White, sliderX + 2, h / 2 - 5);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _subscription?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
