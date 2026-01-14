using Machine.Framework.Devices.Motion.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Machine.Framework.Configuration;
using WinMachine.Services;
using Machine.Framework.Runtime;
using WinMachine.ConfigUi.WinForms;

namespace WinMachine
{
    public partial class Form1 : Form
    {
        private readonly IMotionSystem _motionSystem;
        private readonly SystemOptions _options;

        public Form1(IMotionSystem motionSystem, IOptions<SystemOptions> options)
        {
            InitializeComponent();
            _motionSystem = motionSystem;
            _options = options.Value;

            // 示例：在标题显示当前模式
            var primary = _options.MotionBoards.FirstOrDefault();
            var controller = primary?.ControllerType ?? (_options.UseSimulator ? "Simulator" : "(未配�?");
            this.Text = $"WinMachine - {(_options.UseSimulator ? "模拟模式" : "在线模式")} ({controller})";

            BtnZController.Click += BtnZController_Click;
            BtnSystemOptions.Click += BtnSystemOptions_Click;
            BtnSingleStep.Click += BtnSingleStep_Click;
        }

        private void BtnZController_Click(object? sender, EventArgs e)
        {
            var view = Program.ServiceProvider.GetRequiredService<ZControllerView>();
            view.ShowDialog();
        }

        private void BtnSystemOptions_Click(object? sender, EventArgs e)
        {
            var view = Program.ServiceProvider.GetRequiredService<SystemOptionsEditorForm>();
            view.ShowDialog(this);
        }

        private void BtnSingleStep_Click(object? sender, EventArgs e)
        {
            var view = Program.ServiceProvider.GetRequiredService<SingleStep>();
            view.ShowDialog(this);
        }
    }
}


