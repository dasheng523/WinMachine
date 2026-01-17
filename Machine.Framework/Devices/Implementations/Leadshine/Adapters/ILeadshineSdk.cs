using System;

namespace Machine.Framework.Devices.Implementations.Leadshine.Adapters
{
    public interface ILeadshineSdk
    {
        short smc_board_init(ushort connectNo, ushort type, string connectString, uint baudRate);
        short smc_board_close(ushort connectNo);
        
        short smc_set_profile_unit(ushort connectNo, ushort axis, double minVel, double maxVel, double acc, double dec, double stopVel);
        short smc_pmove_unit(ushort connectNo, ushort axis, double pos, ushort mode);
        short smc_vmove(ushort connectNo, ushort axis, ushort dir);
        short smc_emg_stop(ushort ConnectNo);
        short smc_stop(ushort ConnectNo, ushort axis, ushort stop_mode);
        short smc_check_done(ushort ConnectNo, ushort axis);
        
        short smc_get_position_unit(ushort connectNo, ushort axis, ref double pos);
        short smc_get_encoder_unit(ushort connectNo, ushort axis, ref double pos);
        
        short smc_read_inbit(ushort connectNo, ushort bitNo);
        short smc_read_outbit(ushort connectNo, ushort bitNo);
        short smc_write_outbit(ushort connectNo, ushort bitNo, ushort onOff);
        
        short smc_home_move(ushort connectNo, ushort axis);
        short smc_set_home_profile_unit(ushort connectNo, ushort axis, double lowVel, double highVel, double acc, double dec);
        short smc_set_homemode(ushort connectNo, ushort axis, ushort homeDir, double vel, ushort mode, ushort source);
    }
}
