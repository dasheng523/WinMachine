using System;
using System.Reactive.Disposables;
using System.Windows.Forms;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Interpreters.Flow;
using Machine.Framework.Visualization;
using Machine.Framework.Core.Primitives; // 引入 AxisID/CylinderID 等类型
using static WinMachine.MachineDevices;  // 引入静态设备定义

namespace WinMachine
{
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

        private void ResetHighlights() { }
        private void Highlight(string deviceId, StepStatus status) { }

        private void ConfigureVisualizationBindings(FlowContext context, IVisualFlowInterpreter interpreter)
        {
            // 使用全新的 Hybrid Layout 语法
            // 宏观布局 (Bind/TargetRoot) + 微观控制 (WithPivot/WithSize)
            
            var layout =
                from v in Visuals.Start()
                select v
                    // --- 定义视觉模型 (外观与锚点) ---
                    // 左侧塔
                    .For(Z1_Lift).AsLinearGuide(180, 16).Vertical().Done()
                    .For(R1_Rotate).AsRotaryTable(60).WithPivot(0.5, 0.5).Done() // 确保以中心旋转
                    
                    // 夹爪因为是十字布局，需要精细定义尺寸以便计算偏移
                    .For(C1_Left_Grip1).AsGripper(15, 5).WithSize(20, 30).WithPivot(0.5, 0).Done() // 以底部中心为支点
                    .For(C1_Left_Grip2).AsGripper(15, 5).WithSize(20, 30).WithPivot(0.5, 0).Done()
                    .For(C1_Left_Grip3).AsGripper(15, 5).WithSize(20, 30).WithPivot(0.5, 0).Done()
                    .For(C1_Left_Grip4).AsGripper(15, 5).WithSize(20, 30).WithPivot(0.5, 0).Done()
                    
                    // 右侧塔 (镜像定义)
                    .For(Z2_Lift).AsLinearGuide(180, 16).Vertical().Done()
                    .For(R2_Rotate).AsRotaryTable(60).WithPivot(0.5, 0.5).Done()
                    .For(C2_Right_Grip1).AsGripper(15, 5).WithSize(20, 30).WithPivot(0.5, 0).Done()
                    .For(C2_Right_Grip2).AsGripper(15, 5).WithSize(20, 30).WithPivot(0.5, 0).Done()
                    .For(C2_Right_Grip3).AsGripper(15, 5).WithSize(20, 30).WithPivot(0.5, 0).Done()
                    .For(C2_Right_Grip4).AsGripper(15, 5).WithSize(20, 30).WithPivot(0.5, 0).Done()
                    
                    // 滑台
                    .For(SlideCyl).AsSlideBlock().Horizontal().Reversed().Done()

                    // --- 绑定到 UI 控件 (Binding) ---
                    
                    // 1. 左塔绑定：直接将 Blueprint 中的 MountPoint "Root" 绑定到 Panel
                    //    假设我们 Blueprint 中左塔的根是 "Left_Tower_Assembly"，但实际上它挂在 Machine 上
                    //    我们这里演示直接绑定两个根级 Assembly
                    .Bind(pnlAxisZ1).TargetRoot("Left_Tower_Assembly").Done()
                    
                    // 2. 右塔绑定
                    .Bind(pnlAxisZ2).TargetRoot("Right_Tower_Assembly").Done()
                    
                    // 3. 滑台 (目前是孤立组件，没有复杂的 Mount 树)
                    .Bind(pnlCylinderSlide).ToCylinder(SlideCyl).Done();

            UI.Link(this)
              .ObserveInterpreter(interpreter)
              .ObserveContext(context)
              .Visuals(layout.Build());
        }
    }
}
