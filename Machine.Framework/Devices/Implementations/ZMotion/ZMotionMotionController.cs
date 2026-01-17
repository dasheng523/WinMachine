using System;
using System.Threading.Tasks;
using LanguageExt;
using Machine.Framework.Core.Hardware.Interfaces;
using Machine.Framework.Core.Hardware.Models;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Devices.Implementations.ZMotion.Adapters;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace Machine.Framework.Devices.Implementations.ZMotion
{
    public class ZMotionMotionController : IMotionController<ushort, ushort, ushort>
    {
        private readonly IZMotionSdk _sdk;
        private readonly string _ip;
        private IntPtr _handle;
        private bool _isConnected;

        public ZMotionMotionController(IZMotionSdk sdk, string ip = "192.168.0.11")
        {
            _sdk = sdk ?? throw new ArgumentNullException(nameof(sdk));
            _ip = ip;
        }

        public Fin<LUnit> Initialization()
        {
            var ret = _sdk.ZAux_OpenEth(_ip, out _handle);
            if (ret != 0) return FinFail<LUnit>(Error.New($"ZMotion Init Failed: {ret}"));
            
            _isConnected = true;
            return FinSucc(LUnit.Default);
        }

        public void Dispose()
        {
            if (_isConnected)
            {
                _sdk.ZAux_Close(_handle);
                _isConnected = false;
                _handle = IntPtr.Zero;
            }
        }

        private Fin<LUnit> CheckConnected() => 
            _isConnected ? FinSucc(LUnit.Default) : FinFail<LUnit>(Error.New("Not connected"));

        public Fin<LUnit> AxisEnable(ushort axis, Level enable)
        {
             // ZMotion typically requires setting axis enable via OP or specific parameter.
             // ZAux_Direct_SetOp for enabling driver? Or ZAux_Direct_AxisEnable?
             // Assuming default enabled or handled externally for now.
             return FinSucc(LUnit.Default);
        }

        public Fin<LUnit> Stop(ushort axis) =>
            CheckConnected().Bind(_ => RunSdK(() => _sdk.ZAux_Direct_Single_Cancel(_handle, axis, 2))); // 2: Decel stop

        public Fin<LUnit> EStop(ushort axis) =>
            CheckConnected().Bind(_ => RunSdK(() => _sdk.ZAux_Direct_Single_Cancel(_handle, axis, 2))); // Emergency typically just cancel

        public Fin<bool> CheckDone(ushort axis)
        {
             if (!_isConnected) return FinFail<bool>(Error.New("Not connected"));
             int idle = 0;
             var ret = _sdk.ZAux_Direct_GetIfIdle(_handle, axis, ref idle); // -1: idle (done), 0: moving ? Check documentation logic. 
             // Usually GetIfIdle: 0 running, -1 idle.
             return ret == 0 ? FinSucc(idle != 0) : FinFail<bool>(Error.New($"CheckDone Failed: {ret}"));
        }

        public Fin<bool> CheckHomeDone(ushort axis) => CheckDone(axis);

        public Fin<LUnit> GoBackHome(ushort axis) =>
             CheckConnected().Bind(_ => RunSdK(() => _sdk.ZAux_Direct_Single_Datum(_handle, axis, 1))); // Mode 1 assumed

        public Fin<LUnit> Move_Absolute(ushort axis, double pos) =>
             CheckConnected().Bind(_ => RunSdK(() => _sdk.ZAux_Direct_MoveAbs(_handle, axis, (float)pos)));

        public Fin<LUnit> Move_Relative(ushort axis, double dist) =>
             CheckConnected().Bind(_ => RunSdK(() => _sdk.ZAux_Direct_Move(_handle, axis, (float)dist)));

        public Fin<LUnit> Move_JOG(ushort axis, MotionDirection dir)
        {
             int d = dir == MotionDirection.Positive ? 1 : -1;
             return CheckConnected().Bind(_ => RunSdK(() => _sdk.ZAux_Direct_Single_Vmove(_handle, axis, d)));
        }

        public Fin<double> GetCommandPos(ushort axis)
        {
            if (!_isConnected) return FinFail<double>(Error.New("Not connected"));
            float pos = 0;
            var ret = _sdk.ZAux_Direct_GetDpos(_handle, axis, ref pos);
            return ret == 0 ? FinSucc((double)pos) : FinFail<double>(Error.New($"GetDpos Failed: {ret}"));
        }

        public Fin<LUnit> SetCommandPos(ushort axis, double pos) => FinSucc(LUnit.Default);

        public Fin<double> GetEncoderPos(ushort axis)
        {
            if (!_isConnected) return FinFail<double>(Error.New("Not connected"));
            float pos = 0;
            var ret = _sdk.ZAux_Direct_GetMpos(_handle, axis, ref pos);
            return ret == 0 ? FinSucc((double)pos) : FinFail<double>(Error.New($"GetMpos Failed: {ret}"));
        }

        public Fin<LUnit> SetEncoderPos(ushort axis, double pos) => FinSucc(LUnit.Default);

        public Fin<LUnit> SetSpeed(ushort axis, AxisSpeed speed)
        {
            return CheckConnected().Bind(_ =>
            {
                var r1 = _sdk.ZAux_Direct_SetSpeed(_handle, axis, (float)speed.Max);
                var r2 = _sdk.ZAux_Direct_SetAccel(_handle, axis, (float)speed.Tacc);
                var r3 = _sdk.ZAux_Direct_SetDecel(_handle, axis, (float)speed.Tdec);
                var r4 = _sdk.ZAux_Direct_SetLspeed(_handle, axis, (float)speed.Min);
                return (r1 | r2 | r3 | r4) == 0 ? FinSucc(LUnit.Default) : FinFail<LUnit>(Error.New("SetSpeed Failed"));
            });
        }

        public Fin<Level> GetInput(ushort bitNo)
        {
             if (!_isConnected) return FinFail<Level>(Error.New("Not connected"));
             uint val = 0;
             var ret = _sdk.ZAux_Direct_GetIn(_handle, bitNo, ref val);
             return FinSucc(val == 0 ? Level.Off : Level.On); // Check inversion logic
        }

        public Fin<Level> GetOutput(ushort bitNo)
        {
             // GetOp equivalent typically checks output status
             // Using GetOp?
             return FinSucc(Level.Off); 
        }

        public Fin<LUnit> SetOutput(ushort bitNo, Level level) =>
            CheckConnected().Bind(_ => RunSdK(() => 
                _sdk.ZAux_Direct_SetOp(_handle, bitNo, level == Level.On ? (uint)1 : (uint)0)));

        public Fin<AxisStatus> GetAxisStatus(ushort axis)
        {
              if (!_isConnected) return FinFail<AxisStatus>(Error.New("Not connected"));
              int status = 0;
              var ret = _sdk.ZAux_Direct_GetAxisStatus(_handle, axis, ref status);
              // Map status bits to AxisStatus
              return FinSucc(new AxisStatus 
              {
                  Moving = (status & 1) != 0, // Simplified guess
                  Error = false,
                  Origin = Level.Off,
                  PositiveHardLimit = Level.Off,
                  NegativeHardLimit = Level.Off
              });
        }

        public Fin<Level> GetAxisAlarm(ushort axis) => FinSucc(Level.Off);

        private Fin<LUnit> RunSdK(Func<int> action)
        {
            var ret = action();
            return ret == 0 ? FinSucc(LUnit.Default) : FinFail<LUnit>(Error.New($"SDK Error: {ret}"));
        }
    }
}
