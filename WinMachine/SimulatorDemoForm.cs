using System;
using System.Reactive.Disposables;
using System.Windows.Forms;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Interpreters.Flow;
using WinMachine.Visualization;

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

            UI.UseFactory(form =>
            {
                if (form is Control control)
                    return new WinFormsUIVisualizer(control);

                return UI.CreateStub();
            });

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

            ConfigureVisualizationBindings(context, interpreter);
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
            // 预留：可在 Visuals 绑定后做“当前设备”描边/发光。
            // 目前用 lblActiveStep 与 txtLog 即可满足追踪需求。
        }

        private void ConfigureVisualizationBindings(FlowContext context, IVisualFlowInterpreter interpreter)
        {
            var layout =
                from v in Visuals.Start()
                select v
                    .ForAxis("X").AsLinearGuide(200, 18).Horizontal().Done()
                    .ForAxis("Z1_Axis").AsLinearGuide(180, 16).Vertical().Done()
                    .ForAxis("Z2_Axis").AsLinearGuide(180, 16).Vertical().Done()
                    .ForAxis("LeftRotate").AsRotaryTable(24).Done()
                    .ForAxis("RightRotate").AsRotaryTable(24).Done()
                    .ForCylinder("Slide").AsSlider(40, 8).Horizontal().Done()
                    .ForCylinder("LeftGrip").AsGripper(18, 6).Horizontal().Done()
                    .ForCylinder("RightGrip").AsGripper(18, 6).Horizontal().Done()
                    .AutoHighlight(pnlAxisX, "X")
                    .AutoHighlight(pnlAxisZ1, "Z1_Axis")
                    .AutoHighlight(pnlAxisZ2, "Z2_Axis")
                    .Bind(pnlAxisX).ToAxis("X").Horizontal().Done()
                    .Bind(pnlAxisZ1).ToAxis("Z1_Axis").Vertical().Done()
                    .Bind(pnlAxisZ2).ToAxis("Z2_Axis").Vertical().Done()
                    .Bind(pnlCylinderSlide).ToCylinder("Slide").Done()
                    .Bind(pnlLeftGrip).ToCylinder("LeftGrip").Done()
                    .Bind(pnlRightGrip).ToCylinder("RightGrip").Done();

            interpreter.AttachVisuals(this, context, layout);
        }

    }
}
