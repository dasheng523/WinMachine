using System;
using cszmcaux;

namespace Machine.Framework.Devices.Implementations.ZMotion.Adapters
{
    public class ZMotionSdkAdapter : IZMotionSdk
    {
        public int ZAux_OpenEth(string ipaddr, out IntPtr phandle) =>
            zmcaux.ZAux_OpenEth(ipaddr, out phandle);

        public int ZAux_Close(IntPtr handle) =>
            zmcaux.ZAux_Close(handle);

        public int ZAux_Direct_MoveAbs(IntPtr handle, int iaxis, float fValue) =>
            zmcaux.ZAux_Direct_Single_MoveAbs(handle, iaxis, fValue);

        public int ZAux_Direct_Move(IntPtr handle, int iaxis, float fValue) =>
            zmcaux.ZAux_Direct_Single_Move(handle, iaxis, fValue);

        public int ZAux_Direct_Single_Vmove(IntPtr handle, int iaxis, int iDir) =>
            zmcaux.ZAux_Direct_Single_Vmove(handle, iaxis, iDir);

        public int ZAux_Direct_Single_Cancel(IntPtr handle, int iaxis, int iMode) =>
            zmcaux.ZAux_Direct_Single_Cancel(handle, iaxis, iMode);

        public int ZAux_Direct_SetSpeed(IntPtr handle, int iaxis, float fValue) =>
            zmcaux.ZAux_Direct_SetSpeed(handle, iaxis, fValue);

        public int ZAux_Direct_SetAccel(IntPtr handle, int iaxis, float fValue) =>
            zmcaux.ZAux_Direct_SetAccel(handle, iaxis, fValue);

        public int ZAux_Direct_SetDecel(IntPtr handle, int iaxis, float fValue) =>
            zmcaux.ZAux_Direct_SetDecel(handle, iaxis, fValue);

        public int ZAux_Direct_SetLspeed(IntPtr handle, int iaxis, float fValue) =>
            zmcaux.ZAux_Direct_SetLspeed(handle, iaxis, fValue);

        public int ZAux_Direct_GetDpos(IntPtr handle, int iaxis, ref float pfValue) =>
            zmcaux.ZAux_Direct_GetDpos(handle, iaxis, ref pfValue);

        public int ZAux_Direct_GetMpos(IntPtr handle, int iaxis, ref float pfValue) =>
            zmcaux.ZAux_Direct_GetMpos(handle, iaxis, ref pfValue);

        public int ZAux_Direct_GetIn(IntPtr handle, int ionum, ref uint piValue) =>
            zmcaux.ZAux_Direct_GetIn(handle, ionum, ref piValue);

        public int ZAux_Direct_SetOp(IntPtr handle, int ionum, uint iValue) =>
            zmcaux.ZAux_Direct_SetOp(handle, ionum, iValue);

        public int ZAux_Direct_GetIfIdle(IntPtr handle, int iaxis, ref int piValue) =>
            zmcaux.ZAux_Direct_GetIfIdle(handle, iaxis, ref piValue);

        public int ZAux_Direct_GetAxisStatus(IntPtr handle, int iaxis, ref int piValue) =>
            zmcaux.ZAux_Direct_GetAxisStatus(handle, iaxis, ref piValue);
            
        public int ZAux_Direct_SetHomeWait(IntPtr handle, int iaxis, int fValue) =>
            zmcaux.ZAux_Direct_SetHomeWait(handle, iaxis, fValue);

        public int ZAux_Direct_Single_Datum(IntPtr handle, int iaxis, int iMode) =>
            zmcaux.ZAux_Direct_Single_Datum(handle, iaxis, iMode);
    }
}
