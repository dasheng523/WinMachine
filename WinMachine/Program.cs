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


        // 根据配置动态注册运动控制器
        var systemOptions = config.GetSection("System").Get<SystemOptions>();

        if (systemOptions?.UseSimulator == true)
        {
            // 注册模拟器 (这里泛型参数暂定为 int，实际建议使用具体的枚举)
            services.AddSingleton(typeof(IMotionController<int, int, int>), typeof(SimulatorMotionController<int, int, int>));
        }
        else
        {
            // 根据具体类型注册
            switch (systemOptions?.ControllerType)
            {
                case "ZMotion":
                    services.AddSingleton(typeof(IMotionController<int, int, int>), typeof(ZauxMotionController<int, int, int>));
                    break;
                case "Leadshine":
                    services.AddSingleton(typeof(IMotionController<int, int, int>), typeof(LeadshineMotionController<int, int, int>));
                    break;
                default:
                    services.AddSingleton(typeof(IMotionController<int, int, int>), typeof(SimulatorMotionController<int, int, int>));
                    break;
            }
        }
    }
}