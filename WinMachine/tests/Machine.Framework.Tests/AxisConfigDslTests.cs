using System;
using System.Collections.Generic;
using Machine.Framework.Core.Configuration;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Interpreters.Configuration;
using Xunit;

namespace Machine.Framework.Tests
{
    public class AxisConfigDslTests
    {
        [Fact]
        public void Prototype_Axis_Detailed_Configuration()
        {
            var blueprint = MachineBlueprint.Define("HardwareMachine")
                .AddBoard("MainMotion", 0, board => board
                    .UseLeadshine(c => c
                        .Model(LeadshineModel.DMC3000)
                        .CardId(0)
                        .ConfigAxis(TestSystemAxis.X, axis => axis
                            .SetPulseOutput(PulseOutputMode.PulseDir_High_PosHigh)
                            .SetEncInput(EncoderInputMode.AbPhase_4x)
                            .SetEquivalency(pulsePerUnit: 1000)
                            .SetHardLimits(limits => limits.Enable(true).Logic(ActiveLevel.Low))
                            .SetSoftLimits(limits => limits.Enable(true).Range(-10, 500))
                            .SetHoming(home => home.Mode(HomeMode.Once).HighSpeed(2000).LowSpeed(100))
                        )
                    )
                    // 使用 Action 重载以维持板卡链式调用
                    .AddAxis(new Machine.Framework.Core.Primitives.AxisID("X"), 0, _ => { })
                );

            var config = BlueprintInterpreter.ToConfig(blueprint);
            Assert.NotNull(config);
        }

        [Fact]
        public void User_Scenario_RealWorld_Axis_Init()
        {
            var blueprint = MachineBlueprint.Define("RealMachine")
                .AddBoard("MainMotion", 0, board => board
                    .UseLeadshine(c => c
                        .Model(LeadshineModel.SMC600)
                        .CardId(0)
                        .ConfigAxis(Axis_B.L1, Configure_L_Axis)
                        .ConfigAxis(Axis_B.R1, Configure_R_Axis)
                    )
                    .AddAxis(new Machine.Framework.Core.Primitives.AxisID("L1"), 0, _ => { })
                    .AddAxis(new Machine.Framework.Core.Primitives.AxisID("R1"), 1, _ => { })
                );

            var config = BlueprintInterpreter.ToConfig(blueprint);
            Assert.NotNull(config);
        }

        private void Configure_L_Axis(AxisConfigBuilder axis)
        {
            axis.SetHardLimits(h => h.Enable(true).Logic(ActiveLevel.High))
                .SetPulseOutput(PulseOutputMode.PulseDir_High_PosLow)
                .SetEquivalency(1000)
                .SetHoming(h => h.Mode(HomeMode.Twice).HighSpeed(10));
        }

        private void Configure_R_Axis(AxisConfigBuilder axis)
        {
            axis.SetAlarmConfig(a => a.Enable(true).Logic(ActiveLevel.High))
                .SetPulseOutput(PulseOutputMode.PulseDir_High_PosLow)
                .SetHoming(h => h.Mode(HomeMode.Twice).HighSpeed(10));
        }

        public enum TestSystemAxis { X, Y1, Z1, Z2, Y2 }
        public enum Axis_B { L1, L2, L3, L4, R1, R2, RS1, RS2 }
    }
}
