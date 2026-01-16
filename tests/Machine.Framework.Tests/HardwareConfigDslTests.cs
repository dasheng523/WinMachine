using System;
using System.Collections.Generic;
using Xunit;
using Machine.Framework.Core.Configuration;

// 命名空间调整为 Tests
namespace Machine.Framework.Tests
{
    public class HardwareConfigDslPrototypeTests
    {
        [Fact]
        public void Prototype_Machine_Hardware_Configuration_DSL()
        {
            // ====================================================================================
            // DSL 设计原型展示 (Implementation V1)
            // ====================================================================================

            var config = MachineConfig.Create()
                
                // --- 板卡 1: 运动控制卡 (雷赛 -> Leadshine) ---
                .AddControlBoard("MainMotion", board => board
                    .UseLeadshine(c => c
                        .Model(LeadshineModel.DMC3000)
                        .CardId(0)
                        .PulseMode(PulseOutputMode.PulseDir_High_PosHigh)

                        // --- 资源映射 (Mapping) ---
                        .MapAxis(SystemAxis.X, physicalIndex: 0)
                        .MapAxis(SystemAxis.Z1, physicalIndex: 1)
                        .MapAxis(SystemAxis.Z2, physicalIndex: 2)

                        .MapInput(SystemDI.StartBtn, port: 0)
                        .MapOutput(SystemDO.GreenLight, port: 0)
                    )
                )

                // --- 板卡 2: 辅助板卡 (正运动) ---
                .AddControlBoard("AuxMotion", board => board
                    .UseZMotion(c => c
                        .Model(ZMotionModel.ZMC432)
                        .IpAddress("192.168.0.11")
                        .SelectAxis(SystemAxis.Y1, physicalIndex: 0)
                    )
                )

                // --- 复杂传感器/外设配置 ---
                .AddDevice("HeightSensor", device => device
                     // 比如一个串口连接的千分表
                    .UseSerialDevice(c => c
                        .Port("COM1")
                        .BaudRate(9600)
                        .Protocol(SerialProtocol.ModbusRTU)
                        .MapFeature(SensorFeature.HeightValue, registerAddress: 40001)
                    )
                )

                .AddDevice("BarcodeScanner", device => device
                    // TCP 连接的扫码枪
                    .UseTcpDevice(c => c
                        .Ip("192.168.0.100")
                        .Port(8080)
                    )
                )

                // --- 共享总线/多站号设备配置 (RS485/Modbus) ---
                .AddBus("PressureSensorBus", bus => bus
                    .UseSerial(s => s
                        .Port("COM1")
                        .BaudRate(115200)
                        .Protocol(SerialProtocol.ModbusRTU)
                    )
                    // 在同一总线上挂载不同站号的设备节点
                    .MountDevice("Pressure_1", node => node
                        .StationId(0) // 站号 0
                        .MapFeature(SensorFeature.PressureValue, registerAddress: 0x1000)
                    )
                    .MountDevice("Pressure_2", node => node
                        .StationId(1) // 站号 1
                        .MapFeature(SensorFeature.PressureValue, registerAddress: 0x1000)
                    )
                    .MountDevice("Pressure_3", node => node
                        .StationId(2) // 站号 2
                        .MapFeature(SensorFeature.PressureValue, registerAddress: 0x1000)
                    )
                    .MountDevice("Pressure_4", node => node
                        .StationId(3) // 站号 3
                        .MapFeature(SensorFeature.PressureValue, registerAddress: 0x1000)
                    )
                );

            // 验证配置生成
            Assert.NotNull(config);
            Assert.Equal(2, config.BoardConfigs.Count);
            Assert.Equal(2, config.DeviceConfigs.Count);
            Assert.Single(config.BusConfigs);

            // 验证详细属性
            var leadshine = config.BoardConfigs[0] as LeadshineConfig;
            Assert.NotNull(leadshine);
            Assert.Equal(LeadshineModel.DMC3000, leadshine.ModelType);
            Assert.Equal(0, leadshine.BoardId);
            Assert.Equal(3, leadshine.AxisMappings.Count);

            var pressureBus = config.BusConfigs[0] as BusConfig;
            Assert.NotNull(pressureBus);
            Assert.Equal(4, pressureBus.Nodes.Count);
            Assert.Equal(0, pressureBus.Nodes[0].StationIdVal);
            
            Console.WriteLine("Machine Configuration DSL execution successful.");
        }
    }

    // 枚举定义 (模拟 WinMachine 层的定义)
    public enum SystemAxis { X, Y1, Z1, Z2, Y2 }
    public enum SystemDI { StartBtn }
    public enum SystemDO { GreenLight }
    public enum SensorFeature { HeightValue, PressureValue }
}
