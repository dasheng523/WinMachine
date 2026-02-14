using System;
using System.Reactive.Disposables;
using System.Windows.Forms;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Interpreters.Flow;
using Machine.Framework.Visualization;
using Machine.Framework.Core.Primitives; // 引入 AxisID/CylinderID 等类型
using Machine.Framework.Visualization.WinForms;
using static WinMachine.MachineDevices;  // 引入静态设备定义

namespace WinMachine
{
    public partial class SimulatorDemoForm : Form
    {
        private readonly SimulationFlowScenario[] _scenarios = SimulationFlowScenarios.All;
        private readonly IUIVisualizer _visualizer;

        private CancellationTokenSource? _cts;
        private CompositeDisposable? _subscriptions;

        private CancellationTokenSource? _previewCts;
        private CompositeDisposable? _previewSubscriptions;

        public SimulatorDemoForm()
        {
            InitializeComponent();

            UI.UseFactory(form =>
            {
                if (form is Control control)
                    return new KinematicVisualizer(control);

                return UI.CreateStub();
            });

            lstScenarios.DataSource = _scenarios;
            lstScenarios.DisplayMember = nameof(SimulationFlowScenario.Name);

            _visualizer = UI.Link(this);

            btnRun.Click += async (_, __) => await RunSelectedAsync();
            btnCancel.Click += (_, __) => _cts?.Cancel();

            lstScenarios.SelectedIndexChanged += (_, __) => ShowPreview();
            ShowPreview();
        }

        private void ShowPreview()
        {
            // Running 时不打断（Run 会切换到运行态上下文）
            if (_cts != null && !_cts.IsCancellationRequested)
                return;

            if (lstScenarios.SelectedItem is not SimulationFlowScenario scenario)
                return;

            _previewSubscriptions?.Dispose();
            _previewSubscriptions = new CompositeDisposable();

            _previewCts?.Dispose();
            _previewCts = new CancellationTokenSource();

            txtLog.Clear();
            lblActiveStep.Text = $"Preview: {scenario.Name}";
            ResetHighlights();

            var runtime = scenario.Create(_previewCts);
            var context = runtime.Context;
            var interpreter = new SimulationFlowInterpreter();
            interpreter.InitializeDevices(context);

            ConfigureVisualizationBindings(context, interpreter);
            runtime.BeforeRun?.Invoke(context, _previewSubscriptions);
        }

        private async System.Threading.Tasks.Task RunSelectedAsync()
        {
            if (lstScenarios.SelectedItem is not SimulationFlowScenario scenario)
                return;

            btnRun.Enabled = false;
            btnCancel.Enabled = true;
            lstScenarios.Enabled = false;
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
                _cts?.Dispose();
                _cts = null;
                _subscriptions?.Dispose();
                _subscriptions = null;

                btnRun.Enabled = true;
                btnCancel.Enabled = false;
                lstScenarios.Enabled = true;

                // Run 结束后回到静态预览态（选中项对应画面）
                ShowPreview();
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

        private void ResetHighlights() { }
        private void Highlight(string deviceId, StepStatus status) { }

        private void ConfigureVisualizationBindings(FlowContext context, IVisualFlowInterpreter interpreter)
        {
            // --- 视觉布局定义 (Layout DSL) ---
            var layout = Visuals.Start()
                // 1. 左侧结构样式
                .For(Z1_Lift).AsLinearGuide(180, 20).Vertical().Done()
                .For(R1_Rotate).AsRotaryTable(60).Done()
                .For(C1_Left_Grip1).AsGripper(15, 5).WithSize(24, 32).WithPivot(0.5, 0).Done()
                .For(C1_Left_Grip2).AsGripper(15, 5).WithSize(24, 32).WithPivot(0.5, 0).Done()
                .For(C1_Left_Grip3).AsGripper(15, 5).WithSize(24, 32).WithPivot(0.5, 0).Done()
                .For(C1_Left_Grip4).AsGripper(15, 5).WithSize(24, 32).WithPivot(0.5, 0).Done()

                // 2. 右侧结构样式
                .For(Z2_Lift).AsLinearGuide(180, 20).Vertical().Done()
                .For(R2_Rotate).AsRotaryTable(60).Done()
                .For(C2_Right_Grip1).AsGripper(15, 5).WithSize(24, 32).WithPivot(0.5, 0).Done()
                .For(C2_Right_Grip2).AsGripper(15, 5).WithSize(24, 32).WithPivot(0.5, 0).Done()
                .For(C2_Right_Grip3).AsGripper(15, 5).WithSize(24, 32).WithPivot(0.5, 0).Done()
                .For(C2_Right_Grip4).AsGripper(15, 5).WithSize(24, 32).WithPivot(0.5, 0).Done()

                // 3. 通用/单体测试样式
                .For(SlideCyl).AsSlideBlock(120).Horizontal().Done()
                .For(Test_Slide).AsSlideBlock(120).Horizontal().Done()
                .For(Test_Elevator).AsSlideBlock(120).Vertical().Done()
                .For(Test_Gripper).AsGripper(20, 5).WithSize(32, 48).Done()
                .For(Test_Suction).AsSuctionPen(30).Done()
                .For(Test_Linear).AsLinearGuide(180, 24).Vertical().Done()
                
                // 确保 Test_Rotary 及其挂载的夹爪有明确样式
                .For(Test_Rotary).AsRotaryTable(64).Done()
                .For(C1_Left_Grip1).AsGripper(15, 5).WithSize(24, 32).WithPivot(0.5, 0).Done()
                .For(C1_Left_Grip2).AsGripper(15, 5).WithSize(24, 32).WithPivot(0.5, 0).Done()
                .For(C2_Right_Grip1).AsGripper(15, 5).WithSize(24, 32).WithPivot(0.5, 0).Done()
                .For(C2_Right_Grip2).AsGripper(15, 5).WithSize(24, 32).WithPivot(0.5, 0).Done()

                // 4. 定义绑定关系 (Binding)
                .Bind(pnlCanvas)
                    .TargetRoot("Machine")
                .Done();

            _visualizer
                .Visuals(layout.Build())
                .ObserveInterpreter(interpreter)
                .ObserveContext(context);
        }
    }
}
