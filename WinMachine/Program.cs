using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Common.Hardware;
using WinMachine.Configuration;
using Devices.Motion.Abstractions;
using Devices.Motion.Implementations.Zaux;
using Devices.Motion.Implementations.Leadshine;
using Devices.Motion.Implementations.Simulator;
using Devices.Sensors.Modbus;
using Devices.Sensors.Serial;
using WinMachine.Services;
using WinMachine.ConfigUi.WinForms;


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
        services.AddTransient<SystemOptionsEditorForm>();

        // 注册机器管理服务 (单例，因为整台机器通常只有一个 lifecycle)
        services.AddSingleton<IMachineService, MachineManager>();

        // 逻辑轴映射（配置驱动）
        services.AddSingleton<IAxisResolver, AxisResolver>();

        // 逻辑 IO / 气缸 / 硬件 facade
        services.AddSingleton<IIoResolver, IoResolver>();
        services.AddSingleton<ICylinderResolver, CylinderResolver>();

        // 设备通讯资源池（Devices 层）
        services.AddSingleton<IModbusRtuMasterPool, NModbusRtuMasterPool>();
        services.AddSingleton<ISerialPortPool, SerialPortPool>();

        // 配置驱动的传感器解析（SensorMap）
        services.AddSingleton<IResolver<ISensor<Common.Core.Level>>, SensorMapLevelResolver>();
        services.AddSingleton<IResolver<ISensor<double>>, SensorMapDoubleResolver>();
        services.AddSingleton<IResolver<ISensor<string>>, SensorMapStringResolver>();

        services.AddSingleton<IHardware, HardwareFacade>();

        // MotionSystem：DI 只负责构造(纯)，初始化(效果)由 MachineManager 触发
        services.AddSingleton<IMotionSystem>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<SystemOptions>>().Value;

            var boards = (opt.MotionBoards is { Count: > 0 })
                ? opt.MotionBoards
                : new List<MotionBoardOptions>
                {
                    new()
                    {
                        Name = "Main",
                        ControllerType = opt.UseSimulator ? "Simulator" : "ZMotion",
                        DeviceIp = "127.0.0.1",
                        DeviceCardNo = 0
                    }
                };

            IMotionController<ushort, ushort, ushort> CreateController(MotionBoardOptions b)
            {
                if (opt.UseSimulator || string.Equals(b.ControllerType, "Simulator", StringComparison.OrdinalIgnoreCase))
                {
                    return new SimulatorMotionController<ushort, ushort, ushort>();
                }

                if (string.Equals(b.ControllerType, "ZMotion", StringComparison.OrdinalIgnoreCase))
                {
                    return new ZauxMotionController<ushort, ushort, ushort>
                    {
                        IP = b.DeviceIp,
                        CardNo = b.DeviceCardNo
                    };
                }

                if (string.Equals(b.ControllerType, "Leadshine", StringComparison.OrdinalIgnoreCase))
                {
                    Func<string, ushort>? axisNameResolver = null;
                    if (b.LeadshineInit is not null)
                    {
                        axisNameResolver = name =>
                        {
                            var map = opt.AxisMap;
                            if (map is null || map.Count == 0)
                            {
                                throw new InvalidOperationException("未配置 System.AxisMap，无法解析 AxisName");
                            }

                            // 兼容大小写
                            if (!map.TryGetValue(name, out var hit))
                            {
                                hit = map.FirstOrDefault(kv => string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase)).Value;
                            }

                            if (hit is null)
                            {
                                throw new KeyNotFoundException($"AxisMap 未找到: {name}");
                            }

                            // 若 AxisMap 显式指定了 Board，则要求与当前板卡一致，避免把配置打到别的板卡。
                            if (!string.IsNullOrWhiteSpace(hit.Board)
                                && !string.Equals(hit.Board, b.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                throw new InvalidOperationException($"AxisName={name} 映射到 Board={hit.Board}，但当前正在初始化 Board={b.Name}");
                            }

                            return hit.Axis;
                        };
                    }

                    var init = b.LeadshineInit is null
                        ? null
                        : LeadshineInit.BuildInitDelegate(b.LeadshineInit, axisNameResolver);

                    return new LeadshineMotionController<ushort, ushort, ushort>(b.DeviceIp, b.DeviceCardNo, init);
                }

                return new SimulatorMotionController<ushort, ushort, ushort>();
            }

            var motionBoards = boards
                .Select(b => new MotionBoard(b.Name, CreateController(b)))
                .ToList();

            return new MotionSystem(motionBoards);
        });
    }
}