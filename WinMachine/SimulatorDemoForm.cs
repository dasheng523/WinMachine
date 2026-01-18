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

            var runtime = scenario.Create(_cts);
            var context = runtime.Context;
            var flow = runtime.Flow;
            var interpreter = new SimulationFlowInterpreter();
            interpreter.InitializeDevices(context);

            WireUpVisualization(context, interpreter, runtime);
            runtime.BeforeRun?.Invoke(context, _subscriptions);

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

        private void WireUpVisualization(FlowContext context, SimulationFlowInterpreter interpreter, ScenarioRuntime runtime)
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

            transferStationView.ResetModel(runtime.TransferModel);

            // 领域事件：驱动物料互换
            _subscriptions.Add(runtime.DomainEvents.Subscribe(ev =>
            {
                void Apply() => transferStationView.ApplyDomainEvent(ev);
                if (sync != null) sync.Post(_ => Apply(), null);
                else ui.BeginInvoke((Action)Apply);
            }));

            // 设备状态：驱动位置/角度/夹爪/升降
            WireAxisToView(context, "Slide", v => transferStationView.SetSlide(v), -120, 120);
            WireAxisToView(context, "LeftRotate", v => transferStationView.SetLeftRotate(v), 0, 180);
            WireAxisToView(context, "RightRotate", v => transferStationView.SetRightRotate(v), 0, 180);

            WireCylinderToView(context, "LeftLift", up => transferStationView.SetLeftLift(up));
            WireCylinderToView(context, "RightLift", up => transferStationView.SetRightLift(up));
            WireCylinderToView(context, "LeftGrip", closed => transferStationView.SetLeftGrip(closed));
            WireCylinderToView(context, "RightGrip", closed => transferStationView.SetRightGrip(closed));
        }

        private void WireAxisToView(FlowContext context, string axisId, Action<double> applyPosition, double min, double max)
        {
            var axis = context.GetDevice<SimulatorAxis>(axisId);
            if (axis == null) return;

            _subscriptions!.Add(axis.StateStream
                .Sample(TimeSpan.FromMilliseconds(16))
                .Subscribe(s =>
                {
                    if (!IsHandleCreated) return;
                    BeginInvoke((Action)(() => applyPosition(s.Position)));
                }));
        }

        private void WireCylinderToView(FlowContext context, string id, Action<bool> applyExtended)
        {
            var cyl = context.GetDevice<ISimulatorCylinder>(id);
            if (cyl == null) return;

            _subscriptions!.Add(cyl.StateStream
                .Where(s => !s.IsMoving)
                .DistinctUntilChanged(s => s.IsExtended)
                .Subscribe(s =>
                {
                    if (!IsHandleCreated) return;
                    BeginInvoke((Action)(() => applyExtended(s.IsExtended)));
                }));
        }

        private void Log(string msg)
        {
            txtLog.AppendText(msg + Environment.NewLine);
        }

        private void ResetHighlights()
        {
            // 新版 UI 的高亮由 Trace 日志文本呈现；视图本身使用设备状态变化。
        }

        private void Highlight(string deviceId, StepStatus status)
        {
            // 预留：可在 TransferStationView 上做“当前设备”描边/发光。
            // 目前用 lblActiveStep 与 txtLog 即可满足追踪需求。
        }

    }
}
