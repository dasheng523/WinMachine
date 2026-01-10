using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WinMachine.Configuration;
using Devices.Motion.Abstractions;
using Devices.Motion.Implementations.Zaux;
using Devices.Motion.Implementations.Leadshine;
using Devices.Motion.Implementations.Simulator;
using WinMachine.Services;


namespace WinMachine;

internal static class Program
{
    /// <summary>
    /// 全局服务提供者
    /// </summary>
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    static void Main()
    {
        // 1. 加载配置
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 2. 设置依赖注入
        var services = new ServiceCollection();
        ConfigureServices(services, config);

        ServiceProvider = services.BuildServiceProvider();

        // 3. 运行应用
        ApplicationConfiguration.Initialize();
        
        // 从容器中获取主窗体
        var mainForm = ServiceProvider.GetRequiredService<Form1>();
        Application.Run(mainForm);
    }

    private static void ConfigureServices(ServiceCollection services, IConfiguration config)
    {
        // 绑定配置
        services.Configure<SystemOptions>(config.GetSection("System"));

        // 注册窗体
        services.AddTransient<Form1>();
        services.AddTransient<ZControllerView>();

        // 注册机器管理服务 (单例，因为整台机器通常只有一个 lifecycle)
        services.AddSingleton<IMachineService, MachineManager>();

        // 运动控制器：DI 只负责构造(纯)，初始化(效果)由 MachineManager 触发
        services.AddSingleton<IMotionController<ushort, ushort, ushort>>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<SystemOptions>>().Value;

            if (opt.UseSimulator || string.Equals(opt.ControllerType, "Simulator", StringComparison.OrdinalIgnoreCase))
            {
                return new SimulatorMotionController<ushort, ushort, ushort>();
            }

            if (string.Equals(opt.ControllerType, "ZMotion", StringComparison.OrdinalIgnoreCase))
            {
                return new ZauxMotionController<ushort, ushort, ushort>
                {
                    IP = opt.DeviceIp,
                    CardNo = opt.DeviceCardNo
                };
            }

            if (string.Equals(opt.ControllerType, "Leadshine", StringComparison.OrdinalIgnoreCase))
            {
                return new LeadshineMotionController<ushort, ushort, ushort>(opt.DeviceIp, opt.DeviceCardNo);
            }

            return new SimulatorMotionController<ushort, ushort, ushort>();
        });
    }
}