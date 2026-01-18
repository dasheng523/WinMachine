using System;
using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Interpreters.Flow;

namespace WinMachine
{
    // Demo Types
    public enum DemoAxis { X, Z }
    public enum DemoIO { PenA }

    public partial class SimulatorDemoForm : Form
    {

        private readonly SimulationFlowScenario[] _scenarios = SimulationFlowScenarios.All;
        private CancellationTokenSource? _cts;
        private CompositeDisposable? _subscriptions;

        public SimulatorDemoForm()
        {
            InitializeComponent();

            lstScenarios.DataSource = _scenarios;
            lstScenarios.DisplayMember = nameof(SimulationFlowScenario.Name);

            btnRun.Click += async (_, __) => await RunSelectedAsync();
            btnCancel.Click += (_, __) => _cts?.Cancel();
        }

        private async System.Threading.Tasks.Task RunSelectedAsync()
        {
            if (lstScenarios.SelectedItem is not SimulationFlowScenario scenario)
                return;

            btnRun.Enabled = false;
            btnCancel.Enabled = true;
            txtLog.Clear();
            lblActiveStep.Text = "Active: -";
            ResetHighlights();

            _subscriptions?.Dispose();
            _subscriptions = new CompositeDisposable();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            var (context, flow, beforeRun) = scenario.Create(_cts);
            var interpreter = new SimulationFlowInterpreter();
            interpreter.InitializeDevices(context);

            WireUpVisualization(context, interpreter);
            beforeRun?.Invoke();

            Log($"--- RUN {scenario.Name} ---");

            try
            {
                await interpreter.RunAsync(flow, context);
                Log("--- COMPLETED ---");
            }
            catch (OperationCanceledException)
            {
                Log("--- CANCELED ---");
            }
            catch (Exception ex)
            {
                Log($"--- ERROR: {ex.Message} ---");
            }
            finally
            {
                btnRun.Enabled = true;
                btnCancel.Enabled = false;
            }
        }

        private void WireUpVisualization(FlowContext context, SimulationFlowInterpreter interpreter)
        {
            var ui = this;
            var sync = System.Threading.SynchronizationContext.Current;

            _subscriptions!.Add(interpreter.TraceStream.Subscribe(update =>
            {
                void Apply()
                {
                    lblActiveStep.Text = $"Active: [{update.Status}] {update.TargetDevice} :: {update.Name}";
                    Highlight(update.TargetDevice, update.Status);
                    Log($"[{update.Status}] {update.TargetDevice} :: {update.Name}");
                }

                if (sync != null) sync.Post(_ => Apply(), null);
                else ui.BeginInvoke((Action)Apply);
            }));

            WireAxis(context, "X", pnlAxisX, AxisDirection.Horizontal, 0, 1000, origin: new Point(40, 60));
            WireAxis(context, "Y", pnlAxisY, AxisDirection.Horizontal, 0, 1000, origin: new Point(40, 110));
            WireAxis(context, "Z", pnlAxisZ, AxisDirection.Vertical, 0, 200, origin: new Point(220, 60));
            WireAxis(context, "Z1_Axis", pnlAxisZ, AxisDirection.Vertical, 0, 100, origin: new Point(220, 60));
            WireAxis(context, "Rotate", pnlAxisRotate, AxisDirection.Rotate, 0, 180, origin: new Point(220, 140));

            WireCylinder(context, "Gripper", pnlCylGripper);
            WireCylinder(context, "Clamp", pnlCylClamp);
            WireVacuum(context, "VAC_1", pnlVac1);
        }

        private void WireAxis(FlowContext context, string axisId, Panel panel, AxisDirection dir, double min, double max, Point origin)
        {
            var axis = context.GetDevice<SimulatorAxis>(axisId);
            if (axis == null) return;

            panel.Location = origin;
            panel.BackColor = ColorForAxis(axisId);

            _subscriptions!.Add(axis.StateStream
                .Sample(TimeSpan.FromMilliseconds(16))
                .Subscribe(s =>
                {
                    void Apply()
                    {
                        if (dir == AxisDirection.Horizontal)
                        {
                            var x = origin.X + MapToPixels(s.Position, min, max, 520);
                            panel.Location = new Point(x, origin.Y);
                        }
                        else if (dir == AxisDirection.Vertical)
                        {
                            var y = origin.Y + MapToPixels(s.Position, min, max, 420);
                            panel.Location = new Point(origin.X, y);
                        }
                        else if (dir == AxisDirection.Rotate)
                        {
                            // 用颜色强度表示角度（简化）：0 -> 暗, max -> 亮
                            var t = Clamp01((s.Position - min) / Math.Max(1e-6, (max - min)));
                            var c = Color.FromArgb(255, (int)(80 + 120 * t), (int)(60 + 80 * t), (int)(140 + 60 * t));
                            panel.BackColor = c;
                        }
                    }

                    if (IsHandleCreated) BeginInvoke((Action)Apply);
                }));
        }

        private void WireCylinder(FlowContext context, string id, Panel panel)
        {
            var cyl = context.GetDevice<ISimulatorCylinder>(id);
            if (cyl == null) return;

            _subscriptions!.Add(cyl.StateStream
                .Subscribe(s =>
                {
                    void Apply()
                    {
                        panel.BackColor = s.IsMoving
                            ? Color.Goldenrod
                            : (s.IsExtended ? Color.LimeGreen : Color.DimGray);
                    }

                    if (IsHandleCreated) BeginInvoke((Action)Apply);
                }));
        }

        private void WireVacuum(FlowContext context, string id, Panel panel)
        {
            var vac = context.GetDevice<ISimulatorVacuum>(id);
            if (vac == null) return;

            _subscriptions!.Add(vac.StateStream
                .Subscribe(s =>
                {
                    void Apply()
                    {
                        panel.BackColor = s.IsChanging
                            ? Color.Goldenrod
                            : (s.IsOn ? Color.DeepSkyBlue : Color.DimGray);
                    }

                    if (IsHandleCreated) BeginInvoke((Action)Apply);
                }));
        }

        private void Highlight(string deviceId, StepStatus status)
        {
            Panel? panel = deviceId switch
            {
                "X" => pnlAxisX,
                "Y" => pnlAxisY,
                "Z" => pnlAxisZ,
                "Z1_Axis" => pnlAxisZ,
                "Rotate" => pnlAxisRotate,
                "Gripper" => pnlCylGripper,
                "Clamp" => pnlCylClamp,
                "VAC_1" => pnlVac1,
                _ => null
            };

            if (panel == null) return;

            var baseColor = panel.BackColor;
            if (status == StepStatus.Running)
                panel.BorderStyle = BorderStyle.FixedSingle;
            else
                panel.BorderStyle = BorderStyle.None;
        }

        private void ResetHighlights()
        {
            pnlAxisX.BorderStyle = BorderStyle.None;
            pnlAxisY.BorderStyle = BorderStyle.None;
            pnlAxisZ.BorderStyle = BorderStyle.None;
            pnlAxisRotate.BorderStyle = BorderStyle.None;
            pnlCylGripper.BorderStyle = BorderStyle.None;
            pnlCylClamp.BorderStyle = BorderStyle.None;
            pnlVac1.BorderStyle = BorderStyle.None;
        }

        private void Log(string msg)
        {
            txtLog.AppendText(msg + Environment.NewLine);
        }

        private static int MapToPixels(double val, double min, double max, int spanPx)
        {
            if (max <= min) return 0;
            var t = Clamp01((val - min) / (max - min));
            return (int)(t * spanPx);
        }

        private static double Clamp01(double t) => t < 0 ? 0 : (t > 1 ? 1 : t);

        private static Color ColorForAxis(string axisId) => axisId switch
        {
            "X" => Color.SteelBlue,
            "Y" => Color.MediumSeaGreen,
            "Z" => Color.Orange,
            "Z1_Axis" => Color.Orange,
            "Rotate" => Color.MediumPurple,
            _ => Color.White
        };

        private enum AxisDirection { Horizontal, Vertical, Rotate }

    }
}
