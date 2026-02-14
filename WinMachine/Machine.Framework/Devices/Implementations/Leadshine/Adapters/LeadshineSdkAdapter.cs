using Leadshine;

namespace Machine.Framework.Devices.Implementations.Leadshine.Adapters
{
    public class LeadshineSdkAdapter : ILeadshineSdk
    {
        public short smc_board_init(ushort connectNo, ushort type, string connectString, uint baudRate) =>
            LTSMC.smc_board_init(connectNo, type, connectString, baudRate);

        public short smc_board_close(ushort connectNo) =>
            LTSMC.smc_board_close(connectNo);

        public short smc_set_profile_unit(ushort connectNo, ushort axis, double minVel, double maxVel, double acc, double dec, double stopVel) =>
            LTSMC.smc_set_profile_unit(connectNo, axis, minVel, maxVel, acc, dec, stopVel);

        public short smc_pmove_unit(ushort connectNo, ushort axis, double pos, ushort mode) =>
            LTSMC.smc_pmove_unit(connectNo, axis, pos, mode);

        public short smc_vmove(ushort connectNo, ushort axis, ushort dir) =>
            LTSMC.smc_vmove(connectNo, axis, dir);

        public short smc_emg_stop(ushort ConnectNo) => LTSMC.smc_emg_stop(ConnectNo);
        public short smc_stop(ushort ConnectNo, ushort axis, ushort stop_mode) => LTSMC.smc_stop(ConnectNo, axis, stop_mode);
        public short smc_check_done(ushort ConnectNo, ushort axis) => LTSMC.smc_check_done(ConnectNo, axis);

        public short smc_get_position_unit(ushort connectNo, ushort axis, ref double pos) =>
            LTSMC.smc_get_position_unit(connectNo, axis, ref pos);

        public short smc_get_encoder_unit(ushort connectNo, ushort axis, ref double pos) =>
            LTSMC.smc_get_encoder_unit(connectNo, axis, ref pos);

        public short smc_read_inbit(ushort connectNo, ushort bitNo) =>
            LTSMC.smc_read_inbit(connectNo, bitNo);

        public short smc_read_outbit(ushort connectNo, ushort bitNo) =>
            LTSMC.smc_read_outbit(connectNo, bitNo);

        public short smc_write_outbit(ushort connectNo, ushort bitNo, ushort onOff) =>
            LTSMC.smc_write_outbit(connectNo, bitNo, onOff);

        public short smc_home_move(ushort connectNo, ushort axis) =>
            LTSMC.smc_home_move(connectNo, axis);

        public short smc_set_home_profile_unit(ushort connectNo, ushort axis, double lowVel, double highVel, double acc, double dec) =>
            LTSMC.smc_set_home_profile_unit(connectNo, axis, lowVel, highVel, acc, dec);

        public short smc_set_homemode(ushort connectNo, ushort axis, ushort homeDir, double vel, ushort mode, ushort source) =>
            LTSMC.smc_set_homemode(connectNo, axis, homeDir, vel, mode, source);
    }
}
