using System;

namespace Machine.Framework.Devices.Implementations.ZMotion.Adapters
{
    public interface IZMotionSdk
    {
        int ZAux_OpenEth(string ipaddr, out IntPtr phandle);
        int ZAux_Close(IntPtr handle);
        
        int ZAux_Direct_MoveAbs(IntPtr handle, int iaxis, float fValue);
        int ZAux_Direct_Move(IntPtr handle, int iaxis, float fValue);
        int ZAux_Direct_Single_Vmove(IntPtr handle, int iaxis, int iDir);
        int ZAux_Direct_Single_Cancel(IntPtr handle, int iaxis, int iMode); // Stop
        
        int ZAux_Direct_SetSpeed(IntPtr handle, int iaxis, float fValue);
        int ZAux_Direct_SetAccel(IntPtr handle, int iaxis, float fValue);
        int ZAux_Direct_SetDecel(IntPtr handle, int iaxis, float fValue);
        int ZAux_Direct_SetLspeed(IntPtr handle, int iaxis, float fValue);
        
        int ZAux_Direct_GetDpos(IntPtr handle, int iaxis, ref float pfValue);
        int ZAux_Direct_GetMpos(IntPtr handle, int iaxis, ref float pfValue);
        
        int ZAux_Direct_GetIn(IntPtr handle, int ionum, ref uint piValue);
        int ZAux_Direct_SetOp(IntPtr handle, int ionum, uint iValue);
        
        int ZAux_Direct_GetIfIdle(IntPtr handle, int iaxis, ref int piValue);
        int ZAux_Direct_GetAxisStatus(IntPtr handle, int iaxis, ref int piValue);

        // Home related
        int ZAux_Direct_SetHomeWait(IntPtr handle, int iaxis, int fValue);
        int ZAux_Direct_Single_Datum(IntPtr handle, int iaxis, int iMode);
    }
}
