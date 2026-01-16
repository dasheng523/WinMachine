using System;
using System.Collections.Generic;
using Machine.Framework.Configuration;
using Machine.Framework.Configuration.Models;
using Xunit;
using Assert = Xunit.Assert;

namespace Machine.Framework.Tests
{
    public class AxisConfigDslTests
    {
        [Fact]
        public void Prototype_Axis_Detailed_Configuration()
        {
            // 这是展示给用户的 "轴参数详情配置" DSL 原型
            // 目标：统一不同板卡的配置接口，同时保留厂商特有能力

            var config = MachineConfig.Create()
                .AddControlBoard("MainMotion", board => board
                    .UseLeadshine(c => c
                        .Model(LeadshineModel.DMC3000)
                        .CardId(0)
                        
                        // --- 轴配置 DSL ---
                        .ConfigAxis(TestSystemAxis.X, axis => axis
                            // 1. 基础信号配置
                            .SetPulseOutput(PulseOutputMode.PulseDir_High_PosHigh)
                            .SetEncInput(EncoderInputMode.AbPhase_4x)

                            // 2. 运动参数 (物理量)
                            .SetEquivalency(pulsePerUnit: 1000) // 1mm = 1000脉冲
                            .SetBacklash(0.05)                  // 反向间隙 0.05mm
                            
                            // 3. 限位与安全 (Hard Limits)
                            .SetHardLimits(limits => limits
                                .Enable(true)
                                .Logic(ActiveLevel.Low)
                                .StopMode(StopAction.Immediate) // 急停/立即停止
                            )

                            // 4. 软限位 (Soft Limits)
                            .SetSoftLimits(limits => limits
                                .Enable(true)
                                .Range(-10, 500)
                                .Action(StopAction.Decelerate) // 减速停止
                            )

                            // 5. 回原点配置 (Homing)
                            .SetHoming(home => home
                                .Mode(HomeMode.Once) // 原点在正方向，仅搜原点
                                .HighSpeed(2000)    // 寻原点高速
                                .LowSpeed(100)      // 爬行速度
                                .OrgLogic(ActiveLevel.Low) // 原点信号电平
                            )

                            // 6. 专用IO映射 (Axis Specific IO Mapping)
                            // 将该轴的"报警信号"映射到物理端口 IN_15
                            .MapAxisIo(AxisIoType.Alarm, IoMapType.GeneralInput, index: 15, filterTime: 0.1)
                        )
                    )
                );

            Assert.NotNull(config);
            Console.WriteLine("Axis Detailed Config DSL execution successful.");
        }

        [Fact]
        public void User_Scenario_RealWorld_Axis_Init()
        {
            // 用户提供的真实初始化场景
            // 目标：验证 DSL 能否复现 LTSMC.smc_set_xxx 系列调用

            var config = MachineConfig.Create()
                .AddControlBoard("MainMotion", board => board
                    .UseLeadshine(c => c
                        .Model(LeadshineModel.SMC600)
                        .CardId(0)

                        // L1 - L4 批量配置 (共性部分可以提取 Loop，但 DSL 中 explicit 写法如下)
                        .ConfigAxis(Axis_B.L1, Configure_L_Axis)
                        .ConfigAxis(Axis_B.L2, Configure_L_Axis)
                        .ConfigAxis(Axis_B.L3, Configure_L_Axis)
                        .ConfigAxis(Axis_B.L4, Configure_L_Axis)

                        // R1 - R2 批量配置
                        .ConfigAxis(Axis_B.R1, Configure_R_Axis)
                        .ConfigAxis(Axis_B.R2, Configure_R_Axis)

                        // RS1 - RS2 批量配置
                        .ConfigAxis(Axis_B.RS1, Configure_RS1_Axis)
                        .ConfigAxis(Axis_B.RS2, Configure_RS2_Axis)
                    )
                );

            Assert.NotNull(config);
            var leadshine = config.BoardConfigs[0] as LeadshineConfig;
            Assert.NotNull(leadshine);
            
            // 验证 L1 的配置
            var l1Config = leadshine.AxisConfigs[Axis_B.L1.ToString()];
            Assert.Equal(PulseOutputMode.PulseDir_High_PosLow, l1Config.PulseOutput); // 1
            Assert.Equal(EncoderInputMode.AbPhase_4x, l1Config.EncoderInput); // 3
            Assert.NotNull(l1Config.Homing);
            Assert.Equal(HomeMode.Twice, l1Config.Homing.ModeVal); // 2
            Assert.NotNull(l1Config.HardLimits);
            Assert.Equal(ActiveLevel.High, l1Config.HardLimits.LogicLevel); // 1
            Assert.Equal(StopAction.Immediate, l1Config.HardLimits.StopActionVal); // 0
        }

        // 辅助配置方法：L轴
        private void Configure_L_Axis(AxisConfigBuilder axis)
        {
            axis
                // smc_set_el_mode(..., 1, 1, 0) -> Enable=true, Logic=High, Stop=Immediate
                .SetHardLimits(h => h.Enable(true).Logic(ActiveLevel.High).StopMode(StopAction.Immediate))
                
                // smc_set_alm_mode(..., 1, 1, 0)
                .SetAlarmConfig(a => a.Enable(true).Logic(ActiveLevel.High))

                // smc_set_pulse_outmode(..., 1) -> value 1 -> PulseDir_High_PosLow
                .SetPulseOutput(PulseOutputMode.PulseDir_High_PosLow)

                // smc_set_equiv(..., GlobalParams...)
                .SetEquivalency(1000) // 模拟 GlobalParams

                // smc_set_counter_inmode(..., 3) -> 4x
                .SetEncInput(EncoderInputMode.AbPhase_4x)

                // smc_set_counter_reverse(..., 1) -> Reverse? 
                // Using B_Lead_A to represent '1' if 0 is A_Lead_B (Default)
                .SetEncDirection(EncoderDir.B_Lead_A)

                // smc_set_homemode(..., 1, 1, 2, 0)
                // 1(Pos), 1(VelMode ignored for now or set via speed), 2(Twice), 0(Src)
                .SetHoming(h => h
                    .Direction(HomeDir.Positive)
                    .Mode(HomeMode.Twice)
                    .HighSpeed(10) // smc_set_home_profile_unit HighSpeed
                    .LowSpeed(1)   // smc_set_home_profile_unit LowSpeed
                    .Acceleration(0.1) // 0.1
                    .OrgLogic(ActiveLevel.High) // smc_set_home_pin_logic(..., 1, 0)
                )

                // smc_set_axis_io_map(..., 2, 0, axis, 0)
                // 2=ORG, 0=MapIoType(0=PosLimit?), axis=MapIoIndex, 0=Filter
                .MapAxisIo(AxisIoType.Org, IoMapType.LimitPositive, 0, 0); // IoMapType.LimitPositive corresponds to 0
        }

        private void Configure_R_Axis(AxisConfigBuilder axis)
        {
            axis
                // smc_set_el_mode(..., 0, 1, 0) -> Disable
                .SetHardLimits(h => h.Enable(false).Logic(ActiveLevel.High).StopMode(StopAction.Immediate))
                .SetAlarmConfig(a => a.Enable(true).Logic(ActiveLevel.High))
                .SetPulseOutput(PulseOutputMode.PulseDir_High_PosLow)
                .SetEquivalency(1000)
                .SetEncInput(EncoderInputMode.AbPhase_4x)
                // No reverse set for R? User code had L1-L4 reverse. R1, R2?
                // User code: smc_set_counter_reverse only L1-L4. So R is Default.
                .SetEncDirection(EncoderDir.A_Lead_B) 
                
                .SetHoming(h => h
                    .Direction(HomeDir.Positive)
                    .Mode(HomeMode.Twice)
                    .HighSpeed(10)
                    .OrgLogic(ActiveLevel.High)
                );
                // No IO Map for R in user snippet?
                // User snippet has IO Map for L1-L4 only.
        }

        private void Configure_RS1_Axis(AxisConfigBuilder axis)
        {
            axis
                .SetHardLimits(h => h.Enable(false).Logic(ActiveLevel.High))
                .SetAlarmConfig(a => a.Enable(false).Logic(ActiveLevel.High)) // User: Enable=0
                .SetPulseOutput(PulseOutputMode.PulseDir_High_PosLow) // 1
                .SetEquivalency(1000)
                .SetEncInput(EncoderInputMode.AbPhase_4x);
        }

        private void Configure_RS2_Axis(AxisConfigBuilder axis)
        {
            axis
                .SetHardLimits(h => h.Enable(false).Logic(ActiveLevel.High))
                .SetAlarmConfig(a => a.Enable(false).Logic(ActiveLevel.High))
                .SetPulseOutput(PulseOutputMode.PulseDir_Low_PosLow) // User: 3
                .SetEquivalency(1000)
                .SetEncInput(EncoderInputMode.AbPhase_4x);
        }

        [Fact]
        public void Verify_DSL_Serialization()
        {
            // 验证 DSL -> JSON -> DSL 的无损还原
            var config = MachineConfig.Create()
                .AddControlBoard("MotionA", b => b
                    .UseLeadshine(l => l
                        .Model(LeadshineModel.DMC3000)
                        .ConfigAxis(TestSystemAxis.X, a => a.SetPulseOutput(PulseOutputMode.PulseDir_High_PosHigh))
                    )
                )
                .AddDevice("Serial1", d => d
                    .UseSerialDevice(s => s.Port("COM1").BaudRate(9600))
                );

            string json = config.ToJson();
            Assert.False(string.IsNullOrWhiteSpace(json));
            // 验证多态鉴别器是否存在
            Assert.Contains("$type", json); 

            var loaded = ConfigurationExtensions.FromJson(json);
            Assert.NotNull(loaded);
            Assert.Single(loaded.BoardConfigs);
            Assert.Single(loaded.DeviceConfigs);

            var board = loaded.BoardConfigs[0] as LeadshineConfig;
            Assert.NotNull(board);
            Assert.Equal("MotionA", board.Name);
            Assert.Equal(LeadshineModel.DMC3000, board.ModelType);
            
            // 验证字典键是否正确序列化为字符串
            Assert.True(board.AxisConfigs.ContainsKey(TestSystemAxis.X.ToString()));
            var axis = board.AxisConfigs[TestSystemAxis.X.ToString()];
            Assert.Equal(PulseOutputMode.PulseDir_High_PosHigh, axis.PulseOutput);

            var device = loaded.DeviceConfigs[0] as SerialConfig;
            Assert.NotNull(device);
            Assert.Equal("Serial1", device.Name);
            Assert.Equal("COM1", device.PortName);
        }

        public enum TestSystemAxis { X, Y1, Z1, Z2, Y2 }

        public enum Axis_B
        {
            L1, L2, L3, L4,
            R1, R2,
            RS1, RS2
        }
    }

}
