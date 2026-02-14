using System;
using System.Linq;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Core.Blueprint.Builders;

namespace Machine.Framework.Interpreters.Configuration
{
    public static class BlueprintInterpreter
    {
        public static MachineConfig ToConfig(IMachineBlueprintBuilder blueprint)
        {
            if (blueprint is not MachineBuilder b)
                throw new ArgumentException("Invalid blueprint builder type.");

            var asm = b.Assembly;
            var config = MachineConfig.Internal_Create_By_Blueprint();

            // 1. 第一遍扫描：创建板卡及其内部设备的物理映射
            foreach (var boardDef in asm.Boards)
            {
                config.AddControlBoard(boardDef.Name, board =>
                {
                    // 应用 IO 映射
                    foreach (var m in boardDef.Builder.InputMappings) board.MapInput(m.Name, m.Port);
                    foreach (var m in boardDef.Builder.OutputMappings) board.MapOutput(m.Name, m.Port);

                    // 应用该板卡下的轴映射
                    foreach (var axis in boardDef.Builder.Axes)
                        board.MapAxis(axis.Name, axis.Channel);

                    // 应用该板卡下的气缸映射
                    foreach (var cyl in boardDef.Builder.Cylinders)
                    {
                        if (cyl.FbOut.HasValue && cyl.FbIn.HasValue)
                            board.MapCylinder(cyl.Name, cyl.DoOut, cyl.FbOut.Value, cyl.FbIn.Value);
                        else
                            board.MapCylinder(cyl.Name, cyl.DoOut);
                    }

                    // 设置基础驱动类型
                    if (boardDef.Builder.DriverTypeValue == DriverType.Simulator)
                        board.UseSimulator();
                });
            }

            // 2. 第二遍扫描：配置驱动细节与仿真参数
            foreach (var boardDef in asm.Boards)
            {
                // 硬件驱动配置
                if (boardDef.Builder.DriverTypeValue == DriverType.Leadshine && boardDef.Builder.LeadshineConfig != null)
                {
                    config.UseLeadshine(boardDef.Name, lc =>
                    {
                        var lb = boardDef.Builder.LeadshineConfig;
                        lc.Model(lb.ModelType).CardId(lb.CardIdValue);
                        foreach (var axisCfg in lb.AxisConfigs)
                            lc.ConfigAxis(axisCfg.Key, axisCfg.Value);
                    });
                }
                else if (boardDef.Builder.DriverTypeValue == DriverType.ZMotion && boardDef.Builder.ZMotionConfig != null)
                {
                    config.UseZMotion(boardDef.Name, zc =>
                    {
                        var zb = boardDef.Builder.ZMotionConfig;
                        zc.Model(zb.ModelType).IpAddress(zb.Ip);
                    });
                }

                // 仿真驱动联动配置
                if (boardDef.Builder.DriverTypeValue == DriverType.Simulator)
                {
                    config.UseSimulator(boardDef.Name, sim =>
                    {
                        foreach (var axis in boardDef.Builder.Axes)
                            sim.Axis(axis.Name, a => a.Travel(axis.Min, axis.Max));
                    });
                }

                // 同步轴参数（软限位等）
                foreach (var axis in boardDef.Builder.Axes)
                    config.ConfigureAxis(axis.Name, a => a.SetSoftLimits(sl => sl.Range(axis.Min, axis.Max)));

                // 同步气缸参数（运动时间等）
                foreach (var cyl in boardDef.Builder.Cylinders)
                    config.ConfigureCylinder(cyl.Name, c => c.MoveTime = cyl.ActionTimeMs);
            }

            // 3. 扫描全局外设与总线
            foreach (var dev in asm.Devices)
                config.AddDevice(dev.Name, dev.Config);

            foreach (var bus in asm.Buses)
                config.AddBus(bus.Name, bus.Config);
            
            // 4. 复制机械结构树 (Kinematics)
            config.MountPoints.AddRange(asm.MountPoints);

            return config;
        }
    }
}
