using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Core.Simulation;

namespace Machine.Framework.Visualization;

public sealed class WinFormsUIVisualizer : IUIVisualizer, IDeviceVisualRegistry, IDisposable
{
    private readonly Control _root;
    private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
    private readonly List<PanelBinding> _bindings = new List<PanelBinding>();
    private readonly Dictionary<string, List<Control>> _highlightTargets = new Dictionary<string, List<Control>>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Control, Color> _originalBackColors = new Dictionary<Control, Color>();
    private readonly Dictionary<string, AxisStyleConfig> _axisStyles = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CylinderStyleConfig> _cylinderStyles = new(StringComparer.OrdinalIgnoreCase);
    private FlowContext? _context;

    public WinFormsUIVisualizer(Control root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
    }

    public IUIVisualizer ObserveInterpreter(IVisualFlowInterpreter interpreter)
    {
        if (interpreter == null) throw new ArgumentNullException(nameof(interpreter));
        _subscriptions.Add(interpreter.TraceStream.Subscribe(update => ApplyHighlight(update.TargetDevice, update.Status)));
        return this;
    }

    public IUIVisualizer ObserveContext(FlowContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        AttachAllBindings();
        return this;
    }

    public IUIVisualizer AutoHighlight(object panel, DeviceID id)
    {
        RegisterHighlightTarget(panel, id.Name);
        return this;
    }

    public IUIVisualizer Visuals(Action<IDeviceVisualRegistry> registryConfig)
    {
        registryConfig?.Invoke(this);
        return this;
    }

    public IBindingBuilder Bind(object panel)
    {
        if (panel is not Control control)
            return new WinFormsBindingBuilder(this, null);

        return new WinFormsBindingBuilder(this, control);
    }

    IDeviceVisualRegistry IDeviceVisualRegistry.AutoHighlight(object panel, DeviceID id)
    {
        RegisterHighlightTarget(panel, id.Name);
        return this;
    }

    public IAxisVisualBuilder For(AxisID axis) => new WinFormsAxisVisualBuilder(_axisStyles, axis.Name);

    public ICylinderVisualBuilder For(CylinderID cylinder) => new WinFormsCylinderVisualBuilder(_cylinderStyles, cylinder.Name);

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    internal void RegisterBinding(PanelBinding binding)
    {
        if (binding.Panel == null || string.IsNullOrWhiteSpace(binding.DeviceId)) return;

        _bindings.Add(binding);
        RegisterHighlightTarget(binding.Panel, binding.DeviceId);
        EnsureLabel(binding);
        TryAttach(binding);
    }

    private void RegisterHighlightTarget(object panel, string deviceId)
    {
        if (panel is not Control control) return;
        if (string.IsNullOrWhiteSpace(deviceId)) return;

        if (!_highlightTargets.TryGetValue(deviceId, out var list))
        {
            list = new List<Control>();
            _highlightTargets[deviceId] = list;
        }

        if (!list.Contains(control))
            list.Add(control);

        if (!_originalBackColors.ContainsKey(control))
            _originalBackColors[control] = control.BackColor;
    }

    private void EnsureLabel(PanelBinding binding)
    {
        if (binding.Panel == null) return;

        const string labelName = "lblVisualHint";
        Label? label = null;

        foreach (Control child in binding.Panel.Controls)
        {
            if (child is Label l && l.Name == labelName)
            {
                label = l;
                break;
            }
        }

        if (label == null)
        {
            label = new Label
            {
                Name = labelName,
                AutoSize = true,
                ForeColor = Color.FromArgb(220, 230, 230, 230),
                BackColor = Color.Transparent,
                Location = new Point(6, 6)
            };
            binding.Panel.Controls.Add(label);
        }

        label.Text = $"{binding.Kind}:{binding.DeviceId}";
    }

    private void AttachAllBindings()
    {
        foreach (var binding in _bindings)
        {
            TryAttach(binding);
        }
    }

    private void TryAttach(PanelBinding binding)
    {
        if (_context == null) return;
        if (binding.Panel == null || string.IsNullOrWhiteSpace(binding.DeviceId)) return;

        switch (binding.Kind)
        {
            case DeviceKind.Axis:
                AttachAxis(binding);
                break;
            case DeviceKind.Cylinder:
                AttachCylinder(binding);
                break;
        }
    }

    private void AttachAxis(PanelBinding binding)
    {
        var axis = _context!.GetDevice<SimulatorAxis>(binding.DeviceId);
        if (axis == null) return;

        var valueLabel = EnsureValueLabel(binding.Panel!);
        var canvas = EnsureAxisCanvas(binding.Panel!);
        if (_axisStyles.TryGetValue(binding.DeviceId, out var style))
        {
            canvas.Style = style.Style;
            canvas.Length = style.Length;
            canvas.SliderWidth = style.SliderWidth;
            canvas.Radius = style.Radius;
            canvas.Vertical = style.IsVertical;
            canvas.Reversed = style.IsReversed;
        }
        else
        {
            canvas.Vertical = binding.Vertical == true;
        }

        _subscriptions.Add(axis.StateStream
            .Sample(TimeSpan.FromMilliseconds(50))
            .Subscribe(state =>
            {
                void Apply()
                {
                    var pos = state.Position;
                    valueLabel.Text = $"Pos: {pos:0.0}  {(state.IsMoving ? "Moving" : "Stop")}";
                    canvas.Min = axis.TravelMin;
                    canvas.Max = axis.TravelMax;
                    canvas.Value = pos;
                    canvas.Invalidate();
                }

                if (_root.IsHandleCreated && _root.InvokeRequired)
                    _root.BeginInvoke((Action)Apply);
                else
                    Apply();
            }));
    }

    private void AttachCylinder(PanelBinding binding)
    {
        var cyl = _context!.GetDevice<ISimulatorCylinder>(binding.DeviceId);
        if (cyl == null) return;

        var valueLabel = EnsureValueLabel(binding.Panel!);
        var canvas = EnsureCylinderCanvas(binding.Panel!);
        if (_cylinderStyles.TryGetValue(binding.DeviceId, out var style))
        {
            canvas.Style = style.Style;
            canvas.BlockSize = style.BlockSize;
            canvas.OpenWidth = style.OpenWidth;
            canvas.CloseWidth = style.CloseWidth;
            canvas.Diameter = style.Diameter;
            canvas.Vertical = style.IsVertical;
            canvas.Reversed = style.IsReversed;
        }
        else
        {
            canvas.Vertical = binding.Vertical == true;
        }

        _subscriptions.Add(cyl.StateStream
            .Subscribe(state =>
            {
                void Apply()
                {
                    var status = state.IsMoving ? "Moving" : (state.IsExtended ? "Extended" : "Retracted");
                    valueLabel.Text = $"{status}";
                    canvas.Value = state.Position;
                    canvas.Invalidate();
                }

                if (_root.IsHandleCreated && _root.InvokeRequired)
                    _root.BeginInvoke((Action)Apply);
                else
                    Apply();
            }));
    }

    private static Label EnsureValueLabel(Control panel)
    {
        const string labelName = "lblVisualValue";
        foreach (Control child in panel.Controls)
        {
            if (child is Label l && l.Name == labelName)
                return l;
        }

        var label = new Label
        {
            Name = labelName,
            AutoSize = true,
            ForeColor = Color.FromArgb(200, 200, 220, 220),
            BackColor = Color.Transparent,
            Location = new Point(6, 26)
        };
        panel.Controls.Add(label);
        return label;
    }

    private static AxisCanvas EnsureAxisCanvas(Control panel)
    {
        const string canvasName = "axisCanvas";
        foreach (Control child in panel.Controls)
        {
            if (child is AxisCanvas c && c.Name == canvasName)
                return c;
        }

        var canvas = new AxisCanvas
        {
            Name = canvasName,
            Dock = DockStyle.Fill,
        };

        panel.Controls.Add(canvas);
        canvas.SendToBack();
        return canvas;
    }

    private static CylinderCanvas EnsureCylinderCanvas(Control panel)
    {
        const string canvasName = "cylinderCanvas";
        foreach (Control child in panel.Controls)
        {
            if (child is CylinderCanvas c && c.Name == canvasName)
                return c;
        }

        var canvas = new CylinderCanvas
        {
            Name = canvasName,
            Dock = DockStyle.Fill,
        };

        panel.Controls.Add(canvas);
        canvas.SendToBack();
        return canvas;
    }

    private void ApplyHighlight(string deviceId, StepStatus status)
    {
        if (string.IsNullOrWhiteSpace(deviceId)) return;

        void Apply()
        {
            if (!_highlightTargets.TryGetValue(deviceId, out var targets)) return;

            foreach (var control in targets)
            {
                if (!_originalBackColors.TryGetValue(control, out var original))
                {
                    _originalBackColors[control] = control.BackColor;
                    original = control.BackColor;
                }

                control.BackColor = status switch
                {
                    StepStatus.Running => Color.FromArgb(50, 110, 80),
                    StepStatus.Completed => original,
                    StepStatus.Error => Color.FromArgb(120, 45, 45),
                    _ => original
                };
            }
        }

        if (_root.IsHandleCreated && _root.InvokeRequired)
            _root.BeginInvoke((Action)Apply);
        else
            Apply();
    }

    internal sealed class PanelBinding
    {
        public Control? Panel { get; init; }
        public string DeviceId { get; set; } = string.Empty;
        public DeviceKind Kind { get; set; }
        public bool? Vertical { get; set; }
        public Func<double, object>? Mapper { get; set; }
    }

    internal enum DeviceKind
    {
        Axis,
        Cylinder
    }

    private sealed class WinFormsBindingBuilder : IBindingBuilder
    {
        private readonly WinFormsUIVisualizer _owner;
        private readonly PanelBinding _binding;

        public WinFormsBindingBuilder(WinFormsUIVisualizer owner, Control? panel)
        {
            _owner = owner;
            _binding = new PanelBinding { Panel = panel };
        }

        public IBindingBuilder ToAxis(AxisID axis)
        {
            _binding.DeviceId = axis.Name;
            _binding.Kind = DeviceKind.Axis;
            _owner.RegisterBinding(_binding);
            return this;
        }

        public IBindingBuilder ToCylinder(CylinderID cylinder)
        {
            _binding.DeviceId = cylinder.Name;
            _binding.Kind = DeviceKind.Cylinder;
            _owner.RegisterBinding(_binding);
            return this;
        }

        public IBindingBuilder Vertical()
        {
            _binding.Vertical = true;
            return this;
        }

        public IBindingBuilder Horizontal()
        {
            _binding.Vertical = false;
            return this;
        }

        public IBindingBuilder TargetRoot(string rootName)
        {
            // WinFormsUIVisualizer 不支持 TargetRoot 渲染，这是 KinematicVisualizer 的特性
            // 作为一个 fallback，我们可以记录日志或者忽略
            return this;
        }

        public IBindingBuilder Map(Func<double, object> mapper)
        {
            _binding.Mapper = mapper;
            return this;
        }
    }

    private sealed class WinFormsAxisVisualBuilder : IAxisVisualBuilder
    {
        private readonly Dictionary<string, AxisStyleConfig> _styles;
        private readonly string _axisId;

        public WinFormsAxisVisualBuilder(Dictionary<string, AxisStyleConfig> styles, string axisId)
        {
            _styles = styles;
            _axisId = axisId;
        }

        private AxisStyleConfig GetConfig()
        {
            if (!_styles.TryGetValue(_axisId, out var cfg))
            {
                cfg = new AxisStyleConfig();
                _styles[_axisId] = cfg;
            }
            return cfg;
        }

        public IAxisVisualBuilder AsLinearGuide(double length, double sliderWidth)
        {
            var cfg = GetConfig();
            cfg.Style = AxisVisualStyle.LinearGuide;
            cfg.Length = length;
            cfg.SliderWidth = sliderWidth;
            return this;
        }

        public IAxisVisualBuilder AsRotaryTable(double radius)
        {
            var cfg = GetConfig();
            cfg.Style = AxisVisualStyle.RotaryTable;
            cfg.Radius = radius;
            return this;
        }

        public IAxisVisualBuilder AsCustom(string modelPath) => this;
        public IAxisVisualBuilder Horizontal() { GetConfig().IsVertical = false; return this; }
        public IAxisVisualBuilder Vertical() { GetConfig().IsVertical = true; return this; }
        public IAxisVisualBuilder Forward() { GetConfig().IsReversed = false; return this; }
        public IAxisVisualBuilder Reversed() { GetConfig().IsReversed = true; return this; }

        public IAxisVisualBuilder WithPivot(double x, double y) => this; // Stub for compatibility
        public IAxisVisualBuilder WithSize(double w, double h) => this; // Stub for compatibility
    }

    private sealed class WinFormsCylinderVisualBuilder : ICylinderVisualBuilder
    {
        private readonly Dictionary<string, CylinderStyleConfig> _styles;
        private readonly string _cylinderId;

        public WinFormsCylinderVisualBuilder(Dictionary<string, CylinderStyleConfig> styles, string cylinderId)
        {
            _styles = styles;
            _cylinderId = cylinderId;
        }

        private CylinderStyleConfig GetConfig()
        {
            if (!_styles.TryGetValue(_cylinderId, out var cfg))
            {
                cfg = new CylinderStyleConfig();
                _styles[_cylinderId] = cfg;
            }
            return cfg;
        }

        public ICylinderVisualBuilder AsSlideBlock(double? blockSize = null)
        {
            var cfg = GetConfig();
            cfg.Style = CylinderVisualStyle.SlideBlock;
            cfg.BlockSize = blockSize;
            return this;
        }

        public ICylinderVisualBuilder AsGripper(double openWidth, double closeWidth)
        {
            var cfg = GetConfig();
            cfg.Style = CylinderVisualStyle.Gripper;
            cfg.OpenWidth = openWidth;
            cfg.CloseWidth = closeWidth;
            return this;
        }

        public ICylinderVisualBuilder AsSuctionPen(double diameter)
        {
            var cfg = GetConfig();
            cfg.Style = CylinderVisualStyle.SuctionPen;
            cfg.Diameter = diameter;
            return this;
        }

        public ICylinderVisualBuilder AsCustom(string modelPath) => this;
        public ICylinderVisualBuilder Horizontal() { GetConfig().IsVertical = false; return this; }
        public ICylinderVisualBuilder Vertical() { GetConfig().IsVertical = true; return this; }
        public ICylinderVisualBuilder Forward() { GetConfig().IsReversed = false; return this; }
        public ICylinderVisualBuilder Reversed() { GetConfig().IsReversed = true; return this; }
        
        public ICylinderVisualBuilder WithPivot(double x, double y) => this; 
        public ICylinderVisualBuilder WithSize(double w, double h) => this;
    }

    private sealed class AxisStyleConfig
    {
        public AxisVisualStyle Style { get; set; } = AxisVisualStyle.LinearGuide;
        public double Length { get; set; } = 120;
        public double SliderWidth { get; set; } = 18;
        public double Radius { get; set; } = 20;
        public bool IsVertical { get; set; }
        public bool IsReversed { get; set; }
    }

    private sealed class CylinderStyleConfig
    {
        public CylinderVisualStyle Style { get; set; } = CylinderVisualStyle.SlideBlock;
        public double? BlockSize { get; set; }
        public double OpenWidth { get; set; } = 18;
        public double CloseWidth { get; set; } = 6;
        public double Diameter { get; set; } = 10;
        public bool IsVertical { get; set; }
        public bool IsReversed { get; set; }
    }
}
