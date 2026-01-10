using Devices.Motion.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using WinMachine.Configuration;
using WinMachine.Services;

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
            this.Text = $"WinMachine - {(_options.UseSimulator ? "模拟模式" : "在线模式")} ({_options.ControllerType})";

            BtnZController.Click += BtnZController_Click;
        }

        private void BtnZController_Click(object? sender, EventArgs e)
        {
            var view = Program.ServiceProvider.GetRequiredService<ZControllerView>();
            view.ShowDialog();
        }
    }
}
