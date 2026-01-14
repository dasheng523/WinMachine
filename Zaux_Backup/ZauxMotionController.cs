using System;
using Machine.Framework.Devices.Motion.Abstractions;
using Machine.Framework.Core.Core;
using LanguageExt;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace Machine.Framework.Devices.Motion.Implementations.Zaux
{
    public class ZauxMotionController<TAxis, TIn, TOut> : IMotionController<TAxis, TIn, TOut>
        where TAxis : notnull
        where TIn : notnull
        where TOut : notnull
    {
        public Fin<LUnit> Initialization() => FinSucc(unit);
        public Fin<LUnit> AxisEnable(TAxis axis, Level enable) => FinSucc(unit);
        public Fin<LUnit> Stop(TAxis axis) => FinSucc(unit);
        public Fin<LUnit> EStop(TAxis axis) => FinSucc(unit);
        public Fin<bool> CheckDone(TAxis axis) => FinSucc(true);
        public Fin<bool> CheckHomeDone(TAxis axis) => FinSucc(true);
        public Fin<LUnit> GoBackHome(TAxis axis) => FinSucc(unit);
        public Fin<LUnit> Move_Absolute(TAxis axis, double pos) => FinSucc(unit);
        public Fin<LUnit> Move_Relative(TAxis axis, double dist) => FinSucc(unit);
        public Fin<LUnit> Move_JOG(TAxis axis, MotionDirection dir) => FinSucc(unit);
        public Fin<double> GetCommandPos(TAxis axis) => FinSucc(0.0);
        public Fin<LUnit> SetCommandPos(TAxis axis, double pos) => FinSucc(unit);
        public Fin<double> GetEncoderPos(TAxis axis) => FinSucc(0.0);
        public Fin<LUnit> SetEncoderPos(TAxis axis, double pos) => FinSucc(unit);
        public Fin<LUnit> SetSpeed(TAxis axis, AxisSpeed speed) => FinSucc(unit);
        public Fin<Level> GetInput(TIn bitNo) => FinSucc(Level.Off);
        public Fin<Level> GetOutput(TOut bitNo) => FinSucc(Level.Off);
        public Fin<LUnit> SetOutput(TOut bitNo, Level level) => FinSucc(unit);
        public Fin<AxisStatus> GetAxisStatus(TAxis axis) => FinSucc(new AxisStatus());
        public Fin<Level> GetAxisAlarm(TAxis axis) => FinSucc(Level.Off);
        public void Dispose() {}
    }
}
