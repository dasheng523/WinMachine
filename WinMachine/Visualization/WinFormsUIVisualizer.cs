using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Simulation;

namespace WinMachine.Visualization;

internal sealed class WinFormsUIVisualizer : IUIVisualizer, IDeviceVisualRegistry, IDisposable
{
    private readonly Control _root;
    private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
    private readonly List<PanelBinding> _bindings = new List<PanelBinding>();
    private readonly Dictionary<string, List<Control>> _highlightTargets = new Dictionary<string, List<Control>>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Control, Color> _originalBackColors = new Dictionary<Control, Color>();
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

    public IUIVisualizer AutoHighlight(object panel, string deviceId)
    {
        RegisterHighlightTarget(panel, deviceId);
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

    IDeviceVisualRegistry IDeviceVisualRegistry.AutoHighlight(object panel, string deviceId)
    {
        RegisterHighlightTarget(panel, deviceId);
        return this;
    }

    public IAxisVisualBuilder ForAxis(string axisId) => new WinFormsAxisVisualBuilder();

    public ICylinderVisualBuilder ForCylinder(string cylinderId) => new WinFormsCylinderVisualBuilder();

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
        var bar = EnsureBar(binding.Panel!, binding.Vertical == true);

        _subscriptions.Add(axis.StateStream
            .Sample(TimeSpan.FromMilliseconds(50))
            .Subscribe(state =>
            {
                void Apply()
                {
                    var pos = state.Position;
                    valueLabel.Text = $"Pos: {pos:0.0}  {(state.IsMoving ? "Moving" : "Stop")}";
                    UpdateBar(bar, binding.Panel!, binding.Vertical == true, axis.TravelMin, axis.TravelMax, pos);
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
        var bar = EnsureBar(binding.Panel!, binding.Vertical == true);

        _subscriptions.Add(cyl.StateStream
            .DistinctUntilChanged(s => (s.IsExtended, s.IsMoving))
            .Subscribe(state =>
            {
                void Apply()
                {
                    var status = state.IsMoving ? "Moving" : (state.IsExtended ? "Extended" : "Retracted");
                    valueLabel.Text = $"{status}";
                    var pos = state.IsExtended ? 1.0 : 0.0;
                    UpdateBar(bar, binding.Panel!, binding.Vertical == true, 0, 1, pos);
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

    private static Panel EnsureBar(Control panel, bool vertical)
    {
        const string barName = "pnlVisualBar";
        foreach (Control child in panel.Controls)
        {
            if (child is Panel p && p.Name == barName)
                return p;
        }

        var bar = new Panel
        {
            Name = barName,
            BackColor = Color.FromArgb(80, 140, 180),
        };

        if (vertical)
        {
            bar.Location = new Point(panel.Width - 18, panel.Height - 8);
            bar.Size = new Size(8, 8);
            bar.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        }
        else
        {
            bar.Location = new Point(6, panel.Height - 12);
            bar.Size = new Size(8, 6);
            bar.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        }

        panel.Controls.Add(bar);
        bar.BringToFront();
        return bar;
    }

    private static void UpdateBar(Panel bar, Control panel, bool vertical, double min, double max, double value)
    {
        if (max <= min) max = min + 1;
        var t = (value - min) / (max - min);
        if (t < 0) t = 0;
        if (t > 1) t = 1;

        if (vertical)
        {
            var full = panel.Height - 16;
            var h = Math.Max(4, (int)(full * t));
            bar.Size = new Size(8, h);
            bar.Location = new Point(panel.Width - 18, panel.Height - 8 - h);
        }
        else
        {
            var full = panel.Width - 16;
            var w = Math.Max(6, (int)(full * t));
            bar.Size = new Size(w, 6);
            bar.Location = new Point(6, panel.Height - 12);
        }
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

        public IBindingBuilder ToAxis(string axisId)
        {
            _binding.DeviceId = axisId;
            _binding.Kind = DeviceKind.Axis;
            _owner.RegisterBinding(_binding);
            return this;
        }

        public IBindingBuilder ToCylinder(string cylinderId)
        {
            _binding.DeviceId = cylinderId;
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

        public IBindingBuilder Map(Func<double, object> mapper)
        {
            _binding.Mapper = mapper;
            return this;
        }
    }

    private sealed class WinFormsAxisVisualBuilder : IAxisVisualBuilder
    {
        public IAxisVisualBuilder AsLinearGuide(double length, double sliderWidth) => this;
        public IAxisVisualBuilder AsRotaryTable(double radius) => this;
        public IAxisVisualBuilder AsCustom(string modelPath) => this;
        public IAxisVisualBuilder Horizontal() => this;
        public IAxisVisualBuilder Vertical() => this;
        public IAxisVisualBuilder Forward() => this;
        public IAxisVisualBuilder Reversed() => this;
    }

    private sealed class WinFormsCylinderVisualBuilder : ICylinderVisualBuilder
    {
        public ICylinderVisualBuilder AsSlider(double width, double height) => this;
        public ICylinderVisualBuilder AsGripper(double openWidth, double closeWidth) => this;
        public ICylinderVisualBuilder AsSuctionPen(double diameter) => this;
        public ICylinderVisualBuilder AsCustom(string modelPath) => this;
        public ICylinderVisualBuilder Horizontal() => this;
        public ICylinderVisualBuilder Vertical() => this;
        public ICylinderVisualBuilder Forward() => this;
        public ICylinderVisualBuilder Reversed() => this;
    }
}
