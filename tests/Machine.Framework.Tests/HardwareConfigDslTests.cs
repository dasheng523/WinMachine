using System;
using System.Collections.Generic;
using Machine.Framework.Core.Configuration;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Interpreters.Configuration;
using Machine.Framework.Core.Primitives;
using Xunit;

namespace Machine.Framework.Tests
{
    public class HardwareConfigDslPrototypeTests
    {
        [Fact]
        public void Prototype_Machine_Hardware_Configuration_DSL()
        {
            var blueprint = MachineBlueprint.Define("HardwareMachine")
                .AddBoard("MainMotion", 0, board => board
                    .MapInput(SystemDI.StartBtn, port: 0)
                    .MapOutput(SystemDO.GreenLight, port: 0)
                    .AddAxis(new AxisID("X"), 0, _ => { })
                    .AddAxis(new AxisID("Z1"), 1, _ => { })
                    .AddAxis(new AxisID("Z2"), 2, _ => { })
                    .UseLeadshine(c => c.Model(LeadshineModel.DMC3000).CardId(0))
                )
                .AddBoard("AuxMotion", 1, board => board
                    .AddAxis(new AxisID("Y1"), 0, _ => { })
                    .UseZMotion(c => c.IpAddress("192.168.0.11"))
                )
                .AddDevice("HeightSensor", device => device
                    .UseSerialDevice(c => c.Port("COM1").BaudRate(9600).MapFeature(SensorFeature.HeightValue, 40001))
                )
                .AddDevice("BarcodeScanner", device => device
                    .UseTcpDevice(c => c.Ip("192.168.0.100").Port(8080))
                )
                .AddBus("PressureSensorBus", bus => bus
                    .UseSerial(s => s.Port("COM1").BaudRate(115200))
                    .MountDevice("Pressure_1", node => node.StationId(0).MapFeature(SensorFeature.PressureValue, 0x1000))
                );

            var config = BlueprintInterpreter.ToConfig(blueprint);

            Assert.NotNull(config);
            Assert.Equal(2, config.DeviceConfigs.Count);
            Assert.Equal(2, config.BoardConfigs.Count);
            Assert.Single(config.BusConfigs);
        }
    }

    public enum SystemAxis { X, Y1, Z1, Z2, Y2 }
    public enum SystemDI { StartBtn }
    public enum SystemDO { GreenLight }
    public enum SensorFeature { HeightValue, PressureValue }
}
