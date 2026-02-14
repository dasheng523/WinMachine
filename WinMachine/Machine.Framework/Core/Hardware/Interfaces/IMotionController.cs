using LanguageExt;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Core.Hardware.Models;
using System;
using LUnit = LanguageExt.Unit;

namespace Machine.Framework.Core.Hardware.Interfaces
{
    public interface IMotionController<TAxis, TIn, TOut> : IDisposable
    {
        Fin<LUnit> Initialization();
        Fin<LUnit> AxisEnable(TAxis axis, Level enable);
        Fin<LUnit> Stop(TAxis axis);
        Fin<LUnit> EStop(TAxis axis);
        Fin<bool> CheckDone(TAxis axis);
        Fin<bool> CheckHomeDone(TAxis axis);
        Fin<LUnit> GoBackHome(TAxis axis);
        
        Fin<LUnit> Move_Absolute(TAxis axis, double pos);
        Fin<LUnit> Move_Relative(TAxis axis, double dist);
        Fin<LUnit> Move_JOG(TAxis axis, MotionDirection dir);

        Fin<double> GetCommandPos(TAxis axis);
        Fin<LUnit> SetCommandPos(TAxis axis, double pos);
        
        Fin<double> GetEncoderPos(TAxis axis);
        Fin<LUnit> SetEncoderPos(TAxis axis, double pos);

        Fin<LUnit> SetSpeed(TAxis axis, AxisSpeed speed);

        Fin<Level> GetInput(TIn bitNo);
        Fin<Level> GetOutput(TOut bitNo);
        Fin<LUnit> SetOutput(TOut bitNo, Level level);

        Fin<AxisStatus> GetAxisStatus(TAxis axis);
        Fin<Level> GetAxisAlarm(TAxis axis);
    }
}
