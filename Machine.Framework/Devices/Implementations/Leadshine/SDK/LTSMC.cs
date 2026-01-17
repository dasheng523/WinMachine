using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Leadshine
{
    public static class LTSMC
    {
        /// <summary>
        /// 设置网络链接超时时间。
        /// </summary>
        /// <param name="time_ms">超时时间，单位ms。如果超时时间等于0或者未调用该函数，则默认为5秒。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_connect_timeout(uint time_ms);

        /// <summary>
        /// 获取控制器连接状态。
        /// </summary>
        /// <param name="ConnectNo">链接号(0-254)，用于标识不同的控制器连接。</param>
        /// <returns>1表示连接正常，0表示连接丢失。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_connect_status(ushort ConnectNo);

        /// <summary>
        /// 设置网络通讯的发送和接收超时时间。
        /// </summary>
        /// <param name="SendTime_ms">发送数据时的超时时间，单位为毫秒。</param>
        /// <param name="RecvTime_ms">接收数据时的超时时间，单位为毫秒。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_send_recv_timeout(uint SendTime_ms, uint RecvTime_ms);

        /// <summary>
        /// 控制器链接初始化函数，分配系统资源。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="type">链接类型：1 - 串口，2 - 网口。</param>
        /// <param name="pconnectstring">链接字符串，对于网口连接，为控制器的IP地址；对于串口连接，为COM口名称（如 "COM1"）。</param>
        /// <param name="dwBaudRate">波特率，串口连接时有效，默认值为115200。</param>
        /// <returns>错误代码。0表示链接成功，非0表示链接失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_board_init(ushort ConnectNo, ushort type, string pconnectstring, uint dwBaudRate);

        /// <summary>
        /// 控制器高级链接初始化函数（串口专用），分配系统资源并设置详细串口参数。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="ConnectType">链接类型：1 - 串口，2 - 网口。</param>
        /// <param name="pconnectstring">链接字符串，对应控制器的IP地址或COM口。</param>
        /// <param name="dwBaudRate">波特率，默认值为115200。</param>
        /// <param name="dwByteSize">数据位，固定为8。</param>
        /// <param name="dwParity">校验位：0 - 无校验，1 - 奇校验，2 - 偶校验。</param>
        /// <param name="dwStopBits">停止位：1 - 1个停止位，2 - 2个停止位。</param>
        /// <returns>错误代码。0表示链接成功，非0表示链接失败。</returns>
        /// <remarks>使用API函数动态库时，数据位必须为8位。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_board_init_ex(ushort ConnectNo, ushort ConnectType, string pconnectstring, uint dwBaudRate, uint dwByteSize, uint dwParity, uint dwStopBits);

        /// <summary>
        /// 控制器关闭函数，释放系统资源。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_board_close(ushort ConnectNo);

        /// <summary>
        /// 控制器热复位，底层软件将重新启动，控制器不会断电。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>调用此函数后，建议等待约10秒再进行重新连接操作。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_soft_reset(ushort ConnectNo);

        /// <summary>
        /// 控制器冷复位，控制器将断电后重新上电。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>调用此函数后，建议等待约10秒再进行重新连接操作。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_board_reset(ushort ConnectNo);

        /// <summary>
        /// 函数调用打印输出设置，用于调试和监控API调用情况。
        /// </summary>
        /// <param name="mode">打印输出使能状态：0 - 禁止，1 - 使能。</param>
        /// <param name="FileName">文件保存路径。可以是相对路径或绝对路径。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_debug_mode(ushort mode, string FileName);

        /// <summary>
        /// 读取函数调用打印输出的设置。
        /// </summary>
        /// <param name="mode">返回打印输出的使能状态：0 - 禁止，1 - 使能。</param>
        /// <param name="FileName">返回用于保存打印信息的文件路径。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_debug_mode(ref ushort mode, byte[] FileName);

        /// <summary>
        /// 设置应用程序调试超时时间。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="time_s">超时时间，单位为秒。默认值为60秒。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_connect_debug_time(ushort ConnectNo, uint time_s);

        /// <summary>
        /// 获取控制器硬件版本号。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="CardVersion">返回控制器硬件版本号，该值为16进制数据。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_card_version(ushort ConnectNo, ref uint CardVersion);

        /// <summary>
        /// 获取控制器固件版本号。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="FirmID">返回控制器固件类型，为十进制数据，需要转换为十六进制进行比对。</param>
        /// <param name="SubFirmID">返回控制器固件版本号。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_card_soft_version(ushort ConnectNo, ref uint FirmID, ref uint SubFirmID);

        /// <summary>
        /// 获取控制器动态链接库(DLL)文件版本号。
        /// </summary>
        /// <param name="LibVer">返回库版本号。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_card_lib_version(ref uint LibVer);

        /// <summary>
        /// 读取固件发布版本号。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="ReleaseVersion">返回控制器发布版本号的字符串。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_release_version(ushort ConnectNo, byte[] ReleaseVersion);

        /// <summary>
        /// 获取当前控制器支持的总轴数。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="TotalAxis">返回当前控制器本地的轴数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_total_axes(ushort ConnectNo, ref uint TotalAxis);

        /// <summary>
        /// 获取当前控制器的数字I/O数量。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="TotalIn">返回当前控制器本地的数字输入数量。</param>
        /// <param name="TotalOut">返回当前控制器本地的数字输出数量。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_total_ionum(ushort ConnectNo, ref ushort TotalIn, ref ushort TotalOut);

        /// <summary>
        /// 获取当前控制器的模拟量输入输出数量。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="TotalIn">返回当前控制器本地的模拟量输入数量。</param>
        /// <param name="TotalOut">返回当前控制器本地的模拟量输出数量。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_total_adcnum(ushort ConnectNo, ref ushort TotalIn, ref ushort TotalOut);

        /// <summary>
        /// 格式化控制器内部FLASH存储空间。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_format_flash(ushort ConnectNo);

        /// <summary>
        /// 获取控制器内部RTC（实时时钟）的系统时间。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="year">返回年份。</param>
        /// <param name="month">返回月份。</param>
        /// <param name="day">返回日期。</param>
        /// <param name="hour">返回小时。</param>
        /// <param name="min">返回分钟。</param>
        /// <param name="sec">返回秒。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_rtc_get_time(ushort ConnectNo, ref int year, ref int month, ref int day, ref int hour, ref int min, ref int sec);

        /// <summary>
        /// 设置控制器内部RTC（实时时钟）的系统时间。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="year">年份。</param>
        /// <param name="month">月份。</param>
        /// <param name="day">日期。</param>
        /// <param name="hour">小时。</param>
        /// <param name="min">分钟。</param>
        /// <param name="sec">秒。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_rtc_set_time(ushort ConnectNo, int year, int month, int day, int hour, int min, int sec);

        /// <summary>
        /// 设置控制器新的IP地址。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="IpAddr">新IP地址的字符串表示，例如 "192.168.5.11"。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_ipaddr(ushort ConnectNo, byte[] IpAddr);

        /// <summary>
        /// 读取控制器当前的IP地址。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="IpAddr">用于接收IP地址字符串的字节数组。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ipaddr(ushort ConnectNo, byte[] IpAddr);

        /// <summary>
        /// 设置控制器COM口（RS232/RS485）的通讯参数。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="com">COM口类型：1 - RS232，2 - RS485。</param>
        /// <param name="dwBaudRate">波特率，例如 9600, 19200, 115200 等。</param>
        /// <param name="wByteSize">数据位，可选值为 7 或 8，默认值为8。</param>
        /// <param name="wParity">校验位：0 - 无校验，1 - 奇校验，2 - 偶校验。</param>
        /// <param name="wStopBits">停止位，可选值为 1 或 2。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_com(ushort ConnectNo, ushort com, uint dwBaudRate, ushort wByteSize, ushort wParity, ushort wStopBits);

        /// <summary>
        /// 读取控制器COM口（RS232/RS485）的通讯参数。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="com">COM口类型：1 - RS232，2 - RS485。</param>
        /// <param name="dwBaudRate">返回当前设置的波特率。</param>
        /// <param name="wByteSize">返回当前设置的数据位。</param>
        /// <param name="wParity">返回当前设置的校验位。</param>
        /// <param name="dwStopBits">返回当前设置的停止位。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_com(ushort ConnectNo, ushort com, ref uint dwBaudRate, ref ushort wByteSize, ref ushort wParity, ref ushort dwStopBits);

        //读写序列号，可将控制器标签上的序列号或者客户自定义的序列号写入控制器，断电保存
        /// <summary>
        /// 写入控制器序列号。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="sn">64位无符号整型序列号。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_write_sn(ushort ConnectNo, UInt64 sn);

        /// <summary>
        /// 读取控制器序列号。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="sn">返回读取到的64位无符号整型序列号。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_sn(ushort ConnectNo, ref UInt64 sn);

        /// <summary>
        /// 写入控制器序列号(字符串形式)。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="sn_str">要写入的序列号字符串。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_write_sn_numstring(ushort ConnectNo, string sn_str);

        /// <summary>
        /// 读取控制器序列号(字符串形式)。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="sn_str">用于接收序列号字符串的字节数组。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_sn_numstring(ushort ConnectNo, byte[] sn_str);

        //客户自定义密码字符串，最大256个字符，可通过此密码有效保护客户应用程序
        /// <summary>
        /// 加密控制器，设定一个访问密码，用于保护程序。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="str_pass">密码字符串，长度不超过256个字符。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_write_password(ushort ConnectNo, string str_pass);

        /// <summary>
        /// 密码校验。校验已写入到控制器的密码，成功后控制器才能正常运行。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="str_pass">需要校验的密码字符串，长度不超过256个字符。</param>
        /// <returns>校验状态：0 - 失败，1 - 成功。</returns>
        /// <remarks>1) 密码校验失败3次后，将无法再次校验。 2) 可在系统软件开启时加入此动作，对软件进行加密。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_check_password(ushort ConnectNo, string str_pass);

        //登入与修改密码，该密码用作限制控制器恢复出厂设置以及上传BASIC程序使用
        /// <summary>
        /// 登陆密码，用于修改控制器参数等受保护操作。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="str_pass">密码字符串，长度不超过16个字符。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_enter_password(ushort ConnectNo, string str_pass);

        /// <summary>
        /// 修改登陆密码。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="spassold">旧密码字符串，长度不超过16个字符。</param>
        /// <param name="spass">新密码字符串，长度不超过16个字符。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_modify_password(ushort ConnectNo, string spassold, string spass);

        //参数文件操作
        /// <summary>
        /// 下载PC端的参数文件（.cfg）到控制器的FLASH中。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="FileName">PC端本地参数文件的完整路径。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_download_parafile(ushort ConnectNo, byte[] FileName);

        /// <summary>
        /// 上传控制器中的参数文件（.cfg）到PC端。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)，默认值为0。</param>
        /// <param name="FileName">PC端用于保存参数文件的目标文件名，包含完整路径。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_upload_parafile(ushort ConnectNo, byte[] FileName);

        /*********************************************************************************************************
        安全机制参数
        *********************************************************************************************************/
        /// <summary>
        /// 设置硬限位（EL）信号。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="enable">使能状态：0 - 正负限位禁止；1 - 正负限位允许；2 - 正限位禁止，负限位允许；3 - 正限位允许，负限位禁止。</param>
        /// <param name="el_logic">有效电平：0 - 正负限位低电平有效；1 - 正负限位高电平有效；2 - 正限位低有效，负限位高有效；3 - 正限位高有效，负限位低有效。</param>
        /// <param name="el_mode">制动方式：0 - 正负限位立即停止；1 - 正负限位减速停止；2 - 正限位立即停止，负限位减速停止；3 - 正限位减速停止，负限位立即停止。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>当轴号设为255时，所有轴的限位信号参数都将被统一设置。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_el_mode(ushort ConnectNo, ushort axis, ushort enable, ushort el_logic, ushort el_mode);

        /// <summary>
        /// 读取硬限位（EL）信号的设置。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="enable">返回使能状态。</param>
        /// <param name="el_logic">返回有效电平设置。</param>
        /// <param name="el_mode">返回制动方式设置。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_el_mode(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort el_logic, ref ushort el_mode);

        /// <summary>
        /// 设置急停（EMG）信号。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="enable">信号功能使能：0 - 禁止，1 - 允许。</param>
        /// <param name="emg_logic">信号有效电平：0 - 低电平有效，1 - 高电平有效。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_emg_mode(ushort ConnectNo, ushort axis, ushort enable, ushort emg_logic);

        /// <summary>
        /// 读取急停（EMG）信号的设置。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="enable">返回信号功能使能状态。</param>
        /// <param name="emg_logic">返回信号有效电平设置。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_emg_mode(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort emg_logic);

        /// <summary>
        /// 设置软限位。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="enable">使能状态：0 - 禁止，1 - 允许。</param>
        /// <param name="source_sel">计数器源选择：0 - 指令位置计数器，1 - 编码器计数器。</param>
        /// <param name="SL_action">限位停止方式：0 - 立即停止，1 - 减速停止。</param>
        /// <param name="N_limit">负限位位置，单位为unit。</param>
        /// <param name="P_limit">正限位位置，单位为unit。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>正负限位位置可为正数也可为负数，但正限位位置应大于负限位位置。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_softlimit_unit(ushort ConnectNo, ushort axis, ushort enable, ushort source_sel, ushort SL_action, double N_limit, double P_limit);

        /// <summary>
        /// 读取软限位设置。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="enable">返回使能状态。</param>
        /// <param name="source_sel">返回计数器源选择。</param>
        /// <param name="SL_action">返回限位停止方式。</param>
        /// <param name="N_limit">返回负限位位置。</param>
        /// <param name="P_limit">返回正限位位置。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_softlimit_unit(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort source_sel, ref ushort SL_action, ref double N_limit, ref double P_limit);

        /*********************************************************************************************************
        单轴特殊功能参数
        *********************************************************************************************************/
        /// <summary>
        /// 设置指定轴的脉冲输出模式。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="outmode">脉冲输出方式：0 - PULSE/DIR, PULSE高电平有效, DIR高电平为正方向；1 - PULSE/DIR, PULSE高电平有效, DIR低电平为正方向；2 - PULSE/DIR, PULSE低电平有效, DIR高电平为正方向；3 - PULSE/DIR, PULSE低电平有效, DIR低电平为正方向；4 - CW/CCW, 高电平有效；5 - CW/CCW, 低电平有效；6 - AB相脉冲。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_pulse_outmode(ushort ConnectNo, ushort axis, ushort outmode);

        /// <summary>
        /// 读取指定轴的脉冲输出模式设置。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="outmode">返回脉冲输出方式。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_pulse_outmode(ushort ConnectNo, ushort axis, ref ushort outmode);

        //脉冲细分数，仅试用于脉冲控制器正弦曲线使用20211101
        /// <summary>
        /// 设置脉冲细分数（仅用于正弦曲线功能）。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="fractional_number">细分数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_pulse_fractional_number(ushort ConnectNo, ushort axis, ushort fractional_number);

        /// <summary>
        /// 获取脉冲细分数（仅用于正弦曲线功能）。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="fractional_number">返回设置的细分数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_pulse_fractional_number(ushort ConnectNo, ushort axis, ref ushort fractional_number);

        /// <summary>
        /// 设置脉冲当量，即一个用户单位(unit)对应的脉冲数(pulse)。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="equiv">脉冲当量，单位为 pulse/unit。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>此函数适用于高级运动函数（点位、插补等），运动前必须设置且不能为0。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_equiv(ushort ConnectNo, ushort axis, double equiv);

        /// <summary>
        /// 读取脉冲当量值。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="equiv">返回当前设置的脉冲当量值。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_equiv(ushort ConnectNo, ushort axis, ref double equiv);

        /// <summary>
        /// 设置反向间隙补偿值。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="backlash">反向间隙值，单位为unit。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_backlash_unit(ushort ConnectNo, ushort axis, double backlash);

        /// <summary>
        /// 读取反向间隙补偿值。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="backlash">返回当前设置的反向间隙值。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_backlash_unit(ushort ConnectNo, ushort axis, ref double backlash);

        //轴IO映射
        /// <summary>
        /// 设置轴专用IO信号到任意硬件输入口的映射。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="Axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="IoType">指定轴的IO信号类型：0-正限位(PEL), 1-负限位(NEL), 2-原点(ORG), 3-急停(EMG), 5-伺服报警(ALM), 7-伺服到位(INP)。</param>
        /// <param name="MapIoType">映射的目标IO类型：0-正限位输入口, 1-负限位输入口, 2-原点输入口, 3-伺服报警输入口, 5-伺服到位输入口, 6-通用输入口。</param>
        /// <param name="MapIoIndex">映射的目标IO索引号。若MapIoType为6，则为通用输入口号；否则为具体轴号。</param>
        /// <param name="Filter">信号滤波时间，单位为秒。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_axis_io_map(ushort ConnectNo, ushort Axis, ushort IoType, ushort MapIoType, ushort MapIoIndex, double Filter);

        /// <summary>
        /// 读取轴专用IO信号的映射关系。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="Axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="IoType">指定轴的IO信号类型。</param>
        /// <param name="MapIoType">返回映射的目标IO类型。</param>
        /// <param name="MapIoIndex">返回映射的目标IO索引号。</param>
        /// <param name="Filter">返回信号滤波时间。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_axis_io_map(ushort ConnectNo, ushort Axis, ushort IoType, ref ushort MapIoType, ref ushort MapIoIndex, ref double Filter);

        /*********************************************************************************************************
        单轴速度参数
        *********************************************************************************************************/
        /// <summary>
        /// 设置单轴运动的速度参数（时间模式）。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="Min_Vel">起始速度，单位：unit/s。</param>
        /// <param name="Max_Vel">最大运行速度，单位：unit/s。</param>
        /// <param name="Tacc">加速时间，单位：s。</param>
        /// <param name="Tdec">减速时间，单位：s。</param>
        /// <param name="Stop_Vel">停止速度，单位：unit/s。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_profile_unit(ushort ConnectNo, ushort axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);

        /// <summary>
        /// 读取单轴运动的速度参数（时间模式）。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="Min_Vel">返回起始速度。</param>
        /// <param name="Max_Vel">返回最大运行速度。</param>
        /// <param name="Tacc">返回加速时间。</param>
        /// <param name="Tdec">返回减速时间。</param>
        /// <param name="Stop_Vel">返回停止速度。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_profile_unit(ushort ConnectNo, ushort axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double Stop_Vel);

        /// <summary>
        /// 设置单轴运动的速度参数（加速度模式）。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="Min_Vel">起始速度，单位：unit/s。</param>
        /// <param name="Max_Vel">最大运行速度，单位：unit/s。</param>
        /// <param name="acc">加速度，单位：unit/s^2。</param>
        /// <param name="dec">减速度，单位：unit/s^2。</param>
        /// <param name="Stop_Vel">停止速度，单位：unit/s。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_profile_unit_acc(ushort ConnectNo, ushort axis, double Min_Vel, double Max_Vel, double acc, double dec, double Stop_Vel);

        /// <summary>
        /// 读取单轴运动的速度参数（加速度模式）。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="Min_Vel">返回起始速度。</param>
        /// <param name="Max_Vel">返回最大运行速度。</param>
        /// <param name="acc">返回加速度。</param>
        /// <param name="dec">返回减速度。</param>
        /// <param name="Stop_Vel">返回停止速度。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_profile_unit_acc(ushort ConnectNo, ushort axis, ref double Min_Vel, ref double Max_Vel, ref double acc, ref double dec, ref double Stop_Vel);

        /// <summary>
        /// 设置单轴S形速度曲线的平滑时间参数。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="s_mode">保留参数，固定为0。</param>
        /// <param name="s_para">S段时间，单位为秒，取值范围0~1s。若为0，则为T形曲线。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_s_profile(ushort ConnectNo, ushort axis, ushort s_mode, double s_para);

        /// <summary>
        /// 读取单轴S形速度曲线的平滑时间参数。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="s_mode">保留参数，固定为0。</param>
        /// <param name="s_para">返回S段时间，单位为秒。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_s_profile(ushort ConnectNo, ushort axis, ushort s_mode, ref double s_para);

        /// <summary>
        /// 设置定长运动在异常情况下的减速停止时间。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="time">减速停止时间，单位为秒。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_dec_stop_time(ushort ConnectNo, ushort axis, double time);

        /// <summary>
        /// 读取定长运动在异常情况下的减速停止时间。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="time">返回减速停止时间，单位为秒。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_dec_stop_time(ushort ConnectNo, ushort axis, ref double time);

        /*********************************************************************************************************
        单轴运动
        *********************************************************************************************************/
        /// <summary>
        /// 启动指定轴执行定长运动（点位运动）。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="Dist">目标位置，单位为unit。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式，1 - 绝对坐标模式。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pmove_unit(ushort ConnectNo, ushort axis, double Dist, ushort posi_mode);

        /// <summary>
        /// 启动指定轴以当前速度持续运动（恒速运动）。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="dir">运动方向：0 - 负方向，1 - 正方向。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_vmove(ushort ConnectNo, ushort axis, ushort dir);

        /// <summary>
        /// 在线改变指定轴的当前运动速度。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="Curr_Vel">新的运行速度，单位为unit/s。</param>
        /// <param name="Taccdec">变速时间，即从当前速度变到新速度所需的时间，单位为秒。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_change_speed_unit(ushort ConnectNo, ushort axis, double Curr_Vel, double Taccdec);

        /// <summary>
        /// 在线改变指定轴的当前目标位置。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="New_Pos">新的目标位置，单位为unit。此值为绝对坐标位置。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>此函数仅在轴处于PMOVE运动状态下有效。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_reset_target_position_unit(ushort ConnectNo, ushort axis, double New_Pos);

        /// <summary>
        /// 强制改变指定轴的当前目标位置。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="New_Pos">新的目标位置，单位为unit。此值为绝对坐标位置。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>此函数在轴处于PMOVE或空闲状态下均有效。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_update_target_position_unit(ushort ConnectNo, ushort axis, double New_Pos);

        //软着陆功能
        /// <summary>
        /// 单轴软着陆功能，整合了速度设置和运动指令。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="MidPos">第一段pmove的终点位置。</param>
        /// <param name="TargetPos">第二段pmove的终点位置。</param>
        /// <param name="Min_Vel">起始速度 (Vs)。</param>
        /// <param name="Max_Vel">最大速度 (Vm)。</param>
        /// <param name="stop_Vel">停止速度 (Ve)。</param>
        /// <param name="acc_time">加速时间 (0.001-2~31s)。</param>
        /// <param name="dec_time">减速时间 (0.001-2~31s)。</param>
        /// <param name="smooth_time">平滑时间(S段时间)。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标，1 - 绝对坐标。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pmove_unit_extern(ushort ConnectNo, ushort axis, double MidPos, double TargetPos, double Min_Vel, double Max_Vel, double stop_Vel, double acc_time, double dec_time, double smooth_time, ushort posi_mode);

        //正弦曲线定长运动
        /// <summary>
        /// 设置规划模式（此函数名可能已废弃或不常用，请谨慎使用）。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="mode">模式。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_plan_mode(ushort ConnectNo, ushort axis, ushort mode);

        /// <summary>
        /// 获取规划模式（此函数名可能已废弃或不常用，请谨慎使用）。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="mode">返回模式。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_plan_mode(ushort ConnectNo, ushort axis, ref ushort mode);

        /// <summary>
        /// 启动正弦曲线速度规划的定长运动。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="Dist">运动距离，单位unit。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标，1 - 绝对坐标。</param>
        /// <param name="MaxVel">最大速度。</param>
        /// <param name="MaxAcc">最大加速度。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pmove_sin_unit(ushort ConnectNo, ushort axis, double Dist, ushort posi_mode, double MaxVel, double MaxAcc);

        //高速IO触发在线变速变位置
        /// <summary>
        /// 配置高速IO触发在线变速变位功能。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="tar_vel">触发后设定的目标速度。</param>
        /// <param name="tar_rel_pos">触发后设定的目标相对位置。</param>
        /// <param name="trig_mode">触发模式。</param>
        /// <param name="source">触发源。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pmove_change_pos_speed_config(UInt16 ConnectNo, UInt16 axis, double tar_vel, double tar_rel_pos, UInt16 trig_mode, UInt16 source);

        /// <summary>
        /// 获取高速IO触发在线变速变位功能的配置。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="tar_vel">返回目标速度。</param>
        /// <param name="tar_rel_pos">返回目标相对位置。</param>
        /// <param name="trig_mode">返回触发模式。</param>
        /// <param name="source">返回触发源。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_pmove_change_pos_speed_config(UInt16 ConnectNo, UInt16 axis, ref double tar_vel, ref double tar_rel_pos, ref UInt16 trig_mode, ref UInt16 source);

        /// <summary>
        /// 使能或禁止高速IO触发在线变速变位功能。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="enable">使能状态：0 - 禁止, 1 - 使能。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pmove_change_pos_speed_enable(UInt16 ConnectNo, UInt16 axis, UInt16 enable);

        /// <summary>
        /// 获取高速IO触发在线变速变位功能的使能状态。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="enable">返回使能状态。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_pmove_change_pos_speed_enable(UInt16 ConnectNo, UInt16 axis, ref UInt16 enable);

        /// <summary>
        /// 获取高速IO触发在线变速变位的状态信息。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="trig_num">返回触发次数。</param>
        /// <param name="trig_pos">返回触发时的位置。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_pmove_change_pos_speed_state(UInt16 ConnectNo, UInt16 axis, ref UInt16 trig_num, ref double trig_pos);

        /*********************************************************************************************************
        回零运动
        *********************************************************************************************************/
        /// <summary>
        /// 设置原点（ORG）信号的逻辑电平和滤波。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="org_logic">ORG信号的有效电平：0 - 低电平有效，1 - 高电平有效。</param>
        /// <param name="filter">保留参数，固定值为0。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_home_pin_logic(ushort ConnectNo, ushort axis, ushort org_logic, double filter);

        /// <summary>
        /// 读取原点（ORG）信号的设置。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="org_logic">返回ORG信号的有效电平。</param>
        /// <param name="filter">返回保留参数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_home_pin_logic(ushort ConnectNo, ushort axis, ref ushort org_logic, ref double filter);

        /// <summary>
        /// 设置编码器Z相（EZ）信号的参数。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="ez_logic">EZ信号的有效电平：0 - 低电平有效，1 - 高电平有效。</param>
        /// <param name="ez_mode">保留参数，固定值为0。</param>
        /// <param name="filter">保留参数，固定值为0。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_ez_mode(ushort ConnectNo, ushort axis, ushort ez_logic, ushort ez_mode, double filter);

        /// <summary>
        /// 读取编码器Z相（EZ）信号的参数。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="ez_logic">返回EZ信号的有效电平。</param>
        /// <param name="ez_mode">返回保留参数。</param>
        /// <param name="filter">返回保留参数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ez_mode(ushort ConnectNo, ushort axis, ref ushort ez_logic, ref ushort ez_mode, ref double filter);

        /// <summary>
        /// 设置回原点运动的模式。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="home_dir">回原点方向：0 - 负向，1 - 正向。</param>
        /// <param name="vel_mode">回原点速度模式，默认值为1。</param>
        /// <param name="mode">回原点模式 (SMC300/600系列): 0-一次回零; 1-一次回零加反找; 2-二次回零; 3-一次回原点+EZ; 4-单独EZ回原点; 5-一次回零再反找EZ; 6-原点锁存; 7-原点锁存加同向EZ锁存; 8-单独EZ锁存; 9-原点锁存加反向EZ锁存。</param>
        /// <param name="pos_source">回零计数源：0 - 指令位置计数器，1 - 编码器计数器。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_homemode(ushort ConnectNo, ushort axis, ushort home_dir, double vel_mode, ushort mode, ushort pos_source);

        /// <summary>
        /// 读取回原点运动的模式设置。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="home_dir">返回回原点方向。</param>
        /// <param name="vel_mode">返回回原点速度模式。</param>
        /// <param name="home_mode">返回回原点模式。</param>
        /// <param name="pos_source">返回回零计数源。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_homemode(ushort ConnectNo, ushort axis, ref ushort home_dir, ref double vel_mode, ref ushort home_mode, ref ushort pos_source);

        /// <summary>
        /// 设置回零速度（该函数可能已被smc_set_home_profile_unit替代）。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="homespeed">回零速度。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_homespeed_unit(ushort ConnectNo, ushort axis, double homespeed);

        /// <summary>
        /// 获取回零速度（该函数可能已被smc_get_home_profile_unit替代）。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="homespeed">返回回零速度。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_homespeed_unit(ushort ConnectNo, ushort axis, ref double homespeed);

        /// <summary>
        /// 设置回原点的速度参数。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="Low_Vel">回原点时的起始速度。</param>
        /// <param name="High_Vel">回原点时的运行速度。</param>
        /// <param name="Tacc">回原点的加速时间，单位：s。</param>
        /// <param name="Tdec">回原点的减速时间（保留参数，固定为0）。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_home_profile_unit(ushort ConnectNo, ushort axis, double Low_Vel, double High_Vel, double Tacc, double Tdec);

        /// <summary>
        /// 读取回原点的速度参数。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="Low_Vel">返回回原点起始速度。</param>
        /// <param name="High_Vel">返回回原点运行速度。</param>
        /// <param name="Tacc">返回回原点加减速时间。</param>
        /// <param name="Tdec">返回保留值。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_home_profile_unit(ushort ConnectNo, ushort axis, ref double Low_Vel, ref double High_Vel, ref double Tacc, ref double Tdec);

        /// <summary>
        /// 设置是否将限位信号用作原点信号。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="mode">切换模式：0 - 不切换，1 - 正限位当原点，2 - 负限位当原点。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_el_home(ushort ConnectNo, ushort axis, ushort mode);

        //回零完成后设置位置
        /// <summary>
        /// 设置回零完成后的偏移位置。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="enable">使能参数：0 - 禁止；1 - 先清零，然后运动到指定位置（相对位置）；2 - 先运动到指定位置（相对位置），再清零。</param>
        /// <param name="position">设置的回原点偏移位置。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_home_position_unit(ushort ConnectNo, ushort axis, ushort enable, double position);

        /// <summary>
        /// 读取回零完成后的偏移位置设置。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="enable">返回使能参数。</param>
        /// <param name="position">返回设置的回原点偏移位置。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_home_position_unit(ushort ConnectNo, ushort axis, ref ushort enable, ref double position);

        /// <summary>
        /// 启动回原点运动。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_home_move(ushort ConnectNo, ushort axis);

        //回原点状态
        /// <summary>
        /// 读取回原点运动的状态。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="state">返回回原点运动状态：0 - 未完成，1 - 已完成。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_home_result(ushort ConnectNo, ushort axis, ref ushort state);

        /*********************************************************************************************************
        PVT运动
        *********************************************************************************************************/
        /// <summary>
        /// 向指定数据表传送数据，采用PVT（位置-速度-时间）模式。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="iaxis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="count">数据点个数。</param>
        /// <param name="pTime">数据点的时间数组，单位s。</param>
        /// <param name="pPos">数据点的位置数组，单位unit。</param>
        /// <param name="pVel">数据点的速度数组，单位unit/s。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>第一组（起始点）数据的位置、时间、速度必须为0。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pvt_table_unit(ushort ConnectNo, ushort iaxis, uint count, double[] pTime, double[] pPos, double[] pVel);

        /// <summary>
        /// 向指定数据表传送数据，采用PTS（位置-时间-速度百分比）模式。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="iaxis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="count">数据点个数。</param>
        /// <param name="pTime">数据点的时间数组，单位s。</param>
        /// <param name="pPos">数据点的位置数组，单位unit。</param>
        /// <param name="pPercent">数据点的速度百分比数组，取值范围[0,100]。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>第一组（起始点）数据的位置、时间必须为0。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pts_table_unit(ushort ConnectNo, ushort iaxis, uint count, double[] pTime, double[] pPos, double[] pPercent);

        /// <summary>
        /// 向指定数据表传送数据，采用PVTS（位置-时间，首末速度）模式。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="iaxis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="count">数据点个数。</param>
        /// <param name="pTime">数据点的时间数组，单位s。</param>
        /// <param name="pPos">数据点的位置数组，单位unit。</param>
        /// <param name="velBegin">轨迹段的起始速度，单位unit/s。</param>
        /// <param name="velEnd">轨迹段的结束速度，单位unit/s。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>第一组（起始点）数据的位置、时间、速度必须为0。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pvts_table_unit(ushort ConnectNo, ushort iaxis, uint count, double[] pTime, double[] pPos, double velBegin, double velEnd);

        /// <summary>
        /// 向指定数据表传送数据，采用PTT（位置-时间）模式。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="iaxis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="count">数据点个数。</param>
        /// <param name="pTime">数据点的时间数组，单位s。</param>
        /// <param name="pPos">数据点的位置数组，单位unit。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>第一组（起始点）数据的位置、时间必须为0。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_ptt_table_unit(ushort ConnectNo, ushort iaxis, uint count, double[] pTime, double[] pPos);

        /// <summary>
        /// 启动一个或多个轴的PVT运动。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="AxisNum">参与运动的轴数量。</param>
        /// <param name="AxisList">参与运动的轴号列表数组。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pvt_move(ushort ConnectNo, ushort AxisNum, ushort[] AxisList);

        /*********************************************************************************************************
        简易电子凸轮运动
        *********************************************************************************************************/
        /// <summary>
        /// 设置电子凸轮表。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="MasterAxisNo">主轴轴号。</param>
        /// <param name="SlaveAxisNo">从轴轴号。</param>
        /// <param name="Count">数据点个数。</param>
        /// <param name="pMasterPos">主轴位置数组。</param>
        /// <param name="pSlavePos">从轴位置数组。</param>
        /// <param name="SrcMode">主轴位置源模式：0 - 指令位置，1 - 编码器反馈位置。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_cam_table_unit(ushort ConnectNo, ushort MasterAxisNo, ushort SlaveAxisNo, uint Count, double[] pMasterPos, double[] pSlavePos, ushort SrcMode);

        /// <summary>
        /// 启动从轴的电子凸轮跟随运动。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="AxisNo">从轴轴号。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_cam_move(ushort ConnectNo, ushort AxisNo);

        /// <summary>
        /// 循环执行电子凸轮运动。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="iaxis">从轴轴号。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_cam_move_cycle(ushort ConnectNo, ushort iaxis);

        /*********************************************************************************************************
        正弦振荡运动
        *********************************************************************************************************/
        /// <summary>
        /// 设置正弦振荡模式。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="Axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="mode">振荡模式：0 - 位置时间为正弦曲线，1 - 位置时间为余弦曲线。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_sine_oscillate_set_mode(ushort ConnectNo, ushort Axis, ushort mode);

        /// <summary>
        /// 读取正弦振荡模式。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="Axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="mode">返回当前设置的振荡模式。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_sine_oscillate_get_mode(ushort ConnectNo, ushort Axis, ref ushort mode);

        /// <summary>
        /// 设置正弦振荡的幅值加速时间。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="Axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="tacc_s">加速时间，单位为秒。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_sine_oscillate_set_amplitude_tacc(ushort ConnectNo, ushort Axis, double tacc_s);

        /// <summary>
        /// 启动正弦曲线振荡运动。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="Axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="Amplitude">正余弦曲线的振幅。</param>
        /// <param name="Frequency">正余弦曲线的频率，即每秒振荡的次数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_sine_oscillate_unit(ushort ConnectNo, ushort Axis, double Amplitude, double Frequency);

        /// <summary>
        /// 停止正弦曲线振荡运动。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="Axis">指定轴号(0~最大轴数-1)。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_sine_oscillate_stop(ushort ConnectNo, ushort Axis);

        /// <summary>
        /// 设置正弦振荡的循环次数。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="Axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="cycle_num">循环次数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_sine_oscillate_set_cycle_num(ushort ConnectNo, ushort Axis, uint cycle_num);

        /// <summary>
        /// 获取正弦振荡的循环次数设置。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="Axis">指定轴号(0~最大轴数-1)。</param>
        /// <param name="cycle_num">返回设置的循环次数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_sine_oscillate_get_cycle_num(ushort ConnectNo, ushort Axis, ref uint cycle_num);

        /*********************************************************************************************************
        手轮运动
        *********************************************************************************************************/
        /// <summary>
        /// (高级模式)设置同一手轮轴选档位下具体运动的轴列表。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="AxisSelIndex">手轮上的轴选择档位。</param>
        /// <param name="AxisNum">该档位下参与运动的轴数量。</param>
        /// <param name="AxisList">该档位下参与运动的轴号数组。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_set_axislist(ushort ConnectNo, ushort AxisSelIndex, ushort AxisNum, ushort[] AxisList);

        /// <summary>
        /// (高级模式)读取同一手轮轴选档位下具体运动的轴列表。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="AxisSelIndex">手轮上的轴选择档位。</param>
        /// <param name="AxisNum">返回该档位下运动的轴数量。</param>
        /// <param name="AxisList">返回该档位下运动的轴号数组。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_get_axislist(ushort ConnectNo, ushort AxisSelIndex, ref ushort AxisNum, ushort[] AxisList);

        /// <summary>
        /// (高级模式)设置指定轴选档位下的手轮倍率列表。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="AxisSelIndex">手轮上的轴选择档位。</param>
        /// <param name="StartRatioIndex">倍率档位的起始索引。</param>
        /// <param name="RatioSelNum">要设置的倍率档位数量。</param>
        /// <param name="RatioList">包含各个倍率值的数组。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_set_ratiolist(ushort ConnectNo, ushort AxisSelIndex, ushort StartRatioIndex, ushort RatioSelNum, double[] RatioList);

        /// <summary>
        /// (高级模式)读取指定轴选档位下的手轮倍率列表。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="AxisSelIndex">手轮上的轴选择档位。</param>
        /// <param name="StartRatioIndex">倍率档位的起始索引。</param>
        /// <param name="RatioSelNum">要读取的倍率档位数量。</param>
        /// <param name="RatioList">返回包含各个倍率值的数组。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_get_ratiolist(ushort ConnectNo, ushort AxisSelIndex, ushort StartRatioIndex, ushort RatioSelNum, double[] RatioList);

        /// <summary>
        /// 设置手轮的运动模式。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="InMode">手轮输入脉冲模式：0 - 脉冲+方向，1 - AB相脉冲。</param>
        /// <param name="IfHardEnable">手轮运行模式：0 - 软件控制，1 - 硬件控制。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_set_mode(ushort ConnectNo, ushort InMode, ushort IfHardEnable);

        /// <summary>
        /// 读取手轮的运动模式设置。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="InMode">返回手轮输入脉冲模式。</param>
        /// <param name="IfHardEnable">返回手轮运行模式。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_get_mode(ushort ConnectNo, ref ushort InMode, ref ushort IfHardEnable);

        /// <summary>
        /// (默认模式)设置当前手轮运动的轴选和倍率档位。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="AxisSelIndex">手轮轴选档位。</param>
        /// <param name="RatioSelIndex">手轮倍率档位。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_set_index(ushort ConnectNo, ushort AxisSelIndex, ushort RatioSelIndex);

        /// <summary>
        /// (默认模式)读取当前手轮运动的轴选和倍率档位。
        /// </summary>
        /// <param name="ConnectNo">指定链接号(0-254)。</param>
        /// <param name="AxisSelIndex">返回手轮轴选档位。</param>
        /// <param name="RatioSelIndex">返回手轮倍率档位。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_get_index(ushort ConnectNo, ref ushort AxisSelIndex, ref ushort RatioSelIndex);

        /// <summary>
        /// 启动手轮运动。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">指定的轴号。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_move(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 停止手轮运动。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_stop(ushort ConnectNo);

        /// <summary>
        /// 设置手轮急停逻辑。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="emg_logic">急停逻辑。具体含义需参考设备手册，通常为0或1代表不同的有效电平。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_set_emg_logic(ushort ConnectNo, ushort emg_logic);

        /// <summary>
        /// 获取手轮急停逻辑。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="emg_logic">返回当前设置的急停逻辑。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_get_emg_logic(ushort ConnectNo, ref ushort emg_logic);

        /// <summary>
        /// 获取手轮的输入状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis_sel_input">返回轴选择开关的当前输入值。</param>
        /// <param name="ratio_sel_input">返回倍率选择开关的当前输入值。</param>
        /// <param name="emg_input">返回急停按钮的当前输入状态。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_handwheel_get_input(ushort ConnectNo, ref ushort axis_sel_input, ref ushort ratio_sel_input, ref ushort emg_input);

        /// <summary>
        /// 设置单轴手轮运动控制的输入方式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="axis">指定的轴号。</param>
        /// <param name="inmode">手轮输入方式：0 - 脉冲+方向信号；1 - A、B相正交信号。</param>
        /// <param name="multi">手轮倍率，正数表示默认方向，负数表示与默认方向反向。</param>
        /// <param name="vh">保留参数，固定值为0。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_handwheel_inmode(UInt16 ConnectNo, UInt16 axis, UInt16 inmode, Int32 multi, double vh);

        /// <summary>
        /// 读取单轴手轮运动控制的输入方式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="axis">指定的轴号。</param>
        /// <param name="inmode">返回手轮输入方式：0 - 脉冲+方向信号；1 - A、B相正交信号。</param>
        /// <param name="multi">返回手轮倍率。</param>
        /// <param name="vh">返回保留参数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_handwheel_inmode(UInt16 ConnectNo, UInt16 axis, ref UInt16 inmode, ref Int32 multi, ref double vh);

        /// <summary>
        /// 设置多轴手轮运动控制的输入方式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="inmode">手轮输入方式：0 - 脉冲+方向信号；1 - A、B相正交信号。</param>
        /// <param name="AxisNum">参与手轮运动的轴数量。</param>
        /// <param name="AxisList">参与手轮运动的轴号数组。</param>
        /// <param name="multi">手轮倍率数组。正数表示默认方向，负数表示与默认方向反向。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>通过该函数可以使一个手轮通道控制多个轴同时运动，运动倍率都以第一个轴的倍率。 </remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_handwheel_inmode_extern(UInt16 ConnectNo, UInt16 inmode, UInt16 AxisNum, UInt16[] AxisList, Int32[] multi);

        /// <summary>
        /// 读取多轴手轮运动控制的输入方式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="inmode">返回手轮输入方式。</param>
        /// <param name="AxisNum">返回参与手轮运动的轴数量。</param>
        /// <param name="AxisList">返回参与手轮运动的轴号数组。</param>
        /// <param name="multi">返回手轮倍率数组。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_handwheel_inmode_extern(UInt16 ConnectNo, ref UInt16 inmode, ref UInt16 AxisNum, UInt16[] AxisList, Int32[] multi);

        /// <summary>
        /// 设置单轴手轮运动控制输入方式（支持浮点数倍率）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">指定的轴号。</param>
        /// <param name="inmode">手轮输入方式：0 - 脉冲+方向信号；1 - A、B相正交信号。</param>
        /// <param name="multi">手轮倍率，支持浮点数。</param>
        /// <param name="vh">保留参数，固定值为0。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_handwheel_inmode_decimals(UInt16 ConnectNo, UInt16 axis, UInt16 inmode, double multi, double vh);

        /// <summary>
        /// 读取单轴手轮运动控制输入方式（支持浮点数倍率）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">指定的轴号。</param>
        /// <param name="inmode">返回手轮输入方式。</param>
        /// <param name="multi">返回手轮倍率。</param>
        /// <param name="vh">返回保留参数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_handwheel_inmode_decimals(UInt16 ConnectNo, UInt16 axis, ref UInt16 inmode, ref double multi, ref double vh);

        /// <summary>
        /// 设置多轴手轮运动控制输入方式（支持浮点数倍率）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="inmode">手轮输入方式：0 - 脉冲+方向信号；1 - A、B相正交信号。</param>
        /// <param name="AxisNum">参与手轮运动的轴数量。</param>
        /// <param name="AxisList">参与手轮运动的轴号数组。</param>
        /// <param name="multi">手轮倍率数组，支持浮点数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_handwheel_inmode_extern_decimals(UInt16 ConnectNo, UInt16 inmode, UInt16 AxisNum, UInt16[] AxisList, double[] multi);

        /// <summary>
        /// 读取多轴手轮运动控制输入方式（支持浮点数倍率）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="inmode">返回手轮输入方式。</param>
        /// <param name="AxisNum">返回参与手轮运动的轴数量。</param>
        /// <param name="AxisList">返回参与手轮运动的轴号数组。</param>
        /// <param name="multi">返回手轮倍率数组。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_handwheel_inmode_extern_decimals(UInt16 ConnectNo, ref UInt16 inmode, ref UInt16 AxisNum, UInt16[] AxisList, double[] multi);

        /*********************************************************************************************************
        多轴插补速度参数设置
        *********************************************************************************************************/
        /// <summary>
        /// 设置插补运动的速度参数（时间模式）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="Min_Vel">起始速度，单位：unit/s。</param>
        /// <param name="Max_Vel">最大速度（矢量合成速度），单位：unit/s。</param>
        /// <param name="Tacc">加速时间，单位：s。</param>
        /// <param name="Tdec">减速时间，单位：s。</param>
        /// <param name="Stop_Vel">停止速度，单位：unit/s。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_vector_profile_unit(ushort ConnectNo, ushort Crd, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);

        /// <summary>
        /// 读取插补运动的速度参数。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="Min_Vel">返回起始速度值，单位：unit/s。</param>
        /// <param name="Max_Vel">返回最大速度值，单位：unit/s。</param>
        /// <param name="Tacc">返回加速时间值，单位：s。</param>
        /// <param name="Tdec">返回减速时间值，单位：s。</param>
        /// <param name="Stop_Vel">返回停止速度值，单位：unit/s。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_vector_profile_unit(ushort ConnectNo, ushort Crd, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double Stop_Vel);

        /// <summary>
        /// 设置插补运动的速度参数(加速度模式)。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="Min_Vel">起始速度，单位：unit/s。</param>
        /// <param name="Max_Vel">最大速度，单位：unit/s。</param>
        /// <param name="acc">加速度，单位：unit/s^2。</param>
        /// <param name="dec">减速度，单位：unit/s^2。</param>
        /// <param name="Stop_Vel">停止速度，单位：unit/s。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_vector_profile_unit_acc(ushort ConnectNo, ushort Crd, double Min_Vel, double Max_Vel, double acc, double dec, double Stop_Vel);

        /// <summary>
        /// 读取插补运动的速度参数(加速度模式)。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="Min_Vel">返回起始速度值，单位：unit/s。</param>
        /// <param name="Max_Vel">返回最大速度值，单位：unit/s。</param>
        /// <param name="acc">返回加速度值，单位：unit/s^2。</param>
        /// <param name="dec">返回减速度值，单位：unit/s^2。</param>
        /// <param name="Stop_Vel">返回停止速度值，单位：unit/s。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出，是根据set函数推断得出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_vector_profile_unit_acc(ushort ConnectNo, ushort Crd, ref double Min_Vel, ref double Max_Vel, ref double acc, ref double dec, ref double Stop_Vel);

        /// <summary>
        /// 设置插补运动速度曲线的S段时间（平滑时间）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="s_mode">保留参数，固定值为0。</param>
        /// <param name="s_para">S段时间（平滑时间），单位：s，范围：0-1。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_vector_s_profile(ushort ConnectNo, ushort Crd, ushort s_mode, double s_para);

        /// <summary>
        /// 读取设置的插补运动速度曲线的S段时间（平滑时间）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="s_mode">保留参数，固定值为0。</param>
        /// <param name="s_para">返回S段时间（平滑时间）的设置值。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_vector_s_profile(ushort ConnectNo, ushort Crd, ushort s_mode, ref double s_para);

        /// <summary>
        /// 设置插补运动异常减速停止的时间。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="time">减速停止时间，单位：s。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_vector_dec_stop_time(ushort ConnectNo, ushort Crd, double time);

        /// <summary>
        /// 读取插补运动异常减速停止的时间。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="time">返回减速停止时间，单位：s。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_vector_dec_stop_time(ushort ConnectNo, ushort Crd, ref double time);

        /*********************************************************************************************************
        多轴单段插补运动
        *********************************************************************************************************/
        /// <summary>
        /// 执行直线插补运动。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">参与运动的轴数量，范围：2到控制器最大轴数。</param>
        /// <param name="AxisList">参与运动的轴号列表数组。</param>
        /// <param name="Dist">各轴的目标位置列表，单位：unit。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_line_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Dist, ushort posi_mode);

        /// <summary>
        /// 执行基于“圆心+终点”模式的螺旋线插补运动（可作两轴圆弧插补）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">运动轴数，范围：2到控制器最大轴数。</param>
        /// <param name="AxisList">轴号列表数组。</param>
        /// <param name="Target_Pos">各轴的目标位置数组，单位：unit。</param>
        /// <param name="Cen_Pos">圆心位置数组，单位：unit。</param>
        /// <param name="Arc_Dir">圆弧方向：0 - 顺时针；1 - 逆时针。</param>
        /// <param name="Circle">圈数。自然数：螺旋线插补的圈数；负数：同心圆插补，绝对值加1为同心圆圈数（如-1表示2圈）。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_arc_move_center_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, ushort Arc_Dir, int Circle, ushort posi_mode);

        /// <summary>
        /// 执行基于“半径+终点”模式的螺旋线插补运动（可作两轴圆弧插补）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">运动轴数，范围：2到控制器最大轴数。</param>
        /// <param name="AxisList">轴号列表数组。</param>
        /// <param name="Target_Pos">各轴的目标位置数组，单位：unit。</param>
        /// <param name="Arc_Radius">圆弧半径值，单位：unit。</param>
        /// <param name="Arc_Dir">圆弧方向：0 - 顺时针（劣弧）；1 - 逆时针（优弧）。</param>
        /// <param name="Circle">螺旋线的圈数，取值范围大于等于0。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_arc_move_radius_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double Arc_Radius, ushort Arc_Dir, int Circle, ushort posi_mode);

        /// <summary>
        /// 执行基于“三点”模式的螺旋线插补运动（可作两轴及三轴空间圆弧插补）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">运动轴数，范围：2到控制器最大轴数。</param>
        /// <param name="AxisList">轴号列表数组。</param>
        /// <param name="Target_Pos">目标点位置数组，单位：unit。</param>
        /// <param name="Mid_Pos">中间点位置数组，单位：unit。</param>
        /// <param name="Circle">圈数。自然数：圆柱螺旋线插补的圈数；负数：空间圆弧插补，其绝对值减1为空间圆弧的圈数（如-1表示0圈）。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_arc_move_3points_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Mid_Pos, int Circle, ushort posi_mode);

        /// <summary>
        /// 执行角度模式的圆弧插补运动。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">参与运动的轴数量。</param>
        /// <param name="AxisList">参与运动的轴号列表数组。</param>
        /// <param name="Cen_Pos">圆心位置数组，单位：unit。</param>
        /// <param name="Angle">转过的角度，单位：度。</param>
        /// <param name="Target_Pos">各轴的目标位置数组（通常未使用）。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_arc_move_angle_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Cen_Pos, double Angle, double[] Target_Pos, ushort posi_mode);

        /// <summary>
        /// 执行基于“圆心+角度”模式的圆弧插补。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">参与运动的轴数量。</param>
        /// <param name="AxisList">参与运动的轴号列表数组。</param>
        /// <param name="Target_Pos">各轴的目标位置数组，单位：unit。</param>
        /// <param name="Cen_Pos">圆心位置数组，单位：unit。</param>
        /// <param name="Angle">转过的角度，单位：度。</param>
        /// <param name="Arc_Dir">圆弧方向：0 - 顺时针；1 - 逆时针。</param>
        /// <param name="Circle">圈数。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_arc_move_center_angle_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, double Angle, ushort Arc_Dir, long Circle, ushort posi_mode);

        /// <summary>
        /// 执行基于“圆心+终点”模式的整圆插补。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">参与运动的轴数量。</param>
        /// <param name="AxisList">参与运动的轴号列表数组。</param>
        /// <param name="Target_Pos">终点位置数组，通常与起点相同，单位：unit。</param>
        /// <param name="Cen_Pos">圆心位置数组，单位：unit。</param>
        /// <param name="Arc_Dir">圆弧方向：0 - 顺时针；1 - 逆时针。</param>
        /// <param name="Circle">圈数。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_circle_move_center_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, ushort Arc_Dir, long Circle, ushort posi_mode);

        /*********************************************************************************************************
        多轴连续插补运动
        *********************************************************************************************************/

        /// <summary>
        /// 设置插补模式及小线段前瞻参数。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="mode">插补模式：0 - 非前瞻模式0；1 - 前瞻模式1；2 - 非前瞻模式2。</param>
        /// <param name="LookaheadSegment">前瞻段数，即每次运行时内部计算的段数。</param>
        /// <param name="PathError">轨迹误差，单位：unit。</param>
        /// <param name="LookaheadAcc">拐弯加速度，单位：unit/s^2。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>前瞻段数、轨迹误差、拐弯加速度都只有使能模式为前瞻运动时才有效。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_set_lookahead_mode(ushort ConnectNo, ushort Crd, ushort mode, int LookaheadSegment, double PathError, double LookaheadAcc);

        /// <summary>
        /// 读取插补模式及小线段前瞻参数。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="mode">返回插补模式：0 - 非前瞻模式0；1 - 前瞻模式1；2 - 非前瞻模式2。</param>
        /// <param name="LookaheadSegment">返回前瞻段数。</param>
        /// <param name="PathError">返回轨迹误差。</param>
        /// <param name="LookaheadAcc">返回拐弯加速度。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_get_lookahead_mode(ushort ConnectNo, ushort Crd, ref ushort mode, ref int LookaheadSegment, ref double PathError, ref double LookaheadAcc);

        /// <summary>
        /// 设置圆弧插补限速功能。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="Enable">使能参数：0 - 不限速；1 - 圆弧限速。</param>
        /// <param name="MaxCenAcc">保留参数，固定值为0。</param>
        /// <param name="MaxArcError">保留参数，固定值为0。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>圆弧限速只用于连续插补模式1和模式2。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_arc_limit(ushort ConnectNo, ushort Crd, ushort Enable, double MaxCenAcc, double MaxArcError);

        /// <summary>
        /// 回读圆弧插补限速功能的设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="Enable">返回使能参数：0 - 不限速；1 - 圆弧限速。</param>
        /// <param name="MaxCenAcc">返回保留参数。</param>
        /// <param name="MaxArcError">返回保留参数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_arc_limit(ushort ConnectNo, ushort Crd, ref ushort Enable, ref double MaxCenAcc, ref double MaxArcError);

        /// <summary>
        /// 打开连续插补缓冲区。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">参与运动的轴数量，范围：2到控制器最大轴数。</param>
        /// <param name="AxisList">参与运动的轴号列表数组。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>缓冲区最多可缓存5000条指令。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_open_list(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList);

        /// <summary>
        /// 关闭连续插补缓冲区。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_close_list(ushort ConnectNo, ushort Crd);

        /// <summary>
        /// 停止连续插补运动。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="stop_mode">停止模式：0 - 减速停止；1 - 立即停止。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_stop_list(ushort ConnectNo, ushort Crd, ushort stop_mode);

        /// <summary>
        /// 暂停连续插补运动。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_pause_list(ushort ConnectNo, ushort Crd);

        /// <summary>
        /// 开始或继续执行连续插补缓冲区中的运动。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>当暂停后，调用此函数将继续运行未完成的轨迹。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_start_list(ushort ConnectNo, ushort Crd);

        /// <summary>
        /// 动态调整连续插补的速度比例（速度倍率）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="percent">速度比例，取值范围：0-2.0 (例如1.0代表100%速度)。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_change_speed_ratio(ushort ConnectNo, ushort Crd, double percent);

        /// <summary>
        /// 读取当前连续插补的运动状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <returns>运动状态：0 - 运动中；1 - 暂停中；2 - 正常停止；3 - 未启动；4 - 空闲；5 - 异常减速停止中。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_get_run_state(ushort ConnectNo, ushort Crd);

        /// <summary>
        /// 查询连续插补缓冲区中剩余的可用空间。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <returns>返回缓冲区剩余可用的指令条数。</returns>
        [DllImport("LTSMC.dll")]
        public static extern int smc_conti_remain_space(ushort ConnectNo, ushort Crd);

        /// <summary>
        /// 读取连续插补缓冲区中当前正在执行的插补段号（标号）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <returns>返回当前插补段的用户定义标号。</returns>
        [DllImport("LTSMC.dll")]
        public static extern int smc_conti_read_current_mark(ushort ConnectNo, ushort Crd);

        /// <summary>
        /// 在连续插补缓冲区中添加一段直线插补指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">参与运动的轴数量。</param>
        /// <param name="AxisList">参与运动的轴号列表数组。</param>
        /// <param name="pPosList">各轴的目标位置数组，单位：unit。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <param name="mark">用户定义的标号，任意指定，0表示自动编号。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_line_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] pPosList, ushort posi_mode, int mark);

        /// <summary>
        /// 在连续插补缓冲区中添加一段基于“圆心+终点”模式的螺旋线插补指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">运动轴数。</param>
        /// <param name="AxisList">轴号列表数组。</param>
        /// <param name="Target_Pos">目标位置数组，单位：unit。</param>
        /// <param name="Cen_Pos">圆心位置数组，单位：unit。</param>
        /// <param name="Arc_Dir">圆弧方向：0 - 顺时针；1 - 逆时针。</param>
        /// <param name="Circle">圈数。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <param name="mark">用户定义的标号，任意指定，0表示自动编号。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_arc_move_center_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, ushort Arc_Dir, int Circle, ushort posi_mode, int mark);

        /// <summary>
        /// 在连续插补缓冲区中添加一段基于“半径+终点”模式的圆柱螺旋线插补指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">运动轴数。</param>
        /// <param name="AxisList">轴号列表数组。</param>
        /// <param name="Target_Pos">目标位置数组，单位：unit。</param>
        /// <param name="Arc_Radius">圆弧半径值，单位：unit。</param>
        /// <param name="Arc_Dir">圆弧方向：0 - 顺时针；1 - 逆时针。</param>
        /// <param name="Circle">螺旋线的圈数。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <param name="mark">用户定义的标号，任意指定，0表示自动编号。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_arc_move_radius_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double Arc_Radius, ushort Arc_Dir, int Circle, ushort posi_mode, int mark);

        /// <summary>
        /// 在连续插补缓冲区中添加一段基于“三点”模式的圆柱螺旋线插补指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">运动轴数。</param>
        /// <param name="AxisList">轴号列表数组。</param>
        /// <param name="Target_Pos">目标点位置数组，单位：unit。</param>
        /// <param name="Mid_Pos">中间点位置数组，单位：unit。</param>
        /// <param name="Circle">圈数。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <param name="mark">用户定义的标号，任意指定，0表示自动编号。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_arc_move_3points_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Mid_Pos, int Circle, ushort posi_mode, int mark);

        /// <summary>
        /// 在连续插补缓冲区中添加一段角度模式的圆弧插补指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">参与运动的轴数量。</param>
        /// <param name="AxisList">参与运动的轴号列表数组。</param>
        /// <param name="Cen_Pos">圆心位置数组，单位：unit。</param>
        /// <param name="Angle">转过的角度，单位：度。</param>
        /// <param name="Target_Pos">目标位置数组。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <param name="mark">用户定义的标号，任意指定，0表示自动编号。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_arc_move_angle_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Cen_Pos, double Angle, double[] Target_Pos, ushort posi_mode, long mark);

        /// <summary>
        /// 在连续插补缓冲区中添加一段“圆心+角度”模式的圆弧插补指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">参与运动的轴数量。</param>
        /// <param name="AxisList">参与运动的轴号列表数组。</param>
        /// <param name="Target_Pos">目标位置数组，单位：unit。</param>
        /// <param name="Cen_Pos">圆心位置数组，单位：unit。</param>
        /// <param name="Angle">转过的角度，单位：度。</param>
        /// <param name="Arc_Dir">圆弧方向：0 - 顺时针；1 - 逆时针。</param>
        /// <param name="Circle">圈数。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <param name="mark">用户定义的标号，任意指定，0表示自动编号。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_arc_move_center_angle_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, double Angle, ushort Arc_Dir, long Circle, ushort posi_mode, long mark);

        /// <summary>
        /// 在连续插补缓冲区中添加一段整圆插补指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="AxisNum">参与运动的轴数量。</param>
        /// <param name="AxisList">参与运动的轴号列表数组。</param>
        /// <param name="Cen_Pos">圆心位置数组，单位：unit。</param>
        /// <param name="Angle">转过的角度，单位：度。</param>
        /// <param name="Target_Pos">目标位置数组。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <param name="mark">用户定义的标号，任意指定，0表示自动编号。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_circle_move_angle_unit(ushort ConnectNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Cen_Pos, double Angle, double[] Target_Pos, ushort posi_mode, long mark);

        /// <summary>
        /// 在连续插补缓冲区中添加一条等待IO输入的指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="bitno">输入口号，取值范围：0-31。</param>
        /// <param name="on_off">期望的电平状态：0 - 低电平；1 - 高电平。</param>
        /// <param name="TimeOut">超时时间，单位：s。若设为0，则无限等待。</param>
        /// <param name="mark">用户定义的标号，任意指定，0表示自动编号。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_wait_input(ushort ConnectNo, ushort Crd, ushort bitno, ushort on_off, double TimeOut, int mark);

        /// <summary>
        /// 在连续插补中添加一条相对于轨迹段【起点】的IO滞后输出指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="bitno">输出口号，取值范围：0-31。</param>
        /// <param name="on_off">输出的电平状态：0 - 低电平；1 - 高电平。</param>
        /// <param name="delay_value">滞后值，单位由delay_mode决定。</param>
        /// <param name="delay_mode">滞后模式：0 - 滞后时间（单位s）；1 - 滞后距离（单位unit）。</param>
        /// <param name="ReverseTime">电平输出后经过指定时间（单位s）自动翻转。若为0则不翻转。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此IO操作将在该指令的下一条轨迹中起作用。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_delay_outbit_to_start(ushort ConnectNo, ushort Crd, ushort bitno, ushort on_off, double delay_value, ushort delay_mode, double ReverseTime);

        /// <summary>
        /// 在连续插补中添加一条相对于轨迹段【终点】的IO滞后输出指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="bitno">输出口号，取值范围：0-31。</param>
        /// <param name="on_off">输出的电平状态：0 - 低电平；1 - 高电平。</param>
        /// <param name="delay_time">滞后时间，单位：s。</param>
        /// <param name="ReverseTime">保留参数，固定值为0。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此IO操作将在该指令的下一条轨迹结束后起作用。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_delay_outbit_to_stop(ushort ConnectNo, ushort Crd, ushort bitno, ushort on_off, double delay_time, double ReverseTime);

        /// <summary>
        /// 在连续插补中添加一条相对于轨迹段【终点】的IO提前输出指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="bitno">输出口号，取值范围：0-31。</param>
        /// <param name="on_off">输出的电平状态：0 - 低电平；1 - 高电平。</param>
        /// <param name="ahead_value">提前值，单位由ahead_mode决定。</param>
        /// <param name="ahead_mode">提前模式：0 - 提前时间（单位s）；1 - 提前距离（单位unit）。</param>
        /// <param name="ReverseTime">电平输出后经过指定时间（单位s）自动翻转。若为0则不翻转。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此IO操作将在该指令的下一条轨迹中起作用。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_ahead_outbit_to_stop(ushort ConnectNo, ushort Crd, ushort bitno, ushort on_off, double ahead_value, ushort ahead_mode, double ReverseTime);

        /// <summary>
        /// 在连续插补中添加一条精确位置CMP输出控制指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="cmp_no">比较器号。</param>
        /// <param name="on_off">开关状态：0 - 关闭；1 - 打开。</param>
        /// <param name="map_axis">映射轴号。</param>
        /// <param name="rel_dist">相对距离，单位：unit。</param>
        /// <param name="pos_source">位置源。</param>
        /// <param name="ReverseTime">翻转时间，单位：s。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_accurate_outbit_unit(ushort ConnectNo, ushort Crd, ushort cmp_no, ushort on_off, ushort map_axis, double rel_dist, ushort pos_source, double ReverseTime);

        /// <summary>
        /// 在连续插补缓冲区中添加一条立即IO输出指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="bitno">输出口号，取值范围：0-31。</param>
        /// <param name="on_off">输出的电平状态：0 - 低电平；1 - 高电平。</param>
        /// <param name="ReverseTime">电平输出后经过指定时间（单位s）自动翻转。若为0则不翻转。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>该指令会立即执行，可能会破坏轨迹的连续性（Blend平滑模式会失效）。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_write_outbit(ushort ConnectNo, ushort Crd, ushort bitno, ushort on_off, double ReverseTime);

        /// <summary>
        /// 清除当前段内未执行完的延时IO动作。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="Io_Mask">清除标志，bit0-bit31分别表示Out0-Out31。位值为1表示清除对应输出口的动作。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>对 smc_conti_delay_outbit_to_start, smc_conti_ahead_outbit_to_stop, smc_conti_delay_outbit_to_stop 指令起作用。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_clear_io_action(ushort ConnectNo, ushort Crd, uint Io_Mask);

        /// <summary>
        /// 设置在连续插补暂停及异常停止时的IO输出状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="action">
        /// 激活模式：
        /// 0 - 保持原状。
        /// 1 - 暂停时输出设定IO状态，恢复时不恢复暂停前状态。
        /// 2 - 暂停时输出设定IO状态，恢复时恢复暂停前状态。
        /// 3 - 暂停、停止或异常时输出设定IO状态。
        /// </param>
        /// <param name="mask">选择要操作的输出端口标志，bit0-bit31代表Out0-Out31，位值为1表示该端口受控。</param>
        /// <param name="state">设置端口的输出电平状态，bit0-bit31代表Out0-Out31，位值为1输出高电平，0输出低电平。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_set_pause_output(ushort ConnectNo, ushort Crd, ushort action, int mask, int state);

        /// <summary>
        /// 读取在连续插补暂停及异常停止时的IO输出状态的设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="action">返回激活模式的设置值。</param>
        /// <param name="mask">返回输出端口选择标志的设置值。</param>
        /// <param name="state">返回输出电平状态的设置值。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_get_pause_output(ushort ConnectNo, ushort Crd, ref ushort action, ref int mask, ref int state);

        /// <summary>
        /// 设置速度倍率。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="Percent">速度倍率，例如1.0代表100%。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_set_override(ushort ConnectNo, ushort Crd, double Percent);

        /// <summary>
        /// 设置连续插补中Blend拐角过渡模式的使能状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="enable">使能状态：0 - 禁止；1 - 允许。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出，但功能描述可在文档中找到。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_set_blend(ushort ConnectNo, ushort Crd, ushort enable);

        /// <summary>
        /// 读取连续插补中Blend拐角过渡模式的使能状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="enable">返回使能状态：0 - 禁止；1 - 允许。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_get_blend(ushort ConnectNo, ushort Crd, ref ushort enable);

        /// <summary>
        /// 在连续插补中控制一个不参与插补的轴执行定长运动。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="axis">指定的轴号，该轴不能是参与连续插补的轴。</param>
        /// <param name="dist">目标位置，单位：unit。</param>
        /// <param name="posi_mode">运动模式：0 - 相对坐标模式；1 - 绝对坐标模式。</param>
        /// <param name="mode">
        /// 启动模式：
        /// 0 - 暂停启动：待缓冲区中上一段插补结束后，执行此定长运动，完成后再执行下一段插补。
        /// 1 - 直接启动：待缓冲区中上一段插补结束后，立即执行此定长运动，并同时开始执行下一段插补。
        /// </param>
        /// <param name="imark">用户定义的标号，任意指定，0表示自动编号。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_pmove_unit(ushort ConnectNo, ushort Crd, ushort axis, double dist, ushort posi_mode, ushort mode, int imark);

        /// <summary>
        /// 在连续插补缓冲区中添加一条暂停延时指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="delay_time">延时时间，单位：秒。若为0，则无限长延时。</param>
        /// <param name="mark">用户定义的标号，任意指定，0表示自动编号。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_delay(ushort ConnectNo, ushort Crd, double delay_time, int mark);

        /*********************************************************************************************************
        PWM功能
        *********************************************************************************************************/

        /// <summary>
        /// 设置PWM功能的使能状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="PwmNo">PWM通道号，范围：0-1。</param>
        /// <param name="enable">使能状态：0 - 禁止；1 - 使能。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出，但功能描述可在文档中找到。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_pwm_enable(ushort ConnectNo, ushort PwmNo, ushort enable);

        /// <summary>
        /// 读取PWM功能的使能状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="PwmNo">PWM通道号，范围：0-1。</param>
        /// <param name="enable">返回使能状态。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_pwm_enable(ushort ConnectNo, ushort PwmNo, ref ushort enable);

        /// <summary>
        /// 设置PWM输出固定的高电平脉冲宽度。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pwm_no">PWM通道号，范围：0-1。</param>
        /// <param name="enable">使能状态：0 - 禁止；1 - 使能。</param>
        /// <param name="high_width_s">高电平宽度，单位：秒。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_pwm_fix_high_width(ushort ConnectNo, ushort pwm_no, ushort enable, double high_width_s);

        /// <summary>
        /// 获取PWM输出的固定高电平脉冲宽度设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pwm_no">PWM通道号，范围：0-1。</param>
        /// <param name="enable">返回使能状态。</param>
        /// <param name="high_width_s">返回高电平宽度，单位：秒。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_pwm_fix_high_width(ushort ConnectNo, ushort pwm_no, ref ushort enable, ref double high_width_s);

        /// <summary>
        /// 设置PWM立即输出的参数。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="PwmNo">PWM通道号，范围：0-1。</param>
        /// <param name="fDuty">占空比，取值范围：0-1。</param>
        /// <param name="fFre">频率，取值范围：1Hz - 500KHz。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_pwm_output(ushort ConnectNo, ushort PwmNo, double fDuty, double fFre);

        /// <summary>
        /// 读取PWM当前的输出参数。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="PwmNo">PWM通道号，范围：0-1。</param>
        /// <param name="fDuty">返回当前的占空比设置值。</param>
        /// <param name="fFre">返回当前的频率设置值。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_pwm_output(ushort ConnectNo, ushort PwmNo, ref double fDuty, ref double fFre);

        /// <summary>
        /// 在连续插补中设置PWM输出参数。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-7，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="pwmno">PWM通道号，范围：0-1。</param>
        /// <param name="fDuty">占空比，取值范围：0-1。</param>
        /// <param name="fFre">频率，取值范围：0-2MHz。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>该函数仅设置参数，不会立即输出PWM信号，需配合smc_conti_write_pwm等函数使用。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_set_pwm_output(ushort ConnectNo, ushort Crd, ushort pwmno, double fDuty, double fFre);

        /*********************************************************************************************************
        PWM速度跟随
        *********************************************************************************************************/
        /// <summary>
        /// 在连续插补中设置PWM速度跟随。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-7，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="pwmno">PWM通道号，范围：0-1。</param>
        /// <param name="mode">
        /// 跟随模式：
        /// 0 - 不跟随，保持状态；
        /// 1 - 不跟随，输出低电平；
        /// 2 - 不跟随，输出高电平；
        /// 3 - 跟随，占空比自动调整；
        /// 4 - 跟随，频率自动调整。
        /// </param>
        /// <param name="MaxVel">最大运行速度，单位：unit/s。</param>
        /// <param name="MaxValue">最大输出值。模式3时为最大占空比(0-1)；模式4时为最大频率(0-2MHz)。</param>
        /// <param name="OutValue">固定输出值。模式3时为固定频率；模式4时为固定占空比。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_set_pwm_follow_speed(ushort ConnectNo, ushort Crd, ushort pwmno, ushort mode, double MaxVel, double MaxValue, double OutValue);

        /// <summary>
        /// 读取连续插补中PWM速度跟随的参数设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-7，默认值0。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="pwmno">PWM通道号，范围：0-1。</param>
        /// <param name="mode">返回跟随模式。</param>
        /// <param name="MaxVel">返回最大运行速度。</param>
        /// <param name="MaxValue">返回最大输出值。</param>
        /// <param name="OutValue">返回固定输出值。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_get_pwm_follow_speed(ushort ConnectNo, ushort Crd, ushort pwmno, ref ushort mode, ref double MaxVel, ref double MaxValue, ref double OutValue);

        /// <summary>
        /// 设置PWM在打开和关闭状态下对应的占空比。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-7，默认值0。</param>
        /// <param name="PwmNo">PWM通道号，范围：0-1。</param>
        /// <param name="fOnDuty">PWM打开状态的占空比，范围：0-1。</param>
        /// <param name="fOffDuty">PWM关闭状态的占空比，范围：0-1。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_pwm_onoff_duty(ushort ConnectNo, ushort PwmNo, double fOnDuty, double fOffDuty);

        /// <summary>
        /// 读取PWM在打开和关闭状态下对应的占空比设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-7，默认值0。</param>
        /// <param name="PwmNo">PWM通道号，范围：0-1。</param>
        /// <param name="fOnDuty">返回PWM打开状态的占空比。</param>
        /// <param name="fOffDuty">返回PWM关闭状态的占空比。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_pwm_onoff_duty(ushort ConnectNo, ushort PwmNo, ref double fOnDuty, ref double fOffDuty);

        /// <summary>
        /// 设置PWM速度跟随时对应的开关状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pwmno">PWM通道号，范围：0-1。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="on_off">开关状态：0 - 关闭；1 - 打开。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_pwm_follow_onoff(ushort ConnectNo, ushort pwmno, ushort Crd, ushort on_off);

        /// <summary>
        /// 获取PWM速度跟随时对应的开关状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pwmno">PWM通道号，范围：0-1。</param>
        /// <param name="Crd">返回坐标系号。</param>
        /// <param name="on_off">返回开关状态。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_pwm_follow_onoff(ushort ConnectNo, ushort pwmno, ref ushort Crd, ref ushort on_off);

        /// <summary>
        /// 在连续插补中添加一条相对于轨迹段【起点】的PWM滞后输出指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-7。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="pwmno">PWM通道号，范围：0-1。</param>
        /// <param name="on_off">输出状态：0 - 关闭；1 - 打开。</param>
        /// <param name="delay_value">滞后值，单位由delay_mode决定。</param>
        /// <param name="delay_mode">滞后模式：0 - 滞后时间（单位s）；1 - 滞后距离（单位unit）。</param>
        /// <param name="ReverseTime">保留参数，固定值为0。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_delay_pwm_to_start(ushort ConnectNo, ushort Crd, ushort pwmno, ushort on_off, double delay_value, ushort delay_mode, double ReverseTime);

        /// <summary>
        /// 连续插补中相对于轨迹段终点PWM滞后输出。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="pwmno">PWM通道号，范围：0-1。</param>
        /// <param name="on_off">输出状态：0 - 关闭；1 - 打开。</param>
        /// <param name="delay_time">滞后时间，单位s。</param>
        /// <param name="ReverseTime">保留参数，固定值为0。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_delay_pwm_to_stop(ushort ConnectNo, ushort Crd, ushort pwmno, ushort on_off, double delay_time, double ReverseTime);

        /// <summary>
        /// 在连续插补中添加一条相对于轨迹段【终点】的PWM提前输出指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-7。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="bitno">PWM通道号，范围：0-1。</param>
        /// <param name="on_off">输出状态：0 - 关闭；1 - 打开。</param>
        /// <param name="ahead_value">提前值，单位由ahead_mode决定。</param>
        /// <param name="ahead_mode">提前模式：0 - 提前时间（单位s）；1 - 提前距离（单位unit）。</param>
        /// <param name="ReverseTime">保留参数，固定值为0。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_ahead_pwm_to_stop(ushort ConnectNo, ushort Crd, ushort bitno, ushort on_off, double ahead_value, ushort ahead_mode, double ReverseTime);

        /// <summary>
        /// 在连续插补缓冲区中添加一条立即PWM输出指令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-7。</param>
        /// <param name="Crd">坐标系号，取值范围：0-1。</param>
        /// <param name="pwmno">PWM通道号，范围：0-1。</param>
        /// <param name="on_off">输出状态：0 - 关闭；1 - 打开。</param>
        /// <param name="ReverseTime">保留参数，固定值为0。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_write_pwm(ushort ConnectNo, ushort Crd, ushort pwmno, ushort on_off, double ReverseTime);

        /*********************************************************************************************************
        编码器功能
        *********************************************************************************************************/
        /// <summary>
        /// 设置编码器的计数方式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="mode">编码器的计数方式：0 - 非A/B相(脉冲/方向)；1 - 1倍频A/B相；2 - 2倍频A/B相；3 - 4倍频A/B相。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_counter_inmode(ushort ConnectNo, ushort axis, ushort mode);

        /// <summary>
        /// 读取编码器的计数方式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="mode">返回编码器的计数方式：0 - 非A/B相(脉冲/方向)；1 - 1倍频A/B相；2 - 2倍频A/B相；3 - 4倍频A/B相。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_counter_inmode(ushort ConnectNo, ushort axis, ref ushort mode);

        /// <summary>
        /// 设置AB相计数的方向。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="reverse">计数方向：0 - A超前B为增加计数；1 - B超前A为增加计数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_counter_reverse(ushort ConnectNo, ushort axis, ushort reverse);

        /// <summary>
        /// 读取AB相计数的方向设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="reverse">返回计数方向：0 - A超前B为增加计数；1 - B超前A为增加计数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_counter_reverse(ushort ConnectNo, ushort axis, ref ushort reverse);

        /// <summary>
        /// 设置指定轴的编码器当前计数值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="pos">要设置的编码器计数值，单位：unit。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_encoder_unit(ushort ConnectNo, ushort axis, double pos);

        /// <summary>
        /// 读取指定轴的编码器当前计数值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="pos">返回编码器的当前位置值，单位：unit。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_encoder_unit(ushort ConnectNo, ushort axis, ref double pos);

        /*********************************************************************************************************
        辅助编码器功能
        *********************************************************************************************************/
        /// <summary>
        /// 设置辅助编码器的计数模式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="channel">辅助编码器通道号。</param>
        /// <param name="inmode">输入模式，例如A/B相或脉冲/方向。</param>
        /// <param name="multi">倍率（倍频）。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_extra_encoder_mode(ushort ConnectNo, ushort channel, ushort inmode, ushort multi);

        /// <summary>
        /// 获取辅助编码器的计数模式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="channel">辅助编码器通道号。</param>
        /// <param name="inmode">返回输入模式。</param>
        /// <param name="multi">返回倍率（倍频）。</param>
        /// <returns>错误代码，0表示成功。</returns>
        /// <remarks>此函数未在提供的V2.1版API文档中列出。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_extra_encoder_mode(ushort ConnectNo, ushort channel, ref ushort inmode, ref ushort multi);

        /// <summary>
        /// 设置辅助编码器的当前位置值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">辅助编码器对应的轴号。</param>
        /// <param name="pos">要设置的位置值。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_extra_encoder(ushort ConnectNo, ushort axis, int pos);

        /// <summary>
        /// 获取辅助编码器的当前位置值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">辅助编码器对应的轴号。</param>
        /// <param name="pos">返回当前的位置值。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_extra_encoder(ushort ConnectNo, ushort axis, ref int pos);

        /*********************************************************************************************************
        通用IO操作
        *********************************************************************************************************/
        /// <summary>
        /// 读取指定控制器的某一个通用输入端口的电平状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="bitno">输入端口号，范围：0 到 控制器本机输入口数-1。</param>
        /// <returns>指定的输入端口电平：0 - 低电平；1 - 高电平。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_inbit(ushort ConnectNo, ushort bitno);

        /// <summary>
        /// 设置指定控制器的某一个通用输出端口的电平状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="bitno">输出端口号，范围：0 到 控制器本机输出口数-1。</param>
        /// <param name="on_off">要设置的输出电平：0 - 低电平；1 - 高电平。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_write_outbit(ushort ConnectNo, ushort bitno, ushort on_off);

        /// <summary>
        /// 读取指定控制器的某一个通用输出端口的当前电平状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="bitno">输出端口号，范围：0 到 控制器本机输出口数-1。</param>
        /// <returns>指定的输出端口电平：0 - 低电平；1 - 高电平。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_outbit(ushort ConnectNo, ushort bitno);

        /// <summary>
        /// 读取指定控制器的一组通用输入端口的电平状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="portno">IO组号。对于SMC604A，范围0-1；其他控制器为0。</param>
        /// <returns>一个32位无符号整数，每一位代表一个IO口的状态。需要进行位运算来解析。</returns>
        /// <remarks>在IO口在32位内PORT号为0，超出32位PORT为1。返回值为10进制，需转换为2进制后，bit值对应各端口输入状态值。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern uint smc_read_inport(ushort ConnectNo, ushort portno);

        /// <summary>
        /// 读取指定控制器的一组通用输出端口的电平状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="portno">IO组号。对于SMC604A，范围0-1；其他控制器为0。</param>
        /// <returns>一个32位无符号整数，每一位代表一个IO口的状态。</returns>
        [DllImport("LTSMC.dll")]
        public static extern uint smc_read_outport(ushort ConnectNo, ushort portno);

        /// <summary>
        /// 设置指定控制器的一组通用输出端口的电平状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254，默认值0。</param>
        /// <param name="portno">IO组号。对于SMC604A，范围0-1；其他控制器为0。</param>
        /// <param name="outport_val">要设置的端口电平数值，一个32位无符号整数，每一位对应一个端口。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_write_outport(ushort ConnectNo, ushort portno, uint outport_val);

        /// <summary>
        /// 读取指定控制器的某一个通用输入端口的电平状态（带错误码返回）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="bitno">输入端口号。</param>
        /// <param name="state">返回端口电平状态：0 - 低电平；1 - 高电平。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_inbit_ex(ushort ConnectNo, ushort bitno, ref ushort state);

        /// <summary>
        /// 读取指定控制器的某一个通用输出端口的电平状态（带错误码返回）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="bitno">输出端口号。</param>
        /// <param name="state">返回端口电平状态：0 - 低电平；1 - 高电平。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_outbit_ex(ushort ConnectNo, ushort bitno, ref ushort state);

        /// <summary>
        /// 读取指定控制器的全部输入端口的电平状态（带错误码返回）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="portno">IO组号。对于SMC604A，取值范围为0-1；对于其他控制器，此值为0。</param>
        /// <param name="state">通过引用返回一个无符号短整型，其位模式代表了对应IO组中所有输入端口的电平状态。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>
        /// 注意：当IO口数量在32个以内时，portno为0。超出32个时，portno为1。
        /// 返回的state值为10进制，需要转换为2进制后，其比特位才对应各端口的输入状态。
        /// </remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_inport_ex(ushort ConnectNo, ushort portno, ref ushort state);

        /// <summary>
        /// 读取指定控制器的全部输出端口的电平状态（带错误码返回）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="portno">IO组号。对于SMC604A，取值范围为0-1；对于其他控制器，此值为0。</param>
        /// <param name="state">通过引用返回一个无符号短整型，其位模式代表了对应IO组中所有输出端口的电平状态。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>
        /// 注意：当IO口数量在32个以内时，portno为0。超出32个时，portno为1。
        /// 返回的state值为10进制，需要转换为2进制后，其比特位才对应各端口的输出状态。
        /// </remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_outport_ex(ushort ConnectNo, ushort portno, ref ushort state);

        //通用IO特殊功能
        /// <summary>
        /// 设置指定输出端口延时翻转。函数执行后，该端口立即输出与当前相反的电平，并在指定时间后再次翻转恢复。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="bitno">输出端口号，范围：0 到 控制器最大输出口数-1。</param>
        /// <param name="reverse_time">延时翻转时间，单位：s。如果设置为0，则翻转时间为无限长。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_reverse_outbit(ushort ConnectNo, ushort bitno, double reverse_time);

        /// <summary>
        /// 设置IO输出延时翻转（此函数在提供的文档中未明确定义，功能基于函数名推断）。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="channel">通道号。</param>
        /// <param name="bitno">端口号。</param>
        /// <param name="level">目标电平。</param>
        /// <param name="reverse_time">翻转时间。</param>
        /// <param name="outmode">输出模式。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_outbit_delay_reverse(ushort ConnectNo, ushort channel, ushort bitno, ushort level, double reverse_time, ushort outmode);

        //设置IO输出一定脉冲个数的PWM波形曲线
        /// <summary>
        /// 设置指定IO端口输出一定数量脉冲的PWM波形。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="outbit">输出端口号。</param>
        /// <param name="fre">PWM波形的频率，单位：Hz。</param>
        /// <param name="duty">PWM波形的占空比，范围：0.0 - 1.0。</param>
        /// <param name="counts">要输出的脉冲总数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_io_pwmoutput(ushort ConnectNo, ushort outbit, double fre, double duty, uint counts);//设置IO输出一定脉冲个数的PWM波形曲线

        //清除IO输出PWM波形曲线
        /// <summary>
        /// 停止并清除指定IO端口的PWM输出任务。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="outbit">输出端口号。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_clear_io_pwmoutput(ushort ConnectNo, ushort outbit);

        /// <summary>
        /// 设置通用输入IO的计数模式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="bitno">输入端口号，范围：0 到 控制器最大输入口数-1。</param>
        /// <param name="mode">IO计数模式。0: 禁止计数；1: 上升沿计数；2: 下降沿计数。</param>
        /// <param name="filter">滤波时间，单位：s。用于消除输入信号的抖动。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_io_count_mode(ushort ConnectNo, ushort bitno, ushort mode, double filter);

        /// <summary>
        /// 读取通用输入IO的计数模式设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="bitno">输入端口号，范围：0 到 控制器最大输入口数-1。</param>
        /// <param name="mode">通过引用返回IO计数模式。0: 禁止计数；1: 上升沿计数；2: 下降沿计数。</param>
        /// <param name="filter">通过引用返回当前设置的滤波时间，单位：s。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_io_count_mode(ushort ConnectNo, ushort bitno, ref ushort mode, ref double filter);

        /// <summary>
        /// 设置或重置指定IO输入端口的计数值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="bitno">输入端口号，范围：0 到 控制器最大输入口数-1。</param>
        /// <param name="CountValue">要设置的IO计数值。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_io_count_value(ushort ConnectNo, ushort bitno, uint CountValue);

        /// <summary>
        /// 读取指定IO输入端口的当前计数值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="bitno">输入端口号，范围：0 到 控制器最大输入口数-1。</param>
        /// <param name="CountValue">通过引用返回当前IO计数值。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_io_count_value(ushort ConnectNo, ushort bitno, ref uint CountValue);

        //虚拟IO映射 用于输入滤波功能
        /// <summary>
        /// 设置虚拟IO映射关系，可用于对输入信号进行滤波。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-7。</param>
        /// <param name="bitno">虚拟IO口号，范围：0-15。</param>
        /// <param name="MapIoType">虚拟IO映射类型。6: 通用输入端口(AxisIoInPort_IO)。</param>
        /// <param name="MapIoIndex">虚拟IO映射索引号。当MapIoType为6时，此参数为0-15之间的整数，代表具体的通用输入端口号。设置为65535表示取消该映射。</param>
        /// <param name="filter_time">虚拟IO信号滤波时间，单位：s，范围：0.001 – 2^31s。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_io_map_virtual(ushort ConnectNo, ushort bitno, ushort MapIoType, ushort MapIoIndex, double filter_time);

        /// <summary>
        /// 读取虚拟IO的映射关系设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-7。</param>
        /// <param name="bitno">虚拟IO口号，范围：0-15。</param>
        /// <param name="MapIoType">通过引用返回虚拟IO映射类型。</param>
        /// <param name="MapIoIndex">通过引用返回虚拟IO映射索引号。</param>
        /// <param name="filter_time">通过引用返回虚拟IO信号滤波时间，单位：s。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_io_map_virtual(ushort ConnectNo, ushort bitno, ref ushort MapIoType, ref ushort MapIoIndex, ref double filter_time);

        /// <summary>
        /// 读取经过虚拟IO映射和滤波后的输入端口电平状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="bitno">虚拟IO口号，范围：0 到 控制器最大输入口数-1。</param>
        /// <returns>返回指定虚拟IO口的电平状态：0 表示低电平，1 表示高电平。</returns>
        /// <remarks>
        /// 1. 此函数需要配合 smc_set_io_map_virtual 函数使用。
        /// 2. 与smc_read_inbit的区别在于，本函数读取的是经过滤波处理后的状态，而smc_read_inbit直接读取硬件端口的瞬时状态。
        /// </remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_inbit_virtual(ushort ConnectNo, ushort bitno);

        /*********************************************************************************************************
        专用IO操作
        *********************************************************************************************************/
        /// <summary>
        /// 设置IO触发减速停止(DSTP)信号。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。若设为255，则对所有轴生效。</param>
        /// <param name="enable">允许/禁止硬件信号功能。0: 禁止；1: 允许。</param>
        /// <param name="logic">外部减速停止信号的有效电平。0: 低电平有效；1: 高电平有效。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>减速停止的减速时间由函数 smc_set_dec_stop_time 设置。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_io_dstp_mode(ushort ConnectNo, ushort axis, ushort enable, ushort logic);

        /// <summary>
        /// 读取IO触发减速停止(DSTP)信号的设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="enable">通过引用返回DSTP信号功能状态。0: 禁止；1: 允许。</param>
        /// <param name="logic">通过引用返回设置的DSTP信号有效电平。0: 低电平有效；1: 高电平有效。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_io_dstp_mode(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort logic);

        /// <summary>
        /// 设置指定轴的伺服报警(ALM)信号参数。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。若设为255，则对所有轴生效。</param>
        /// <param name="enable">信号使能。0: 禁止；1: 允许。</param>
        /// <param name="alm_logic">信号的有效电平。0: 低电平有效；1: 高电平有效。</param>
        /// <param name="alm_action">信号触发后的制动方式。0: 立即停止。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_alm_mode(ushort ConnectNo, ushort axis, ushort enable, ushort alm_logic, ushort alm_action);

        /// <summary>
        /// 读取指定轴的伺服报警(ALM)信号设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="enable">通过引用返回ALM信号使能状态。</param>
        /// <param name="alm_logic">通过引用返回设置的ALM信号有效电平。</param>
        /// <param name="alm_action">通过引用返回ALM信号的制动方式。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_alm_mode(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort alm_logic, ref ushort alm_action);

        //硬件输入INP配置
        /// <summary>
        /// 设置指定轴的伺服到位(INP)信号。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。若设为255，则对所有轴生效。</param>
        /// <param name="enable">INP信号使能。0: 禁止；1: 允许。</param>
        /// <param name="inp_logic">INP信号的有效电平。0: 低电平有效；1: 高电平有效。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>适用范围：SMC300、SMC600系列控制器。当使能INP后，只有INP为有效状态时轴才能运动。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_inp_mode(ushort ConnectNo, ushort axis, ushort enable, ushort inp_logic);

        /// <summary>
        /// 读取指定轴的伺服到位(INP)信号设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="enable">通过引用返回INP信号使能状态。</param>
        /// <param name="inp_logic">通过引用返回设置的INP信号有效电平。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_inp_mode(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort inp_logic);

        //软件检测INP配置
        /// <summary>
        /// 设置软件检测伺服到位(INP)的参数。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="enable">使能软件INP检测功能。0: 禁止；1: 允许。</param>
        /// <param name="inp_error">到位误差范围，单位：unit。当指令位置与反馈位置的差值在此范围内时，认为可能到位。</param>
        /// <param name="inp_time_s">到位时间，单位：s。位置差值持续在此误差范围内超过该时间，才判定为真正到位。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_sinp_param_unit(ushort ConnectNo, ushort axis, ushort enable, double inp_error, double inp_time_s);

        /// <summary>
        /// 获取软件检测伺服到位(INP)的参数。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="enable">通过引用返回使能状态。</param>
        /// <param name="inp_error">通过引用返回INP误差范围，单位：unit。</param>
        /// <param name="inp_time_s">通过引用返回INP时间，单位：s。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_sinp_param_unit(ushort ConnectNo, ushort axis, ref ushort enable, ref double inp_error, ref double inp_time_s);

        /// <summary>
        /// 控制指定轴的伺服使能(SEVON)端口的输出电平。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="on_off">设置伺服使能端口电平。0: 低电平；1: 高电平。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>适用范围：SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_write_sevon_pin(ushort ConnectNo, ushort axis, ushort on_off);

        /// <summary>
        /// 读取指定轴的伺服使能(SEVON)端口的电平。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>伺服使能端口电平。0: 低电平；1: 高电平。</returns>
        /// <remarks>适用范围：SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_sevon_pin(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 控制指定轴的编码器复位(ERC)信号输出。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="on_off">输出电平。0: 低电平；1: 高电平。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>适用范围：SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_write_erc_pin(ushort ConnectNo, ushort axis, ushort on_off);

        /// <summary>
        /// 读取指定轴的编码器复位(ERC)端口电平。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>ERC端口电平。0: 低电平；1: 高电平。</returns>
        /// <remarks>适用范围：脉冲型SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_erc_pin(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 读取指定轴的报警(ALARM)端口电平。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>ALARM端口电平。0: 低电平；1: 高电平。</returns>
        /// <remarks>适用范围：脉冲型SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_alarm_pin(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 读取指定轴的到位(INP)端口电平。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>INP端口电平。0: 低电平；1: 高电平。</returns>
        /// <remarks>适用范围：脉冲型SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_inp_pin(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 读取指定轴的原点(ORG)端口电平。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>ORG端口电平。0: 低电平；1: 高电平。</returns>
        /// <remarks>适用范围：脉冲型SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_org_pin(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 读取指定轴的正硬限位(EL+)端口电平。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>EL+端口电平。0: 低电平；1: 高电平。</returns>
        /// <remarks>适用范围：脉冲型SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_elp_pin(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 读取指定轴的负硬限位(EL-)端口电平。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>EL-端口电平。0: 低电平；1: 高电平。</returns>
        /// <remarks>适用范围：脉冲型SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_eln_pin(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 读取指定轴的急停(EMG)端口电平。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>EMG端口电平。0: 低电平；1: 高电平。</returns>
        /// <remarks>适用范围：脉冲型SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_emg_pin(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 读取指定轴的编码器Z相(EZ)端口电平。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>EZ端口电平状态。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_ez_pin(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 读取指定轴的伺服准备好(RDY)端口电平。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>RDY端口电平状态。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_rdy_pin(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 读取指定高速比较(CMP)端口的电平。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">高速比较器，范围：0-1。</param>
        /// <returns>CMP端口电平。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_cmp_pin(ushort ConnectNo, ushort hcmp);

        /// <summary>
        /// 控制指定高速比较(CMP)端口的输出。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">高速比较器，范围：0-1。</param>
        /// <param name="on_off">输出电平。0: 低电平；1: 高电平。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>注意：该函数只在高速比较功能被禁止的状态下起作用。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_write_cmp_pin(ushort ConnectNo, ushort hcmp, ushort on_off);

        //带错误码返回值函数
        /// <summary>
        /// 读取伺服使能(SEVON)引脚状态（带错误码返回）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="state">通过引用返回端口电平状态。0: 低电平；1: 高电平。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_sevon_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);

        /// <summary>
        /// 读取编码器复位(ERC)引脚状态（带错误码返回）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="state">通过引用返回端口电平状态。0: 低电平；1: 高电平。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_erc_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);

        /// <summary>
        /// 读取报警(ALARM)引脚状态（带错误码返回）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="state">通过引用返回端口电平状态。0: 低电平；1: 高电平。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_alarm_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);

        /// <summary>
        /// 读取到位(INP)引脚状态（带错误码返回）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="state">通过引用返回端口电平状态。0: 低电平；1: 高电平。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_inp_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);

        /// <summary>
        /// 读取原点(ORG)引脚状态（带错误码返回）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="state">通过引用返回端口电平状态。0: 低电平；1: 高电平。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_org_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);

        /// <summary>
        /// 读取正硬限位(ELP)引脚状态（带错误码返回）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="state">通过引用返回端口电平状态。0: 低电平；1: 高电平。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_elp_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);

        /// <summary>
        /// 读取负硬限位(ELN)引脚状态（带错误码返回）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="state">通过引用返回端口电平状态。0: 低电平；1: 高电平。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_eln_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);

        /// <summary>
        /// 读取急停(EMG)引脚状态（带错误码返回）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="state">通过引用返回端口电平状态。0: 低电平；1: 高电平。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_emg_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);

        /// <summary>
        /// 读取伺服准备好(RDY)引脚状态（带错误码返回）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="state">通过引用返回端口电平状态。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_rdy_pin_ex(ushort ConnectNo, ushort axis, ref ushort state);

        /*********************************************************************************************************
        位置比较
        *********************************************************************************************************/
        //单轴位置比较	
        /// <summary>
        /// 设置一维位置比较器参数。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="enable">比较功能状态。0: 禁止；1: 使能。</param>
        /// <param name="cmp_source">比较源。0: 指令位置计数器；1: 编码器计数器。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_set_config(ushort ConnectNo, ushort axis, ushort enable, ushort cmp_source);

        /// <summary>
        /// 读取一维位置比较器设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="enable">通过引用返回比较功能状态。</param>
        /// <param name="cmp_source">通过引用返回比较源。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_get_config(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort cmp_source);

        /// <summary>
        /// 清除指定轴已添加的所有一维位置比较点。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_clear_points(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 添加一个一维位置比较点。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="pos">比较位置，单位：unit。</param>
        /// <param name="dir">比较模式。0: 小于等于；1: 大于等于。</param>
        /// <param name="action">比较点触发功能编号，具体参考API文档附录。</param>
        /// <param name="actpara">比较点触发功能参数，具体参考API文档附录。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_add_point_unit(ushort ConnectNo, ushort axis, double pos, ushort dir, ushort action, uint actpara);

        /// <summary>
        /// 添加一个一维循环比较点，用于产生周期性输出。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="pos">比较位置，单位: unit。</param>
        /// <param name="dir">比较模式。0: 小于等于；1: 大于等于。</param>
        /// <param name="bitno">要操作的输出端口号。</param>
        /// <param name="cycle">总线周期的倍数，设置为0则代表输出口保持。输出维持时间 = 总线周期 * cycle。</param>
        /// <param name="level">比较点输出口的电平。0: 低电平；1: 高电平。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_add_point_cycle(ushort ConnectNo, ushort axis, double pos, ushort dir, uint bitno, uint cycle, ushort level);

        /// <summary>
        /// 读取当前正在等待比较的一维比较点位置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="pos">通过引用返回当前比较点的位置，单位：unit。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_get_current_point_unit(ushort ConnectNo, ushort axis, ref double pos);

        /// <summary>
        /// 查询已经触发过的一维位置比较点的数量。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="pointNum">通过引用返回已经比较过的点的数量。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_get_points_runned(ushort ConnectNo, ushort axis, ref int pointNum);

        /// <summary>
        /// 查询还可以添加的一维位置比较点的数量。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="pointNum">通过引用返回当前可添加的比较点数量（最大256）。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_get_points_remained(ushort ConnectNo, ushort axis, ref int pointNum);

        //二维位置比较
        /// <summary>
        /// 设置二维位置比较器。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="enable">使能状态。0: 禁止；1: 使能。</param>
        /// <param name="cmp_source">比较源。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_set_config_extern(ushort ConnectNo, ushort enable, ushort cmp_source);

        /// <summary>
        /// 获取二维位置比较器配置。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="enable">通过引用返回使能状态。</param>
        /// <param name="cmp_source">通过引用返回比较源。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_get_config_extern(ushort ConnectNo, ref ushort enable, ref ushort cmp_source);

        /// <summary>
        /// 清除所有二维位置比较点。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_clear_points_extern(ushort ConnectNo);

        /// <summary>
        /// 添加二维位置比较点。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴列表，包含两个轴号。</param>
        /// <param name="pos">位置列表，包含两个轴的比较位置。</param>
        /// <param name="dir">方向列表，包含两个轴的比较模式。</param>
        /// <param name="action">动作。</param>
        /// <param name="actpara">动作参数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_add_point_extern_unit(ushort ConnectNo, ushort[] axis, double[] pos, ushort[] dir, ushort action, uint actpara);

        /// <summary>
        /// 添加二维循环比较点。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴列表，包含两个轴号。</param>
        /// <param name="pos">位置列表，包含两个轴的比较位置。</param>
        /// <param name="dir">方向列表，包含两个轴的比较模式。</param>
        /// <param name="bitno">位号。</param>
        /// <param name="cycle">周期。</param>
        /// <param name="level">电平。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_add_point_cycle_2d(ushort ConnectNo, ushort[] axis, double[] pos, ushort[] dir, uint bitno, uint cycle, int level);

        /// <summary>
        /// 读取当前二维位置比较点位置。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="pos">通过引用返回位置数组。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_get_current_point_extern_unit(ushort ConnectNo, double[] pos);

        /// <summary>
        /// 查询已经比较过的二维比较点个数。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="pointNum">通过引用返回点数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_get_points_runned_extern(ushort ConnectNo, ref int pointNum);

        /// <summary>
        /// 查询可以加入的二维比较点个数。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="pointNum">通过引用返回点数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_compare_get_points_remained_extern(ushort ConnectNo, ref int pointNum);

        //高速位置比较
        /// <summary>
        /// 设置高速比较模式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">高速比较器，范围：0-1。</param>
        /// <param name="cmp_mode">比较模式。0: 禁止；1: 等于；2: 小于；3: 大于；4: 队列；5: 线性。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_set_mode(ushort ConnectNo, ushort hcmp, ushort cmp_mode);

        /// <summary>
        /// 读取高速比较模式设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">高速比较器，范围：0-1。</param>
        /// <param name="cmp_mode">通过引用返回比较模式。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_get_mode(ushort ConnectNo, ushort hcmp, ref ushort cmp_mode);

        /// <summary>
        /// 配置高速比较器。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">高速比较器，范围：0-1。</param>
        /// <param name="axis">关联轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="cmp_source">比较位置源。0: 指令位置计数器；1: 编码器计数器。</param>
        /// <param name="cmp_logic">有效电平。0: 低电平；1: 高电平。</param>
        /// <param name="time">脉冲宽度，单位：us，范围：1us-20s。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_set_config(ushort ConnectNo, ushort hcmp, ushort axis, ushort cmp_source, ushort cmp_logic, int time);

        /// <summary>
        /// 读取高速比较器配置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">高速比较器，范围：0-1。</param>
        /// <param name="axis">通过引用返回关联轴号。</param>
        /// <param name="cmp_source">通过引用返回比较位置源。</param>
        /// <param name="cmp_logic">通过引用返回有效电平。</param>
        /// <param name="time">通过引用返回脉冲宽度。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_get_config(ushort ConnectNo, ushort hcmp, ref ushort axis, ref ushort cmp_source, ref ushort cmp_logic, ref int time);

        /// <summary>
        /// 添加或更新高速比较位置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">高速比较器，范围：0-1。</param>
        /// <param name="cmp_pos">比较位置，单位: unit。在队列模式下为添加，其他模式下为更新。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_add_point_unit(ushort ConnectNo, ushort hcmp, double cmp_pos);

        /// <summary>
        /// 设置高速比较线性模式的参数。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">高速比较器，范围：0-1。</param>
        /// <param name="Increment">位置增量值，单位：unit。</param>
        /// <param name="Count">比较次数，范围：1-65535。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_set_liner_unit(ushort ConnectNo, ushort hcmp, double Increment, int Count);

        /// <summary>
        /// 读取高速比较线性模式参数设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">高速比较器，范围：0-1。</param>
        /// <param name="Increment">通过引用返回位置增量值。</param>
        /// <param name="Count">通过引用返回比较次数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_get_liner_unit(ushort ConnectNo, ushort hcmp, ref double Increment, ref int Count);

        /// <summary>
        /// 读取高速比较状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">高速比较器，范围：0-1。</param>
        /// <param name="remained_points">通过引用返回可添加的比较点数。</param>
        /// <param name="current_point">通过引用返回当前比较点位置，单位: unit。</param>
        /// <param name="runned_points">通过引用返回已比较过的点数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_get_current_state_unit(ushort ConnectNo, ushort hcmp, ref int remained_points, ref double current_point, ref int runned_points);

        /// <summary>
        /// 清除已添加的所有高速位置比较点。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">高速比较器，范围：0-1。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_clear_points(ushort ConnectNo, ushort hcmp);

        //启用缓存方式添加比较位置
        /// <summary>
        /// 设置是否启用高速比较的缓存（FIFO）模式。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="hcmp">高速比较器。</param>
        /// <param name="fifo_mode">是否启用缓存方式。0: 不启用；1: 启用。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_fifo_set_mode(ushort ConnectNo, ushort hcmp, ushort fifo_mode);

        /// <summary>
        /// 读取高速比较的缓存（FIFO）模式设置。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="hcmp">高速比较器。</param>
        /// <param name="fifo_mode">通过引用返回是否启用缓存方式。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_fifo_get_mode(ushort ConnectNo, ushort hcmp, ushort[] fifo_mode);

        /// <summary>
        /// 读取高速比较缓存（FIFO）的剩余空间。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="hcmp">高速比较器。</param>
        /// <param name="remained_points">通过引用返回剩余缓存点数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_fifo_get_state(ushort ConnectNo, ushort hcmp, short[] remained_points);

        /// <summary>
        /// 在缓存（FIFO）模式下添加一个高速比较点（此函数已不推荐使用，请改用smc_hcmp_fifo_add_table）。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="hcmp">高速比较器。</param>
        /// <param name="num">点数。</param>
        /// <param name="cmp_pos">比较位置。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_fifo_add_point_unit(ushort ConnectNo, ushort hcmp, ushort num, double[] cmp_pos);

        /// <summary>
        /// 清除高速比较缓存（FIFO）中的所有数据，并同步清除FPGA中的位置。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="hcmp">高速比较器。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_fifo_clear_points(ushort ConnectNo, ushort hcmp);

        /// <summary>
        /// 通过数组批量向高速比较缓存（FIFO）中添加比较位置。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="hcmp">高速比较器。</param>
        /// <param name="num">数据点数，单次最多可添加25000点。</param>
        /// <param name="cmp_pos">包含比较位置的数组。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>注意：添加大量数据时，函数可能会阻塞一段时间，直到数据全部添加完成。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_fifo_add_table(ushort ConnectNo, ushort hcmp, ushort num, double[] cmp_pos);

        //二维高速位置比较
        /// <summary>
        /// 设置二维高速位置比较功能的使能状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">保留参数，固定为0。</param>
        /// <param name="cmp_enable">二维高速比较器使能。0: 禁止；1: 使能。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_set_enable(ushort ConnectNo, ushort hcmp, ushort cmp_enable);

        /// <summary>
        /// 读取二维高速位置比较功能的使能状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">保留参数，固定为0。</param>
        /// <param name="cmp_enable">通过引用返回二维高速比较器使能状态。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_get_enable(ushort ConnectNo, ushort hcmp, ref ushort cmp_enable);

        /// <summary>
        /// 配置二维高速比较器。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">保留参数，固定为0。</param>
        /// <param name="cmp_mode">比较模式。0: 进入误差带后触发；1: 进入误差带且单轴等于后再触发。</param>
        /// <param name="x_axis">X轴关联的轴号。</param>
        /// <param name="x_cmp_source">X轴比较位置源，固定为1: 辅助编码器计数器。</param>
        /// <param name="x_cmp_error">X轴的误差带设置，单位: unit。</param>
        /// <param name="y_axis">Y轴关联的轴号。</param>
        /// <param name="y_cmp_source">Y轴比较位置源，固定为1: 辅助编码器计数器。</param>
        /// <param name="y_cmp_error">Y轴的误差带设置，单位: unit。</param>
        /// <param name="cmp_logic">输出有效电平。0: 低电平；1: 高电平。</param>
        /// <param name="time">输出脉冲宽度，单位：us，范围：1us-20s。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_set_config_unit(ushort ConnectNo, ushort hcmp, ushort cmp_mode, ushort x_axis, ushort x_cmp_source, double x_cmp_error, ushort y_axis, ushort y_cmp_source, double y_cmp_error, ushort cmp_logic, int time);

        /// <summary>
        /// 读取二维高速比较器配置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">保留参数，固定为0。</param>
        /// <param name="cmp_mode">通过引用返回比较模式。</param>
        /// <param name="x_axis">通过引用返回X轴关联轴号。</param>
        /// <param name="x_cmp_source">通过引用返回X轴比较位置源。</param>
        /// <param name="x_cmp_error">通过引用返回X轴误差带。</param>
        /// <param name="y_axis">通过引用返回Y轴关联轴号。</param>
        /// <param name="y_cmp_source">通过引用返回Y轴比较位置源。</param>
        /// <param name="y_cmp_error">通过引用返回Y轴误差带。</param>
        /// <param name="cmp_logic">通过引用返回有效电平。</param>
        /// <param name="time">通过引用返回脉冲宽度。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_get_config_unit(ushort ConnectNo, ushort hcmp, ref ushort cmp_mode, ref ushort x_axis, ref ushort x_cmp_source, ref double x_cmp_error, ref ushort y_axis, ref ushort y_cmp_source, ref ushort y_cmp_error, ref ushort cmp_logic, ref int time);

        /// <summary>
        /// 配置二维高速比较的PWM输出模式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">保留参数，固定为0。</param>
        /// <param name="pwm_enable">PWM模式使能。0: 禁止；1: 使能。</param>
        /// <param name="duty">占空比，范围0.0-1.0。</param>
        /// <param name="freq">频率，单位Hz。</param>
        /// <param name="pwm_number">输出的PWM脉冲数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_set_pwmoutput(ushort ConnectNo, ushort hcmp, ushort pwm_enable, double duty, double freq, ushort pwm_number);

        /// <summary>
        /// 读取配置的二维高速比较PWM输出模式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">保留参数，固定为0。</param>
        /// <param name="pwm_enable">通过引用返回PWM使能状态。</param>
        /// <param name="duty">通过引用返回占空比。</param>
        /// <param name="freq">通过引用返回频率。</param>
        /// <param name="pwm_number">通过引用返回输出的PWM脉冲数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_get_pwmoutput(ushort ConnectNo, ushort hcmp, ref ushort pwm_enable, ref double duty, ref double freq, ref ushort pwm_number);

        /// <summary>
        /// 在队列模式下添加一个二维高速比较位置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">保留参数，固定为0。</param>
        /// <param name="x_cmp_pos">X轴的比较位置，单位：unit。</param>
        /// <param name="y_cmp_pos">Y轴的比较位置，单位：unit。</param>
        /// <param name="cmp_outbit">输出口号，范围：14-15。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_add_point_unit(ushort ConnectNo, ushort hcmp, double x_cmp_pos, double y_cmp_pos, ushort cmp_outbit);

        /// <summary>
        /// 读取二维高速比较的当前状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">保留参数，固定为0。</param>
        /// <param name="remained_points">通过引用返回可添加的比较点数。</param>
        /// <param name="x_current_point">通过引用返回当前X轴比较点位置。</param>
        /// <param name="y_current_point">通过引用返回当前Y轴比较点位置。</param>
        /// <param name="runned_points">通过引用返回已比较过的点数。</param>
        /// <param name="current_state">通过引用返回比较器状态。1: 正在输出；0: 输出完成。</param>
        /// <param name="current_outbit">通过引用返回当前输出口，范围：14-15。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_get_current_state_unit(ushort ConnectNo, ushort hcmp, ref int remained_points, ref double x_current_point, ref double y_current_point, ref int runned_points, ref ushort current_state, ref ushort current_outbit);

        /// <summary>
        /// 清除所有二维高速位置比较的缓冲点，并退出当前比较状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">保留参数，固定为0。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_clear_points(ushort ConnectNo, ushort hcmp);

        /// <summary>
        /// 强制二维高速比较输出。输出将按照预先配置好的脉冲模式或PWM模式进行。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="hcmp">保留参数，固定为0。</param>
        /// <param name="enable">使能标识。1: 使能；0: 失能。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_force_output(ushort ConnectNo, ushort hcmp, ushort enable);

        //二维高速位置比较缓存
        /// <summary>
        /// 启用二维高速比较的缓存（FIFO）模式。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="hcmp">高速比较器。</param>
        /// <param name="fifo_mode">是否启用缓存方式。0: 不启用；1: 启用。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_fifo_set_mode(ushort ConnectNo, ushort hcmp, ushort fifo_mode);

        /// <summary>
        /// 获取二维高速比较缓存（FIFO）模式。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="hcmp">高速比较器。</param>
        /// <param name="fifo_mode">通过引用返回FIFO模式。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_fifo_get_mode(ushort ConnectNo, ushort hcmp, ushort[] fifo_mode);

        /// <summary>
        /// 获取二维高速比较缓存（FIFO）状态。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="hcmp">高速比较器。</param>
        /// <param name="remained_points">通过引用返回剩余点数。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_fifo_get_state(ushort ConnectNo, ushort hcmp, long[] remained_points);

        /// <summary>
        /// 向二维高速比较FIFO中添加一个比较点（此函数已不推荐使用，请改用smc_hcmp_2d_fifo_add_table_unit）。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="hcmp">高速比较器。</param>
        /// <param name="num">点数。</param>
        /// <param name="x_cmp_pos">X比较位置。</param>
        /// <param name="y_cmp_pos">Y比较位置。</param>
        /// <param name="cmp_outbit">比较输出位。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_fifo_add_point_unit(ushort ConnectNo, ushort hcmp, ushort num, double[] x_cmp_pos, double[] y_cmp_pos, ushort cmp_outbit);

        /// <summary>
        /// 清除二维高速比较FIFO中的所有点，并同步清除FPGA中的位置。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="hcmp">高速比较器。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_fifo_clear_points(ushort ConnectNo, ushort hcmp);

        /// <summary>
        /// 通过数组批量向二维高速比较缓存（FIFO）中添加比较位置。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="hcmp">高速比较器。</param>
        /// <param name="num">数据点数，单次最多100点。</param>
        /// <param name="x_cmp_pos">X轴比较位置数组。</param>
        /// <param name="y_cmp_pos">Y轴比较位置数组。</param>
        /// <param name="outbit">输出口（此参数在当前版本API中可能已不使用，请参考最新文档）。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_hcmp_2d_fifo_add_table_unit(ushort ConnectNo, ushort hcmp, ushort num, double[] x_cmp_pos, double[] y_cmp_pos, ushort outbit);

        /*********************************************************************************************************
        原点锁存
        *********************************************************************************************************/
        /// <summary>
        /// 设置原点锁存(Home Latch)模式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="enable">原点锁存使能。0: 禁止；1: 允许。</param>
        /// <param name="logic">触发方式。0: 下降沿；1: 上升沿。</param>
        /// <param name="source">位置源选择。0: 指令位置计数器；1: 编码器计数器。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>适用范围：SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_homelatch_mode(ushort ConnectNo, ushort axis, ushort enable, ushort logic, ushort source);

        /// <summary>
        /// 读取原点锁存(Home Latch)模式设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="enable">通过引用返回原点锁存使能状态。</param>
        /// <param name="logic">通过引用返回触发方式。</param>
        /// <param name="source">通过引用返回位置源选择。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>适用范围：SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_homelatch_mode(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort logic, ref ushort source);

        /// <summary>
        /// 读取原点锁存标志。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>原点锁存标志。0: 未锁存；1: 已锁存。</returns>
        /// <remarks>适用范围：SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern int smc_get_homelatch_flag(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 清除（复位）原点锁存标志，以便进行下一次锁存。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>适用范围：SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_reset_homelatch_flag(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 读取原点锁存发生时的位置值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="value">通过引用返回锁存的位置值，单位：unit。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>适用范围：SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_homelatch_value_unit(ushort ConnectNo, ushort axis, ref double value);

        /*********************************************************************************************************
        EZ锁存
        *********************************************************************************************************/
        /// <summary>
        /// 设置编码器Z相(EZ)锁存模式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="enable">EZ锁存使能。0: 禁止；1: 允许。</param>
        /// <param name="logic">触发方式。0: 下降沿；1: 上升沿。</param>
        /// <param name="source">位置源选择。0: 指令位置计数器；1: 编码器计数器。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>适用范围：SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_ezlatch_mode(ushort ConnectNo, ushort axis, ushort enable, ushort logic, ushort source);

        /// <summary>
        /// 读取编码器Z相(EZ)锁存模式设置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="enable">通过引用返回EZ锁存使能状态。</param>
        /// <param name="logic">通过引用返回触发方式。</param>
        /// <param name="source">通过引用返回位置源选择。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>适用范围：SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ezlatch_mode(ushort ConnectNo, ushort axis, ref ushort enable, ref ushort logic, ref ushort source);

        /// <summary>
        /// 读取EZ锁存标志。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>EZ锁存标志。0: 未锁存；1: 已锁存。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ezlatch_flag(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 清除（复位）EZ锁存标志。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>适用范围：SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_reset_ezlatch_flag(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 读取EZ锁存发生时的位置值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="pos_by_mm">通过引用返回锁存的位置值，单位：unit。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>适用范围：SMC300、SMC600系列控制器。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ezlatch_value_unit(ushort ConnectNo, ushort axis, ref double pos_by_mm);
        /*********************************************************************************************************
        高速锁存
        *********************************************************************************************************/
        /// <summary>
        /// 设置高速锁存(LTC)模式（旧版函数）。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="ltc_logic">锁存信号的触发方式。</param>
        /// <param name="ltc_mode">锁存模式。</param>
        /// <param name="filter">滤波时间。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_ltc_mode(ushort ConnectNo, ushort axis, ushort ltc_logic, ushort ltc_mode, double filter);

        /// <summary>
        /// 读取高速锁存(LTC)模式（旧版函数）。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="ltc_logic">返回锁存信号的触发方式。</param>
        /// <param name="ltc_mode">返回锁存模式。</param>
        /// <param name="filter">返回滤波时间。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ltc_mode(ushort ConnectNo, ushort axis, ref ushort ltc_logic, ref ushort ltc_mode, ref double filter);

        /// <summary>
        /// 设置锁存模式（旧版函数）。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="latchmode">锁存模式。</param>
        /// <param name="latch_source">锁存源。</param>
        /// <param name="latch_channel">锁存通道。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_latch_mode(ushort ConnectNo, ushort axis, ushort latchmode, ushort latch_source, ushort latch_channel);

        /// <summary>
        /// 获取锁存模式（旧版函数）。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="latchmode">返回锁存模式。</param>
        /// <param name="latch_source">返回锁存源。</param>
        /// <param name="latch_channel">返回锁存通道。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_latch_mode(ushort ConnectNo, ushort axis, ref ushort latchmode, ref ushort latch_source, ref ushort latch_channel);

        /// <summary>
        /// 获取锁存标志（旧版函数）。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴号。</param>
        /// <returns>锁存标志。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_latch_flag(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 复位锁存标志（旧版函数）。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴号。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_reset_latch_flag(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 获取锁存值（旧版函数）。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="value">返回锁存值。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_latch_value_unit(ushort ConnectNo, ushort axis, ref double value);

        /*********************************************************************************************************
        高速锁存 新规划
        *********************************************************************************************************/
        /// <summary>
        /// 配置高速锁存器。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="latch">锁存器号，范围：0-3。</param>
        /// <param name="ltc_mode">锁存模式。0: 单次锁存；1: 连续锁存；3: 触发延时停止。</param>
        /// <param name="ltc_logic">锁存信号的触发方式。0: 下降沿锁存；1: 上升沿锁存；2: 双边沿锁存。</param>
        /// <param name="filter">滤波时间，单位：us。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_ltc_set_mode(ushort ConnectNo, ushort latch, ushort ltc_mode, ushort ltc_logic, double filter);

        /// <summary>
        /// 读取高速锁存器配置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="latch">锁存器号，范围：0-3。</param>
        /// <param name="ltc_mode">通过引用返回锁存模式。</param>
        /// <param name="ltc_logic">通过引用返回锁存信号的触发方式。</param>
        /// <param name="filter">通过引用返回滤波时间。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_ltc_get_mode(ushort ConnectNo, ushort latch, ref ushort ltc_mode, ref ushort ltc_logic, ref double filter);

        /// <summary>
        /// 配置高速锁存源。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="latch">锁存器号，范围：0-3。</param>
        /// <param name="axis">要锁存的轴号，范围：0 到 控制器最大轴数-1。</param>
        /// <param name="ltc_source">锁存源。0: 指令位置计数器；1: 编码器计数器。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_ltc_set_source(ushort ConnectNo, ushort latch, ushort axis, ushort ltc_source);

        /// <summary>
        /// 读取高速锁存源配置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="latch">锁存器号，范围：0-3。</param>
        /// <param name="axis">要锁存的轴号。</param>
        /// <param name="ltc_source">通过引用返回锁存源。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_ltc_get_source(ushort ConnectNo, ushort latch, ushort axis, ref ushort ltc_source);

        /// <summary>
        /// 复位指定的锁存器，清除锁存标志和已锁存的数据。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="latch">锁存器号，范围：0-3。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>当使用锁存功能前，必须先调用此函数复位锁存器的标志位。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_ltc_reset(ushort ConnectNo, ushort latch);

        /// <summary>
        /// 读取指定锁存器已锁存的数据个数。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="latch">锁存器号，范围：0-3。</param>
        /// <param name="axis">指定的轴号。</param>
        /// <param name="number">通过引用返回已锁存的数据个数。0表示无锁存值。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_ltc_get_number(ushort ConnectNo, ushort latch, ushort axis, ref int number);

        /// <summary>
        /// 读取锁存值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="latch">锁存器号，范围：0-3。</param>
        /// <param name="axis">指定的轴号。</param>
        /// <param name="value">通过引用返回锁存值。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        /// <remarks>
        /// 1. 该函数只可以读取辅助编码器的锁存值。
        /// 2. 在连续锁存模式下，每次调用会依次返回下一个锁存值。
        /// 3. 在单次锁存模式下，调用此函数不会自动清除已锁存个数，需手动调用smc_ltc_reset。
        /// </remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_ltc_get_value_unit(ushort ConnectNo, ushort latch, ushort axis, ref double value);

        /*********************************************************************************************************
        软件锁存 
        *********************************************************************************************************/
        /// <summary>
        /// 配置软件锁存器。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="latch">锁存器号，范围：0-1。</param>
        /// <param name="ltc_enable">使能锁存器。0: 禁止；1: 使能。</param>
        /// <param name="ltc_mode">锁存模式。0: 单次锁存；1: 连续锁存。</param>
        /// <param name="ltc_inbit">锁存触发的输入信号口。</param>
        /// <param name="ltc_logic">锁存触发边沿。0: 下降沿；1: 上升沿；2: 双边沿。</param>
        /// <param name="filter">滤波时间，单位：us。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_softltc_set_mode(ushort ConnectNo, ushort latch, ushort ltc_enable, ushort ltc_mode, ushort ltc_inbit, ushort ltc_logic, double filter);

        /// <summary>
        /// 读取软件锁存器配置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="latch">锁存器号，范围：0-1。</param>
        /// <param name="ltc_enable">通过引用返回使能状态。</param>
        /// <param name="ltc_mode">通过引用返回锁存模式。</param>
        /// <param name="ltc_inbit">通过引用返回锁存触发输入信号。</param>
        /// <param name="ltc_logic">通过引用返回锁存触发边沿。</param>
        /// <param name="filter">通过引用返回滤波时间。</param>
        /// <returns>错误代码。0表示成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_softltc_get_mode(ushort ConnectNo, ushort latch, ref ushort ltc_enable, ref ushort ltc_mode, ref ushort ltc_inbit, ref ushort ltc_logic, ref double filter);

        /// <summary>
        /// 配置软件锁存的锁存源。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="latch">锁存器号，范围：0-1。</param>
        /// <param name="axis">锁存对应的轴号。</param>
        /// <param name="ltc_source">配置锁存源。0 - 指令位置；1 - 编码器反馈位置。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_softltc_set_source(ushort ConnectNo, ushort latch, ushort axis, ushort ltc_source);

        /// <summary>
        /// 读取软件锁存的锁存源配置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="latch">锁存器号，范围：0-1。</param>
        /// <param name="axis">锁存对应的轴号。</param>
        /// <param name="ltc_source">返回锁存源。0 - 指令位置；1 - 编码器反馈位置。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_softltc_get_source(ushort ConnectNo, ushort latch, ushort axis, ref ushort ltc_source);

        /// <summary>
        /// 复位指定的软件锁存器，清除锁存标志和已锁存的数据。
        /// 注意：在单次锁存模式下，触发后需要调用此函数才能进行下一次锁存。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="latch">锁存器号，范围：0-1。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_softltc_reset(ushort ConnectNo, ushort latch);

        /// <summary>
        /// 读取软件锁存器已锁存的数据个数。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="latch">锁存器号，范围：0-1。</param>
        /// <param name="axis">锁存对应的轴号。</param>
        /// <param name="number">返回已锁存的数据个数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_softltc_get_number(ushort ConnectNo, ushort latch, ushort axis, ref int number);

        /// <summary>
        /// 读取软件锁存值。
        /// 注意：在连续锁存模式下，每次调用会依次读出缓存中的锁存值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="latch">锁存器号，范围：0-1。</param>
        /// <param name="axis">锁存对应的轴号。</param>
        /// <param name="value">返回读取到的锁存位置值。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_softltc_get_value_unit(ushort ConnectNo, ushort latch, ushort axis, ref double value);

        /*********************************************************************************************************
        模拟量操作
        *********************************************************************************************************/
        /// <summary>
        /// 设置模拟量输入触发功能参数。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="channel">通道号。</param>
        /// <param name="mode">模式。</param>
        /// <param name="fvoltage">电压值。</param>
        /// <param name="action">动作。</param>
        /// <param name="actpara">动作参数。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_ain_action(ushort ConnectNo, ushort channel, ushort mode, double fvoltage, ushort action, double actpara);

        /// <summary>
        /// 回读模拟量输入触发功能参数。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="channel">通道号。</param>
        /// <param name="mode">返回模式。</param>
        /// <param name="fvoltage">返回电压值。</param>
        /// <param name="action">返回动作。</param>
        /// <param name="actpara">返回动作参数。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ain_action(ushort ConnectNo, ushort channel, ref ushort mode, ref double fvoltage, ref ushort action, ref double actpara);

        /// <summary>
        /// 读取模拟量输入触发状态值。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="channel">通道号。</param>
        /// <returns>状态值。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ain_state(ushort ConnectNo, ushort channel);

        /// <summary>
        /// 置位模拟量输入触发状态，通常用于手动清除触发标志。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="channel">通道号。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_ain_state(ushort ConnectNo, ushort channel);

        /// <summary>
        /// 读取模拟量输入通道的电压值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="channel">通道号，可选值：0, 1。</param>
        /// <returns>返回当前通道的模拟电压值，范围：0-5V。</returns>
        [DllImport("LTSMC.dll")]
        public static extern double smc_get_ain(ushort ConnectNo, ushort channel);

        /// <summary>
        /// 读取模拟量输入通道的电压值（带错误码返回）。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="channel">通道号。</param>
        /// <param name="Vout">返回读取到的电压值。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ain_extern(ushort ConnectNo, ushort channel, ref double Vout);

        /// <summary>
        /// 获取AD输入值。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="da_no">AD通道号。</param>
        /// <param name="Vout">返回电压值。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ad_input(ushort ConnectNo, ushort da_no, ref double Vout);

        /// <summary>
        /// 获取所有AD输入值。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="Vout">返回所有通道的电压值数组。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_ad_input_all(ushort ConnectNo, ref double Vout);

        /// <summary>
        /// 设置模拟量（DA）输出值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="channel">通道号，可选值：0, 1。</param>
        /// <param name="fvoltage">要设置的模拟量输出电压值。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_da_output(ushort ConnectNo, ushort channel, double fvoltage);

        /// <summary>
        /// 读取当前设置的模拟量（DA）输出值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="channel">通道号，可选值：0, 1。</param>
        /// <param name="fvoltage">返回当前设置的模拟量输出电压值。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_da_output(ushort ConnectNo, ushort channel, ref double fvoltage);

        /*********************************************************************************************************
        文件操作
        *********************************************************************************************************/
        /// <summary>
        /// 下载PC端本地文件到控制器的FLASH。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pfilename">PC端本地文件名，必须包含完整路径。</param>
        /// <param name="pfilenameinControl">存放在控制器内的文件名。</param>
        /// <param name="filetype">文件类型：0-Basic, 1-Gcode, 2-参数, 3-固件, 200-eni(总线配置文件), 201-ini文件。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_download_file(ushort ConnectNo, string pfilename, byte[] pfilenameinControl, ushort filetype);

        /// <summary>
        /// 下载PC端的内存数据到控制器的FLASH。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pbuffer">存放文件内容的内存缓冲区。</param>
        /// <param name="buffsize">内存文件大小，单位：字节。</param>
        /// <param name="pfilenameinControl">存放在控制器内的文件名。根据文件类型，可能需要为空字符串("")。</param>
        /// <param name="filetype">文件类型：0-Basic, 1-Gcode, 2-参数, 3-固件, 200-eni, 201-ini文件。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_download_memfile(ushort ConnectNo, byte[] pbuffer, uint buffsize, byte[] pfilenameinControl, ushort filetype);

        /// <summary>
        /// 上传控制器的FLASH文件到PC端本地。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pfilename">PC端目标文件名，必须包含完整路径。</param>
        /// <param name="pfilenameinControl">控制器内的文件名。</param>
        /// <param name="filetype">文件类型：0-Basic, 1-Gcode, 2-参数, 3-固件, 200-eni, 201-ini文件。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_upload_file(ushort ConnectNo, string pfilename, byte[] pfilenameinControl, ushort filetype);

        /// <summary>
        /// 上传控制器的FLASH文件到PC端内存。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pbuffer">用于接收文件内容的内存缓冲区。</param>
        /// <param name="buffsize">内存文件缓冲区大小，单位：字节。</param>
        /// <param name="pfilenameinControl">控制器内的文件名。根据文件类型，可能需要为空字符串("")。</param>
        /// <param name="puifilesize">返回控制器内实际文件的大小，单位：字节。</param>
        /// <param name="filetype">文件类型：0-Basic, 1-Gcode, 2-参数, 3-固件, 200-eni, 201-ini文件。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_upload_memfile(ushort ConnectNo, byte[] pbuffer, uint buffsize, byte[] pfilenameinControl, ref uint puifilesize, ushort filetype);

        /// <summary>
        /// 下载PC端本地文件到控制器RAM，掉电后内容丢失。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pfilename">PC端本地文件名，必须包含完整路径。</param>
        /// <param name="filetype">文件类型：0-Basic, 1-Gcode, 2-参数, 3-固件, 200-eni, 201-ini文件。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_download_file_to_ram(ushort ConnectNo, string pfilename, ushort filetype);

        /// <summary>
        /// 下载PC端内存文件到控制器RAM，掉电后内容丢失。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pbuffer">存放文件内容的内存缓冲区。</param>
        /// <param name="buffsize">内存文件大小，单位：字节。</param>
        /// <param name="filetype">文件类型：0-Basic, 1-Gcode, 2-参数, 3-固件, 200-eni, 201-ini文件。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_download_memfile_to_ram(ushort ConnectNo, byte[] pbuffer, uint buffsize, ushort filetype);

        /// <summary>
        /// 获取文件下载进度。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="progress">返回文件下载进度值（0.0-1.0）。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_progress(ushort ConnectNo, ref float progress);

        /********************************************************************************************************
        U盘文件管理
        (注意：以下U盘相关函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        *********************************************************************************************************/
        /// <summary>
        /// 获取U盘状态。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="state">返回U盘状态。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_udisk_get_state(ushort ConnectNo, ref ushort state);

        /// <summary>
        /// 检查U盘内部文件。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="filename">文件名。</param>
        /// <param name="filesize">文件大小。</param>
        /// <param name="filetype">文件类型。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_udisk_check_file(ushort ConnectNo, byte[] filename, int[] filesize, ushort filetype);

        /// <summary>
        /// 获取U盘第一个文件。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="filename">文件名。</param>
        /// <param name="filesize">文件大小。</param>
        /// <param name="fileid">文件ID。</param>
        /// <param name="filetype">文件类型。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_udisk_get_first_file(ushort ConnectNo, byte[] filename, int[] filesize, int[] fileid, ushort filetype);

        /// <summary>
        /// 获取U盘下一个文件。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="filename">文件名。</param>
        /// <param name="filesize">文件大小。</param>
        /// <param name="fileid">文件ID。</param>
        /// <param name="filetype">文件类型。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_udisk_get_next_file(ushort ConnectNo, byte[] filename, int[] filesize, int[] fileid, ushort filetype);

        /// <summary>
        /// 复制U盘文件。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="SrcFileName">源文件名。</param>
        /// <param name="DstFileName">目标文件名。</param>
        /// <param name="filetype">文件类型。</param>
        /// <param name="mode">模式。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_udisk_copy_file(ushort ConnectNo, byte[] SrcFileName, byte[] DstFileName, ushort filetype, ushort mode);

        /*********************************************************************************************************
        寄存器操作
        *********************************************************************************************************/
        //Modbus寄存器
        /// <summary>
        /// 写位寄存器(0x区)。一个字节(pdata[i])对应8个位寄存器。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="start">寄存器首地址，范围：0-9999。</param>
        /// <param name="inum">要写入的位寄存器个数。</param>
        /// <param name="pdata">字节数组，包含要写入的数据。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_modbus_0x(ushort ConnectNo, ushort start, ushort inum, byte[] pdata);

        /// <summary>
        /// 读位寄存器(0x区)。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="start">寄存器首地址，范围：0-9999。</param>
        /// <param name="inum">要读取的位寄存器个数。</param>
        /// <param name="pdata">返回数据的字节数组。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_modbus_0x(ushort ConnectNo, ushort start, ushort inum, byte[] pdata);

        /// <summary>
        /// 写字寄存器(4x区)。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="start">寄存器首地址，范围：0-9999。</param>
        /// <param name="inum">要写入的字寄存器个数。</param>
        /// <param name="pdata">包含要写入数据的字(ushort)数组。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_modbus_4x(ushort ConnectNo, ushort start, ushort inum, ushort[] pdata);

        /// <summary>
        /// 读字寄存器(4x区)。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="start">寄存器首地址，范围：0-9999。</param>
        /// <param name="inum">要读取的字寄存器个数。</param>
        /// <param name="pdata">返回数据的字(ushort)数组。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_modbus_4x(ushort ConnectNo, ushort start, ushort inum, ushort[] pdata);

        /// <summary>
        /// 写浮点型字寄存器。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="start">起始地址。</param>
        /// <param name="inum">数量。</param>
        /// <param name="pdata">数据。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_modbus_4x_float(ushort ConnectNo, ushort start, ushort inum, float[] pdata);

        /// <summary>
        /// 读浮点型字寄存器。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="start">起始地址。</param>
        /// <param name="inum">数量。</param>
        /// <param name="pdata">返回数据。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_modbus_4x_float(ushort ConnectNo, ushort start, ushort inum, float[] pdata);

        /// <summary>
        /// 写整型字寄存器。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="start">起始地址。</param>
        /// <param name="inum">数量。</param>
        /// <param name="pdata">数据。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_modbus_4x_int(ushort ConnectNo, ushort start, ushort inum, int[] pdata);

        //掉电保持寄存器
        /// <summary>
        /// 设置掉电保存寄存器值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="start">寄存器首地址，范围：0-4096。</param>
        /// <param name="inum">寄存器个数，单次最大写入1024个字节。</param>
        /// <param name="pdata">要写入的寄存器值数组。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_persistent_reg(ushort ConnectNo, uint start, uint inum, byte[] pdata);

        /// <summary>
        /// 回读掉电保存寄存器数值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="start">寄存器首地址，范围：0-4096。</param>
        /// <param name="inum">寄存器个数，单次最大读取1024个字节。</param>
        /// <param name="pdata">返回读取到的寄存器值数组。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_persistent_reg(ushort ConnectNo, uint start, uint inum, byte[] pdata);

        //以下分类型区间
        /// <summary>
        /// 设置字节型掉电保持寄存器。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="start">起始地址。</param>
        /// <param name="inum">数量。</param>
        /// <param name="pdata">数据。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_persistent_reg_byte(ushort ConnectNo, uint start, uint inum, byte[] pdata);

        /// <summary>
        /// 获取字节型掉电保持寄存器。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="start">起始地址。</param>
        /// <param name="inum">数量。</param>
        /// <param name="pdata">返回数据。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_persistent_reg_byte(ushort ConnectNo, uint start, uint inum, byte[] pdata);

        /// <summary>
        /// 设置浮点型掉电保持寄存器。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="start">起始地址。</param>
        /// <param name="inum">数量。</param>
        /// <param name="pdata">数据。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_persistent_reg_float(ushort ConnectNo, uint start, uint inum, float[] pdata);

        /// <summary>
        /// 获取浮点型掉电保持寄存器。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="start">起始地址。</param>
        /// <param name="inum">数量。</param>
        /// <param name="pdata">返回数据。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_persistent_reg_float(ushort ConnectNo, uint start, uint inum, float[] pdata);

        /// <summary>
        /// 设置整型掉电保持寄存器。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="start">起始地址。</param>
        /// <param name="inum">数量。</param>
        /// <param name="pdata">数据。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_persistent_reg_int(ushort ConnectNo, uint start, uint inum, int[] pdata);

        /// <summary>
        /// 获取整型掉电保持寄存器。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="start">起始地址。</param>
        /// <param name="inum">数量。</param>
        /// <param name="pdata">返回数据。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_persistent_reg_int(ushort ConnectNo, uint start, uint inum, int[] pdata);

        /// <summary>
        /// 设置短整型掉电保持寄存器。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="start">起始地址。</param>
        /// <param name="inum">数量。</param>
        /// <param name="pdata">数据。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_persistent_reg_short(ushort ConnectNo, uint start, uint inum, short[] pdata);

        /// <summary>
        /// 获取短整型掉电保持寄存器。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="start">起始地址。</param>
        /// <param name="inum">数量。</param>
        /// <param name="pdata">返回数据。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_persistent_reg_short(ushort ConnectNo, uint start, uint inum, short[] pdata);

        /*********************************************************************************************************
        Basic程序控制
        *********************************************************************************************************/
        /// <summary>
        /// 按索引读取BASIC程序中的数组值（整型）。
        /// 注意：返回的long值为真实值乘以4294967296（左移32位），使用时需反向处理。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="name">数组名，多个名称用逗号隔开，如 "array0,array1"。</param>
        /// <param name="index">要读取的数组索引号。</param>
        /// <param name="var">返回读取到的数组值数组。</param>
        /// <param name="num">要读取的数组个数（应与name中的个数一致），同时返回实际读取的个数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_array(ushort ConnectNo, string name, uint index, long[] var, ref int num);

        /// <summary>
        /// 按索引修改BASIC程序中的数组值（整型）。
        /// 注意：传入的long值应为真实值乘以4294967296（右移32位）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="name">数组名，多个名称用逗号隔开。</param>
        /// <param name="index">要修改的数组索引号。</param>
        /// <param name="var">要写入的数组值数组。</param>
        /// <param name="num">要修改的数组个数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_modify_array(ushort ConnectNo, string name, uint index, long[] var, int num);

        /// <summary>
        /// 读取BASIC程序中的变量值（整型）。
        /// 注意：返回的long值为真实值除以4294967296，使用时需反向处理。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="varstring">变量名，多个名称用逗号隔开，如 "var0,var1"。</param>
        /// <param name="var">返回读取到的变量值数组。</param>
        /// <param name="num">要读取的变量个数，同时返回实际读取的个数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_var(ushort ConnectNo, string varstring, long[] var, ref int num);

        /// <summary>
        /// 修改BASIC程序中的变量值（整型）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="varstring">变量名，多个名称用逗号隔开。</param>
        /// <param name="var">要写入的变量值。</param>
        /// <param name="varnum">要修改的变量个数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_modify_var(ushort ConnectNo, string varstring, long[] var, int varnum);

        /// <summary>
        /// 批量写入BASIC程序中的数组值（整型）。
        /// 注意：传入的int值应为实际值右移32位后的值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="name">数组名，如 "array"。</param>
        /// <param name="startindex">起始写入的索引号。</param>
        /// <param name="var">要写入的数组值列表。</param>
        /// <param name="num">要写入的数组值个数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_write_array(ushort ConnectNo, string name, uint startindex, long[] var, int num);

        /// <summary>
        /// 按索引读取BASIC程序中的数组值（浮点型）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="name">数组名，多个名称用逗号隔开。</param>
        /// <param name="index">要读取的数组索引号。</param>
        /// <param name="var">返回读取到的double型数组值数组。</param>
        /// <param name="num">要读取的数组个数，同时返回实际读取的个数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_array_ex(ushort ConnectNo, string name, uint index, double[] var, ref int num);

        /// <summary>
        /// 按索引修改BASIC程序中的数组值（浮点型）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="name">数组名，多个名称用逗号隔开。</param>
        /// <param name="index">要修改的数组索引号。</param>
        /// <param name="var">要写入的double型数组值数组。</param>
        /// <param name="num">要修改的数组个数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_modify_array_ex(ushort ConnectNo, string name, uint index, double[] var, int num);

        /// <summary>
        /// 读取BASIC程序中的变量值（浮点型）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="varstring">变量名，多个名称用逗号隔开。</param>
        /// <param name="var">返回读取到的double型变量值数组。</param>
        /// <param name="num">要读取的变量个数，同时返回实际读取的个数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_var_ex(ushort ConnectNo, string varstring, double[] var, ref int num);

        /// <summary>
        /// 修改BASIC程序中的变量值（浮点型）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="varstring">变量名，多个名称用逗号隔开。</param>
        /// <param name="var">要写入的double型变量值数组。</param>
        /// <param name="varnum">要修改的变量个数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_modify_var_ex(ushort ConnectNo, string varstring, double[] var, int varnum);

        /// <summary>
        /// 批量写入BASIC程序中的数组值（浮点型）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="name">数组名。</param>
        /// <param name="startindex">起始写入的索引号。</param>
        /// <param name="var">要写入的double型数组值数组。</param>
        /// <param name="num">要写入的数组值个数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_write_array_ex(ushort ConnectNo, string name, uint startindex, double[] var, int num);

        /// <summary>
        /// 读取变量类型。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="varstring">变量名。</param>
        /// <param name="m_Type">返回变量类型。1-SUB, 2-全局变量, 3-全局数组, 4-参数, 5-命令, 6-关键字, 7-局部变量, 8-局部数组, 10-未知变量。</param>
        /// <param name="num">如果变量是数组，返回数组的长度。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_stringtype(ushort ConnectNo, string varstring, ref int m_Type, ref int num);

        /// <summary>
        /// 删除控制器中的BASIC程序。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_basic_delete_file(ushort ConnectNo);

        /// <summary>
        /// 运行控制器中的BASIC程序。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_basic_run(ushort ConnectNo);

        /// <summary>
        /// 停止运行中的BASIC程序。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_basic_stop(ushort ConnectNo);

        /// <summary>
        /// 暂停运行中的BASIC程序。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-7。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_basic_pause(ushort ConnectNo);

        /// <summary>
        /// 单步运行BASIC程序（执行一行）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_basic_step_run(ushort ConnectNo);

        /// <summary>
        /// 从当前暂停位置继续运行，直到遇到下一个断点。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_basic_step_over(ushort ConnectNo);

        /// <summary>
        /// 从暂停状态继续运行BASIC程序。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_basic_continue_run(ushort ConnectNo);

        /// <summary>
        /// 获取BASIC程序的当前运行状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="State">返回运行状态：1 - 运行中, 2 - 暂停中, 3 - 已停止, 100 - 异常。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_basic_state(ushort ConnectNo, ref ushort State);

        /// <summary>
        /// 获取BASIC程序当前正在执行的行号。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="line">返回当前执行的行号。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_basic_current_line(ushort ConnectNo, ref uint line);

        /// <summary>
        /// 获取BASIC程序中的断点信息。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="line">返回断点行号的列表数组。</param>
        /// <param name="linenum">断点总行数。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_basic_break_info(ushort ConnectNo, uint[] line, uint linenum);

        /// <summary>
        /// 读取BASIC程序的输出信息（如PRINT指令的输出）。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pbuff">用于接收输出信息的缓冲区。</param>
        /// <param name="uimax">缓冲区的大小。</param>
        /// <param name="puiread">返回实际读取到的信息大小。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_basic_message(ushort ConnectNo, byte[] pbuff, uint uimax, ref uint puiread);

        /// <summary>
        /// 向控制器发送在线命令。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pszCommand">要发送的命令字符串。</param>
        /// <param name="psResponse">用于接收控制器返回信息的字符串缓冲区。</param>
        /// <param name="uiResponseLength">返回字符串的长度。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_basic_command(ushort ConnectNo, byte[] pszCommand, byte[] psResponse, uint uiResponseLength);

        /*********************************************************************************************************
        G代码程序控制
        *********************************************************************************************************/
        /// <summary>
        /// 检查控制器中是否存在指定的G代码文件。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pfilenameinControl">要检查的控制器内文件名。</param>
        /// <param name="pbIfExist">返回文件是否存在。0 - 不存在；1 - 存在。</param>
        /// <param name="pFileSize">如果文件存在，返回文件大小。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_check_file(ushort ConnectNo, byte[] pfilenameinControl, ref byte pbIfExist, ref uint pFileSize);

        /// <summary>
        /// 读取控制器中的第一个G代码文件名。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pfilenameinControl">返回控制器文件名。</param>
        /// <param name="pFileSize">返回文件大小。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_get_first_file(ushort ConnectNo, byte[] pfilenameinControl, ref uint pFileSize);

        /// <summary>
        /// 在调用`smc_gcode_get_first_file`后，读取下一个G代码文件名。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pfilenameinControl">返回控制器文件名。</param>
        /// <param name="pFileSize">返回文件大小。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_get_next_file(ushort ConnectNo, byte[] pfilenameinControl, ref uint pFileSize);

        /// <summary>
        /// 启动当前设置的G代码程序。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_start(ushort ConnectNo);

        /// <summary>
        /// 停止正在运行的G代码程序。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_stop(ushort ConnectNo);

        /// <summary>
        /// 暂停正在运行的G代码程序。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_pause(ushort ConnectNo);

        /// <summary>
        /// 读取当前G代码程序的运行状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="state">返回当前G代码运行状态：1 - 运行中, 2 - 暂停中, 3 - 已停止, 4 - 异常。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_state(ushort ConnectNo, ref ushort state);

        /// <summary>
        /// 设置要运行的当前G代码文件。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="pFileName">控制器内的文件名。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_set_current_file(ushort ConnectNo, byte[] pFileName);

        /// <summary>
        /// 读取当前设置的G代码文件名。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-7。</param>
        /// <param name="pFileName">返回控制器文件名。</param>
        /// <param name="fileid">返回当前文件号。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_get_current_file(ushort ConnectNo, byte[] pFileName, ref ushort fileid);

        /// <summary>
        /// 读取G代码程序当前正在运行的行。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="line">返回行号。</param>
        /// <param name="pCurLine">返回该行的字符串内容。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_current_line(ushort ConnectNo, ref uint line, byte[] pCurLine);

        /// <summary>
        /// 读取G代码程序当前正在执行的行。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="line">行号。</param>
        /// <param name="pCurLine">行字符串。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_get_current_line(ushort ConnectNo, ref uint line, byte[] pCurLine);

        /// <summary>
        /// 检查指定ID的G代码文件信息。
        /// (注意：该函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="fileid">文件ID。</param>
        /// <param name="pFileName">文件名。</param>
        /// <param name="pFileSize">文件大小。</param>
        /// <param name="pTotalLine">总行数。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_check_file_id(ushort ConnectNo, ushort fileid, string pFileName, ulong[] pFileSize, ulong[] pTotalLine);

        /// <summary>
        /// 读取G代码文件的相关属性。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="maxfilenum">返回支持的最大文件数量。</param>
        /// <param name="maxfilesize">返回文件容量最大值。</param>
        /// <param name="savedfilenum">返回已保存的文件数量。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_gcode_get_file_profile(ushort ConnectNo, ref uint maxfilenum, ref uint maxfilesize, ref uint savedfilenum);

        /*********************************************************************************************************
        状态监控
        *********************************************************************************************************/
        /// <summary>
        /// 紧急停止控制器上的所有轴。
        /// 注意：此函数适用于所有运动模式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_emg_stop(ushort ConnectNo);

        /// <summary>
        /// 检测指定轴的运动是否完成。
        /// 注意：此函数适用于单轴运动和PVT运动。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">要检测的轴号。</param>
        /// <returns>0：指定轴正在运行；1：指定轴已停止。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_check_done(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 停止指定轴的运动。
        /// 注意：此函数适用于单轴运动和PVT运动。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">要停止的轴号。</param>
        /// <param name="stop_mode">制动方式：0 - 减速停止；1 - 立即停止。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_stop(ushort ConnectNo, ushort axis, ushort stop_mode);

        /// <summary>
        /// 检测指定坐标系内的运动是否完成。
        /// 注意：此函数适用于插补运动。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，范围：0-1。</param>
        /// <returns>坐标系状态：0 - 正在使用中；1 - 正常停止。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_check_done_multicoor(ushort ConnectNo, ushort Crd);

        /// <summary>
        /// 停止指定坐标系内所有轴的运动。
        /// 注意：此函数适用于插补运动。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，范围：0-1。</param>
        /// <param name="stop_mode">制动方式：0 - 减速停止；1 - 立即停止。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_stop_multicoor(ushort ConnectNo, ushort Crd, ushort stop_mode);

        /// <summary>
        /// 读取指定轴的运动相关IO信号的状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号。</param>
        /// <returns>返回一个32位无符号整数，表示IO状态的位掩码。
        /// Bit0-ALM, Bit1-EL+, Bit2-EL-, Bit3-EMG, Bit4-ORG, Bit6-SL+, Bit7-SL-。
        /// 值为1表示ON，值为0表示OFF。</returns>
        [DllImport("LTSMC.dll")]
        public static extern uint smc_axis_io_status(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 读取指定轴特殊IO信号的使能状态。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号。</param>
        /// <returns>返回一个位掩码，表示各特殊IO的使能状态。bit0表示禁用，bit1表示允许。</returns>
        [DllImport("LTSMC.dll")]
        public static extern uint smc_axis_io_enable_status(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 读取指定轴当前的运动模式。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="run_mode">返回运动模式：0-空闲, 1-Pmove, 2-Vmove, 3-Hmove, 4-Handwheel, 5-Ptt/Pts, 6-Pvt/Pvts, 7-Gear, 8-Cam, 9-Line, 10-Continue。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_axis_run_mode(ushort ConnectNo, ushort axis, ref ushort run_mode);

        /// <summary>
        /// 读取指定轴的当前速度。
        /// 注意：当执行插补运动时，此函数读取的是矢量速度。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="current_speed">返回当前速度值，单位：unit/s。正值表示正向，负值表示负向。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_current_speed_unit(ushort ConnectNo, ushort axis, ref double current_speed);

        /// <summary>
        /// 设置指定轴的当前指令位置计数器值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="pos">要设置的位置值，单位：unit。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_position_unit(ushort ConnectNo, ushort axis, double pos);

        /// <summary>
        /// 读取指定轴的当前指令位置计数器值。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="pos">返回当前位置值，单位：unit。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_position_unit(ushort ConnectNo, ushort axis, ref double pos);

        /// <summary>
        /// 读取指定轴的当前运动目标位置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="pos">返回目标位置值，单位：unit。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_target_position_unit(ushort ConnectNo, ushort axis, ref double pos);

        /// <summary>
        /// 设置当前位置为工件坐标系原点。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="pos">要设置的工件原点位置值，单位：unit。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_workpos_unit(ushort ConnectNo, ushort axis, double pos);

        /// <summary>
        /// 读取当前工件坐标系原点的位置。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="pos">返回工件原点位置值，单位：unit。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_workpos_unit(ushort ConnectNo, ushort axis, ref double pos);

        /// <summary>
        /// 读取指定轴的最后一次停止原因。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="StopReason">返回停止原因代码。例如：0-正常停止, 1-ALM立即停止, 4-EMG立即停止, 5-正硬限位立即停止等。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_stop_reason(ushort ConnectNo, ushort axis, ref int StopReason);

        /// <summary>
        /// 清除指定轴的停止原因标志。
        /// </summary>
        /// <param name="ConnectNo">链接号，范围：0-254。</param>
        /// <param name="axis">轴号。</param>
        /// <returns>错误代码，0表示成功。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_clear_stop_reason(ushort ConnectNo, ushort axis);

        /**************************************************************************************************************************
        数据采集
        (注意：以下数据采集相关函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        ***************************************************************************************************************************/
        /// <summary>
        /// 设置追踪源。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="source">源。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_trace_set_source(ushort ConnectNo, ushort source);

        /// <summary>
        /// 读取追踪数据。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="bufsize">缓冲区大小。</param>
        /// <param name="time">返回时间数据。</param>
        /// <param name="pos">返回位置数据。</param>
        /// <param name="vel">返回速度数据。</param>
        /// <param name="acc">返回加速度数据。</param>
        /// <param name="recv_num">返回接收到的数量。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_read_trace_data(ushort ConnectNo, ushort axis, int bufsize, double[] time, double[] pos, double[] vel, double[] acc, ref int recv_num);

        /// <summary>
        /// 开始追踪。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="AxisNum">轴数量。</param>
        /// <param name="AxisList">轴列表。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_trace_start(ushort ConnectNo, ushort AxisNum, ushort[] AxisList);

        /// <summary>
        /// 停止追踪。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_trace_stop(ushort ConnectNo);

        //TRACE数据采集新规划

        /*********************************************************************************************************
        总线专用函数
        (注意：以下总线相关函数在所提供的API文档中未详细说明，请参考最新版文档以获取确切信息。)
        *********************************************************************************************************/
        /*************************************** EtherCAT & CANopen *****************************/
        //从站对象字典
        /// <summary>
        /// 设置总线从站节点的对象字典。
        /// </summary>
        /// <param name="ConnectNo">链接号。</param>
        /// <param name="PortNum">端口号。</param>
        /// <param name="nodenum">节点号。</param>
        /// <param name="index">对象字典索引。</param>
        /// <param name="subindex">对象字典子索引。</param>
        /// <param name="valuelength">值长度（字节）。</param>
        /// <param name="value">要写入的值。</param>
        /// <returns>错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_node_od(ushort ConnectNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort valuelength, uint value);

        /// <summary>
        /// 读取总线节点对象字典。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="nodenum">节点号。</param>
        /// <param name="index">对象字典索引。</param>
        /// <param name="subindex">对象字典子索引。</param>
        /// <param name="valuelength">读取的值长度，单位为字节。</param>
        /// <param name="value">返回读取到的对象字典的值。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_node_od(ushort ConnectNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort valuelength, ref uint value);

        /// <summary>
        /// 按浮点数设置总线节点对象字典的值。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="nodenum">节点号。</param>
        /// <param name="index">对象字典索引。</param>
        /// <param name="subindex">对象字典子索引。</param>
        /// <param name="valuelength">写入的值长度，单位为字节。</param>
        /// <param name="value">要写入的浮点数值。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_node_od_float(ushort ConnectNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort valuelength, float value);

        /// <summary>
        /// 按浮点数读取总线节点对象字典的值。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="nodenum">节点号。</param>
        /// <param name="index">对象字典索引。</param>
        /// <param name="subindex">对象字典子索引。</param>
        /// <param name="valuelength">读取的值长度，单位为字节。</param>
        /// <param name="value">返回读取到的浮点数值。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_node_od_float(ushort ConnectNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort valuelength, ref float value);

        /// <summary>
        /// 按字节流（byte数组）设置总线节点对象字典的值。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="nodenum">节点号。</param>
        /// <param name="index">对象字典索引。</param>
        /// <param name="subindex">对象字典子索引。</param>
        /// <param name="bytes">要写入的字节数。</param>
        /// <param name="value">要写入的字节数组。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_node_od_pbyte(ushort ConnectNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort bytes, byte[] value);

        /// <summary>
        /// 按字节流（byte数组）读取总线节点对象字典的值。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="nodenum">节点号。</param>
        /// <param name="index">对象字典索引。</param>
        /// <param name="subindex">对象字典子索引。</param>
        /// <param name="bytes">期望读取的字节数。</param>
        /// <param name="value">返回读取到的字节数组。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_node_od_pbyte(ushort ConnectNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort bytes, byte[] value);

        /*************************************** EtherCAT & RTEX *****************************/
        /// <summary>
        /// 使能指定总线轴，使其进入伺服ON状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_axis_enable(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 禁能指定总线轴，使其进入伺服OFF状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_axis_disable(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 获取指定总线轴的数字量输出（DO）状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <returns>返回IO状态的整数表示，按位对应。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_axis_io_out(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 设置指定总线轴的数字量输出（DO）状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="iostate">要设置的IO状态，按位组合的整数值。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_axis_io_out(ushort ConnectNo, ushort axis, int iostate);

        /// <summary>
        /// 获取指定总线轴的数字量输入（DI）状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <returns>返回IO状态的整数表示，按位对应。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_axis_io_in(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 设置总线通讯周期。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNo">总线端口号。</param>
        /// <param name="CycleTime">周期时间，单位通常为微秒(us)。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_cycletime(ushort ConnectNo, ushort PortNo, int CycleTime);

        /// <summary>
        /// 获取总线通讯周期。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNo">总线端口号。</param>
        /// <param name="CycleTime">返回周期时间，单位通常为微秒(us)。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_cycletime(ushort ConnectNo, ushort PortNo, ref int CycleTime);

        /// <summary>
        /// 设置总线轴的位置偏移值。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="offset_pos">要设置的位置偏移量。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_offset_pos(ushort ConnectNo, ushort axis, double offset_pos);

        /// <summary>
        /// 获取总线轴的位置偏移值。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="offset_pos">返回当前的位置偏移量。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_offset_pos(ushort ConnectNo, ushort axis, ref double offset_pos);

        /*************************************** EtherCAT & CANopen  & RTEX *********************/
        /// <summary>
        /// 获取指定轴的类型（如：伺服、步进等）。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="Axis_Type">返回轴类型的代码。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_axis_type(ushort ConnectNo, ushort axis, ref ushort Axis_Type);

        /// <summary>
        /// 读取指定总线轴有关运动信号的综合状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <returns>
        /// 返回一个整数，表示轴的运动信号状态（位域）。
        /// 位 0 (ALM): 1 表示伺服报警信号为 ON。
        /// 位 1 (EL+): 1 表示正向硬限位信号为 ON。
        /// 位 2 (EL-): 1 表示负向硬限位信号为 ON。
        /// 位 3 (EMG): 1 表示急停信号为 ON。
        /// 位 4 (ORG): 1 表示原点信号为 ON。
        /// 位 6 (SL+): 1 表示正向软限位信号为 ON。
        /// 位 7 (SL-): 1 表示负向软限位信号为 ON。
        /// 0 表示对应信号为 OFF。
        /// </returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_axis_io_status(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 获取控制器卡级别的错误码。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Errcode">返回错误码。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_card_errcode(ushort ConnectNo, ref int Errcode);

        /// <summary>
        /// 清除控制器卡级别的错误码。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_clear_card_errcode(ushort ConnectNo);

        /// <summary>
        /// 获取指定总线端口的错误码。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="errcode">返回错误码。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_errcode(ushort ConnectNo, ushort PortNum, ref int errcode);

        /// <summary>
        /// 清除指定总线端口的错误码。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_clear_errcode(ushort ConnectNo, ushort PortNum);

        /// <summary>
        /// 获取指定总线轴的错误码。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="errcode">返回错误码。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_axis_errcode(ushort ConnectNo, ushort axis, ref int errcode);

        /// <summary>
        /// 获取控制器上配置的总轴数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="TotalAxis">返回总轴数。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_total_axes(ushort ConnectNo, ref uint TotalAxis);

        /// <summary>
        /// 获取控制器上配置的总数字量IO数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="TotalIn">返回总输入（DI）点数。</param>
        /// <param name="TotalOut">返回总输出（DO）点数。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_total_ionum(ushort ConnectNo, ref ushort TotalIn, ref ushort TotalOut);

        /// <summary>
        /// 按节点ID读取指定扩展IO模块的单个输入位（DI）的状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Channel">总线端口号。</param>
        /// <param name="NoteID">IO模块的节点ID。</param>
        /// <param name="IoBit">要读取的输入点位号。</param>
        /// <param name="IoValue">返回IO点的状态：0 - 低电平，1 - 高电平。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_inbit_extern(ushort ConnectNo, ushort Channel, ushort NoteID, ushort IoBit, ref ushort IoValue);

        /// <summary>
        /// 按节点ID读取指定扩展IO模块的一组输入端口（DI）的状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Channel">总线端口号。</param>
        /// <param name="NoteID">IO模块的节点ID。</param>
        /// <param name="PortNo">要读取的端口组号。</param>
        /// <param name="IoValue">返回端口组的状态，按位组合的整数值。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_inport_extern(ushort ConnectNo, ushort Channel, ushort NoteID, ushort PortNo, ref int IoValue);

        /// <summary>
        /// 按节点ID设置指定扩展IO模块的单个输出位（DO）的状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNo">总线端口号。</param>
        /// <param name="NodeID">IO模块的节点ID。</param>
        /// <param name="BitNo">要设置的输出点位号。</param>
        /// <param name="IoValue">要设置的IO状态：0 - 低电平，1 - 高电平。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_write_outbit_extern(ushort ConnectNo, ushort PortNo, ushort NodeID, ushort BitNo, ushort IoValue);

        /// <summary>
        /// 按节点ID设置指定扩展IO模块的一组输出端口（DO）的状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="NodeID">IO模块的节点ID。</param>
        /// <param name="PortNo">要设置的端口组号。</param>
        /// <param name="IoValue">要设置的端口组状态，按位组合的整数值。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_write_outport_extern(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort PortNo, uint IoValue);

        /// <summary>
        /// 按节点ID读取指定扩展IO模块的单个输出位（DO）的当前状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNo">总线端口号。</param>
        /// <param name="NodeID">IO模块的节点ID。</param>
        /// <param name="BitNo">要读取的输出点位号。</param>
        /// <param name="IoValue">返回IO点的状态：0 - 低电平，1 - 高电平。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_outbit_extern(ushort ConnectNo, ushort PortNo, ushort NodeID, ushort BitNo, ref ushort IoValue);

        /// <summary>
        /// 按节点ID读取指定扩展IO模块的一组输出端口（DO）的当前状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="NodeID">IO模块的节点ID。</param>
        /// <param name="PortNo">要读取的端口组号。</param>
        /// <param name="IoValue">返回端口组的状态，按位组合的整数值。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_outport_extern(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort PortNo, ref int IoValue);

        /// <summary>
        /// 设置总线复位时，从站输出端口是否保持状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Enable">使能参数：0 - 不保持；1 - 保持。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_slave_output_retain(ushort ConnectNo, ushort Enable);

        /// <summary>
        /// 获取总线复位时，从站输出端口是否保持状态的设置。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Enable">返回使能状态：0 - 不保持；1 - 保持。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_slave_output_retain(ushort ConnectNo, ref ushort Enable);

        /*************************************** CANopen **************************************/
        /// <summary>
        /// 复位CANopen总线。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_reset_canopen(ushort ConnectNo);

        /// <summary>
        /// 获取发生心跳报文丢失事件的节点列表。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="NodeID">返回丢失心跳的节点ID数组。</param>
        /// <param name="NodeNum">返回丢失心跳的节点数量。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_LostHeartbeat_Nodes(ushort ConnectNo, ushort PortNum, ushort[] NodeID, ref ushort NodeNum);

        /// <summary>
        /// 获取接收到的紧急报文信息。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="NodeMsg">返回紧急报文的数组。</param>
        /// <param name="MsgNum">返回紧急报文的数量。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_EmergeneyMessege_Nodes(ushort ConnectNo, ushort PortNum, uint[] NodeMsg, ref ushort MsgNum);

        /// <summary>
        /// 向指定CANopen节点发送NMT（网络管理）命令。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="NodeID">目标节点ID。若为0，则广播至所有节点。</param>
        /// <param name="NmtCommand">NMT命令码，如启动、停止、复位等。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_SendNmtCommand(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort NmtCommand);

        /// <summary>
        /// 清除指定总线节点的报警状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="nodenum">节点号。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_alarm_clear(ushort ConnectNo, ushort PortNum, ushort nodenum);

        /// <summary>
        /// 启动多轴同步点位运动。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="AxisNum">参与同步运动的轴数量。</param>
        /// <param name="AxisList">参与同步运动的轴号列表数组。</param>
        /// <param name="Position">各轴的目标位置数组。</param>
        /// <param name="PosiMode">
        /// 各轴的运动模式数组。
        /// 0: 相对坐标模式。
        /// 1: 绝对坐标模式。
        /// </param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_syn_move_unit(ushort ConnectNo, ushort AxisNum, ushort[] AxisList, double[] Position, ushort[] PosiMode);

        /// <summary>
        /// 获取控制器上配置的总模拟量IO数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="TotalIn">返回总模拟量输入（AI）点数。</param>
        /// <param name="TotalOut">返回总模拟量输出（AO）点数。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_total_adcnum(ushort ConnectNo, ref ushort TotalIn, ref ushort TotalOut);

        /// <summary>
        /// 设置EtherCAT轴的硬限位（EL）停止模式。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="el_control_mode">限位控制模式。</param>
        /// <param name="diff_pos">差分位置，可能用于反向运动距离。</param>
        /// <param name="filter">滤波时间。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_etc_el_stop_mode(ushort ConnectNo, ushort axis, ushort el_control_mode, double diff_pos, int filter);

        /*************************************** EtherCAT **************************************/
        /// <summary>
        /// 复位EtherCAT总线。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_reset_etc(ushort ConnectNo);

        /// <summary>
        /// 停止EtherCAT协议栈。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="ETCState">用于返回EtherCAT状态的数组。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_stop_etc(ushort ConnectNo, ushort[] ETCState);

        /// <summary>
        /// 读取EtherCAT总线轴的状态机当前状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="Axis_StateMachine">返回轴的状态机代码。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_axis_state_machine(ushort ConnectNo, ushort axis, ref ushort Axis_StateMachine);

        /// <summary>
        /// 根据轴号获取其在EtherCAT总线上的节点地址。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="SlaveAddr">返回从站的物理地址。</param>
        /// <param name="Sub_SlaveAddr">返回子从站地址（如有）。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_axis_node_address(ushort ConnectNo, ushort axis, ref ushort SlaveAddr, ref ushort Sub_SlaveAddr);

        /// <summary>
        /// 写入RxPDO（接收过程数据对象）的扩展数据。
        /// </summary>
        /// <param name="CardNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="address">从站地址。</param>
        /// <param name="DataLen">数据长度，单位为字节。</param>
        /// <param name="Value">要写入的32位整数值。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_write_rxpdo_extra(ushort CardNo, ushort PortNum, ushort address, ushort DataLen, Int32 Value);

        /// <summary>
        /// 读取RxPDO（接收过程数据对象）的扩展数据。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNo">总线端口号。</param>
        /// <param name="address">从站地址。</param>
        /// <param name="DataLen">数据长度，单位为字节。</param>
        /// <param name="Value">返回读取到的32位整数值。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_rxpdo_extra(ushort ConnectNo, ushort PortNo, ushort address, ushort DataLen, ref int Value);

        /// <summary>
        /// 读取TxPDO（发送过程数据对象）的扩展数据。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNo">总线端口号。</param>
        /// <param name="address">从站地址。</param>
        /// <param name="DataLen">数据长度，单位为字节。</param>
        /// <param name="Value">返回读取到的32位整数值。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_txpdo_extra(ushort ConnectNo, ushort PortNo, ushort address, ushort DataLen, ref int Value);

        /// <summary>
        /// 启动指定轴的转矩控制模式运动。
        /// </summary>
        /// <param name="CardNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="Torque">目标转矩值，通常为额定转矩的千分比。</param>
        /// <param name="PosLimitValid">位置限制使能：0 - 不限制；1 - 限制。</param>
        /// <param name="PosLimitValue">位置限制值。</param>
        /// <param name="PosMode">位置限制值的坐标模式：0 - 相对坐标；1 - 绝对坐标。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_torque_move(ushort CardNo, ushort axis, int Torque, ushort PosLimitValid, double PosLimitValue, ushort PosMode);

        /// <summary>
        /// 在转矩控制模式下，在线改变目标转矩。
        /// </summary>
        /// <param name="CardNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="Torque">新的目标转矩值。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_change_torque(ushort CardNo, ushort axis, int Torque);

        /// <summary>
        /// 读取指定轴的当前转矩值。
        /// </summary>
        /// <param name="CardNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="Torque">返回当前转矩值。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_torque(ushort CardNo, ushort axis, ref int Torque);

        /// <summary>
        /// 使指定轴进入PDO缓存运动模式。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_enter(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 停止指定轴的PDO缓存运动模式。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_stop(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 清除指定轴的PDO缓存区中的所有数据。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_clear(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 获取PDO缓存运动的状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="RunState">返回运行状态。</param>
        /// <param name="Remain">返回缓存区剩余空间。</param>
        /// <param name="NotRunned">返回未运行的数据点数。</param>
        /// <param name="Runned">返回已运行的数据点数。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_run_state(ushort ConnectNo, ushort axis, ref int RunState, ref int Remain, ref int NotRunned, ref int Runned);

        /// <summary>
        /// 向指定轴的PDO缓存区添加运动数据。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="size">要添加的数据点数量。</param>
        /// <param name="data_table">包含运动数据点的数组。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_add_data(ushort ConnectNo, ushort axis, int size, int[] data_table);

        /// <summary>
        /// 启动多个轴的PDO缓存运动。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="AxisNum">参与运动的轴数量。</param>
        /// <param name="AxisList">参与运动的轴号列表数组。</param>
        /// <param name="ResultList">返回各轴的执行结果数组。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_start_multi(ushort ConnectNo, ushort AxisNum, ushort[] AxisList, ushort[] ResultList);

        /// <summary>
        /// 暂停多个轴的PDO缓存运动。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="AxisNum">参与运动的轴数量。</param>
        /// <param name="AxisList">参与运动的轴号列表数组。</param>
        /// <param name="ResultList">返回各轴的执行结果数组。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_pause_multi(ushort ConnectNo, ushort AxisNum, ushort[] AxisList, ushort[] ResultList);

        /// <summary>
        /// 停止多个轴的PDO缓存运动。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="AxisNum">参与运动的轴数量。</param>
        /// <param name="AxisList">参与运动的轴号列表数组。</param>
        /// <param name="ResultList">返回各轴的执行结果数组。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_pdo_buffer_stop_multi(ushort ConnectNo, ushort AxisNum, ushort[] AxisList, ushort[] ResultList);

        /// <summary>
        /// 获取当前现场总线的状态信息。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Channel">总线端口号。</param>
        /// <param name="Axis">返回关联的轴号数组。</param>
        /// <param name="ErrorType">返回错误类型数组。</param>
        /// <param name="SlaveAddr">返回发生错误的从站地址数组。</param>
        /// <param name="ErrorFieldbusCode">返回现场总线的具体错误码数组。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_current_fieldbus_state_info(ushort ConnectNo, ushort Channel, ushort[] Axis, ushort[] ErrorType, ushort[] SlaveAddr, int[] ErrorFieldbusCode);

        /// <summary>
        /// 获取详细的现场总线状态信息。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Channel">总线端口号。</param>
        /// <param name="ReadErrorNum">要读取的错误记录数量。</param>
        /// <param name="TotalNum">返回错误记录总数。</param>
        /// <param name="ActualNum">返回实际读取到的错误记录数。</param>
        /// <param name="Axis">返回关联的轴号。</param>
        /// <param name="ErrorType">返回错误类型。</param>
        /// <param name="SlaveAddr">返回发生错误的从站地址。</param>
        /// <param name="ErrorFieldbusCode">返回现场总线的具体错误码。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_detail_fieldbus_state_info(ushort ConnectNo, ushort Channel, ushort ReadErrorNum, ref ushort TotalNum, ref int ActualNum, ref ushort Axis, ref ushort ErrorType, ref ushort SlaveAddr, ref int ErrorFieldbusCode);

        /// <summary>
        /// 设置总线错误检查次数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="channel">总线端口号。</param>
        /// <param name="checktimes">设置的检查次数。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_error_checktimes(ushort ConnectNo, ushort channel, ushort checktimes);

        /// <summary>
        /// 获取总线错误检查次数的设置。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="channel">总线端口号。</param>
        /// <param name="checktimes">返回当前设置的检查次数。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_error_checktimes(ushort ConnectNo, ushort channel, ref int checktimes);

        /// <summary>
        /// 加载用户自定义的功能库文件到控制器。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="pLibname">库文件名。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_userlib_loadlibrary(ushort ConnectNo, byte[] pLibname);

        /// <summary>
        /// 设置用户库的自定义参数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="type">参数类型标识。</param>
        /// <param name="pParameter">参数数据缓冲区。</param>
        /// <param name="length">参数数据长度。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_userlib_set_parameter(ushort ConnectNo, int type, byte[] pParameter, int length);

        /// <summary>
        /// 获取用户库的自定义参数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="type">参数类型标识。</param>
        /// <param name="pParameter">用于接收参数数据的缓冲区。</param>
        /// <param name="length">缓冲区长度。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_userlib_get_parameter(ushort ConnectNo, int type, byte[] pParameter, int length);

        /// <summary>
        /// 立即停止由用户库控制的轴运动。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_userlib_imd_stop(ushort ConnectNo, ushort axis);

        /// <summary>
        /// 设置或取消轴的位置范围限制（环形位置/模数轴功能）。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="enable">使能参数：0 - 禁止；1 - 允许。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_position_range_limit(ushort ConnectNo, ushort Axis, ushort enable);

        /// <summary>
        /// 获取轴的位置范围限制（环形位置/模数轴功能）的设置状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="enable">返回使能状态：0 - 禁止；1 - 允许。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_position_range_limit(ushort ConnectNo, ushort Axis, ref ushort enable);

        /// <summary>
        /// 设置看门狗超时后触发的动作事件。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="event_mask">事件掩码，定义超时后执行的动作。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_watchdog_action_event(ushort ConnectNo, uint event_mask);

        /// <summary>
        /// 获取看门狗超时后触发的动作事件设置。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="event_mask">返回当前设置的事件掩码。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_watchdog_action_event(ushort ConnectNo, ref uint event_mask);

        /// <summary>
        /// 设置看门狗的超时时间和使能状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="timer_period">超时时间，单位为秒。</param>
        /// <param name="enable">使能参数：0 - 禁止；1 - 允许。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_watchdog_enable(ushort ConnectNo, double timer_period, uint enable);

        /// <summary>
        /// 获取看门狗的超时时间和使能状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="timer_period">返回当前设置的超时时间。</param>
        /// <param name="enable">返回当前使能状态：0 - 禁止；1 - 允许。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_watchdog_enable(ushort ConnectNo, ref double timer_period, ref uint enable);

        /// <summary>
        /// 复位（喂狗）看门狗定时器，防止其超时。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_reset_watchdog_timer(ushort ConnectNo);

        /// <summary>
        /// (扩展)设置指定索引的看门狗超时后触发的动作事件。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="index">看门狗索引号。</param>
        /// <param name="event_mask">事件掩码，定义超时后执行的动作。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_watchdog_action_event_extern(ushort ConnectNo, ushort index, ushort event_mask);

        /// <summary>
        /// (扩展)获取指定索引的看门狗超时后触发的动作事件设置。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="index">看门狗索引号。</param>
        /// <param name="event_mask">返回当前设置的事件掩码。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_watchdog_action_event_extern(ushort ConnectNo, ushort index, ref ushort event_mask);

        /// <summary>
        /// (扩展)设置指定索引的看门狗的超时时间和使能状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="index">看门狗索引号。</param>
        /// <param name="timer_period">超时时间，单位为秒。</param>
        /// <param name="enable">使能参数：0 - 禁止；1 - 允许。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_watchdog_enable_extern(ushort ConnectNo, ushort index, double timer_period, ushort enable);

        /// <summary>
        /// (扩展)获取指定索引的看门狗的超时时间和使能状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="index">看门狗索引号。</param>
        /// <param name="timer_period">返回当前设置的超时时间。</param>
        /// <param name="enable">返回当前使能状态：0 - 禁止；1 - 允许。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_watchdog_enable_extern(ushort ConnectNo, ushort index, double timer_period, ref ushort enable);

        /// <summary>
        /// (扩展)复位（喂狗）指定索引的看门狗定时器。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="index">看门狗索引号。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_reset_watchdog_timer_extern(ushort ConnectNo, ushort index);

        /// <summary>
        /// 在连续插补运动过程中，强制将指定轴的当前位置设定为新值。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，范围 0-1。</param>
        /// <param name="axis_num">要设置位置的轴数量。</param>
        /// <param name="axis_list">要设置位置的轴号列表数组。</param>
        /// <param name="position">各轴的新位置值数组。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_force_set_position(ushort ConnectNo, ushort Crd, ushort axis_num, ushort[] axis_list, double[] position);

        /// <summary>
        /// 设置模数（modulo）运动的相关参数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="enable">使能参数：0 - 禁止；1 - 允许。</param>
        /// <param name="Modulo_Vel">模数速度。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_modulo_profile(ushort ConnectNo, ushort axis, ushort enable, double Modulo_Vel);

        /// <summary>
        /// 获取模数（modulo）运动的相关参数设置。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="enable">返回使能状态：0 - 禁止；1 - 允许。</param>
        /// <param name="Modulo_Vel">返回当前设置的模数速度。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_modulo_profile(ushort ConnectNo, ushort axis, ref ushort enable, ref double Modulo_Vel);

        /// <summary>
        /// 设置总线数据偏移时间，用于同步调整。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="offset_us">偏移时间，单位为微秒(us)。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_data_offset_time(ushort ConnectNo, int offset_us);

        /// <summary>
        /// 在连续插补模式下，设置锐角减速的配置参数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，范围 0-1。</param>
        /// <param name="acuate_angle">定义为锐角的角度阈值，单位为度。</param>
        /// <param name="angle_trans_speed">经过锐角时的过渡速度。</param>
        /// <param name="enable">使能参数：0 - 禁止锐角减速；1 - 启用锐角减速。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_acuate_angle_config_params(ushort ConnectNo, ushort Crd, double acuate_angle, double angle_trans_speed, ushort enable);

        /// <summary>
        /// 获取连续插补模式下锐角减速的配置参数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，范围 0-1。</param>
        /// <param name="acuate_angle">返回锐角角度阈值。</param>
        /// <param name="angle_trans_speed">返回角度转换速度。</param>
        /// <param name="enable">返回使能状态：0 - 禁止；1 - 启用。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_acuate_angle_config_params(ushort ConnectNo, ushort Crd, ref double acuate_angle, ref double angle_trans_speed, ref ushort enable);

        /// <summary>
        /// 以特定紧凑格式（dxXYZ）向连续插补缓冲区批量添加直线运动指令。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，范围 0-1。</param>
        /// <param name="buff">包含紧凑格式指令的字节数组缓冲区。</param>
        /// <param name="pack_num">要添加的指令包数量。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_dxXYZLineBuff(ushort ConnectNo, ushort Crd, byte[] buff, ushort pack_num);

        /// <summary>
        /// 以特定紧凑格式（dfXYZ）向连续插补缓冲区批量添加直线运动指令。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，范围 0-1。</param>
        /// <param name="buff">包含紧凑格式指令的字节数组缓冲区。</param>
        /// <param name="pack_num">要添加的指令包数量。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_dfXYZLineBuff(ushort ConnectNo, ushort Crd, byte[] buff, ushort pack_num);

        /// <summary>
        /// 删除控制器存储器中的指定类型的文件。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="FileType">
        /// 文件类型：
        /// 0 - Basic文件。
        /// 1 - G代码文件。
        /// 2 - 参数文件。
        /// 3 - 固件。
        /// 200 - eni文件。
        /// 201 - ini文件。
        /// </param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_delete_file(ushort ConnectNo, ushort FileType);

        /// <summary>
        /// EtherCAT总线功能：读取指定从站的寄存器。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="wSlaveAddress">从站的物理地址。</param>
        /// <param name="wRegisterOffset">要读取的寄存器偏移地址。</param>
        /// <param name="wLen">读取的数据长度，单位为字节。</param>
        /// <param name="pdwData">用于接收读取数据的字节数组。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_ecat_read_slave_register(ushort ConnectNo, ushort wSlaveAddress, ushort wRegisterOffset, ushort wLen, byte[] pdwData);

        /// <summary>
        /// EtherCAT总线功能：向指定从站的寄存器写入数据。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="wSlaveAddress">从站的物理地址。</param>
        /// <param name="wRegisterOffset">要写入的寄存器偏移地址。</param>
        /// <param name="wLen">写入的数据长度，单位为字节。</param>
        /// <param name="pdwData">包含要写入数据的字节数组。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_ecat_write_slave_register(ushort ConnectNo, ushort wSlaveAddress, ushort wRegisterOffset, ushort wLen, byte[] pdwData);

        /// <summary>
        /// 启动循环计数功能。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Channel">计数通道号。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_cir_count_start(ushort ConnectNo, ushort Channel);

        /// <summary>
        /// 获取循环计数的状态标志。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Channel">计数通道号。</param>
        /// <param name="Flag">返回状态标志。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_cir_count_flag(ushort ConnectNo, ushort Channel, ref ushort Flag);

        /// <summary>
        /// 复位指定通道的循环计数值。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Channel">计数通道号。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_cir_count_reset(ushort ConnectNo, ushort Channel);

        /// <summary>
        /// 获取指定通道的当前循环计数值。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Channel">计数通道号。</param>
        /// <param name="Value">返回当前计数值。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_cir_count_value(ushort ConnectNo, ushort Channel, ref ushort Value);

        /// <summary>
        /// (扩展)设置由多个IO信号组合触发的精确停止功能。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">作用的轴号。</param>
        /// <param name="ioNum">参与触发的IO数量。</param>
        /// <param name="ioList">IO点号列表数组。</param>
        /// <param name="ioTypeList">IO类型列表数组。</param>
        /// <param name="ioLogicList">IO有效电平逻辑列表数组。</param>
        /// <param name="enable">使能参数：0 - 禁止；1 - 允许。</param>
        /// <param name="action">触发后的动作：0 - 减速停止；1 - 立即停止。</param>
        /// <param name="move_dir">生效的运动方向。</param>
        /// <returns>返回错误代码，0表示执行成功，非0表示失败。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_io_exactstop_ex(ushort ConnectNo, ushort axis, ushort ioNum, ushort[] ioList, ushort[] ioTypeList, ushort[] ioLogicList, ushort enable, ushort action, ushort move_dir);

        /// <summary>
        /// 设置总线轴的运行模式。此功能为总线型控制器专用。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="run_mode">
        /// 运行模式:
        /// 0 - 空闲
        /// 1 - Pmove (点位运动)
        /// 2 - Vmove (速度运动)
        /// 3 - Hmove (回原点运动)
        /// 4 - Handwheel (手轮运动)
        /// 5 - Ptt / Pts (单轴速度规划)
        /// 6 - Pvt / Pvts (多轴轨迹规划)
        /// 7 - Gear (电子齿轮)
        /// 8 - Cam (电子凸轮)
        /// 9 - Line (插补运动)
        /// 10 - Continue (连续插补)
        /// </param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于标准函数 smc_get_axis_run_mode (3.7节) 推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_axis_run_mode(ushort ConnectNo, ushort axis, ushort run_mode);

        /// <summary>
        /// 晶圆测量比较功能配置。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="enable">使能状态。0: 禁止, 1: 使能。</param>
        /// <param name="source">比较源。具体值待查，通常 0: 指令位置, 1: 编码器位置。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_wafer_meas_compare_config(ushort ConnectNo, ushort axis, ushort enable, ushort source);

        /// <summary>
        /// 清除所有已设置的晶圆测量比较点。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_wafer_meas_clear_point(ushort ConnectNo);

        /// <summary>
        /// 添加一组晶圆测量比较点。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="start_pos">第一个测量点的绝对位置。</param>
        /// <param name="interval">每个测量点之间的间隔距离。</param>
        /// <param name="count">要添加的测量点总数。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_wafer_meas_add_point(ushort ConnectNo, double start_pos, double interval, ushort count);

        /// <summary>
        /// 获取一个晶圆测量结果。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="pos">返回触发测量的位置。</param>
        /// <param name="val">返回测量传感器的值。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_wafer_meas_get_value(ushort ConnectNo, ref double pos, ref ushort val);

        //20240717新增
        /// <summary>
        /// 打开一个多轴指令列表缓冲区。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="group">多轴指令组号。</param>
        /// <param name="axis_num">参与该指令组的轴数量。</param>
        /// <param name="axis_list">参与该指令组的轴号列表（数组）。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_m_open_list(ushort ConnectNo, ushort group, ushort axis_num, ref ushort axis_list);

        /// <summary>
        /// 启动执行指定的多轴指令列表。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="group">多轴指令组号。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_m_start_list(ushort ConnectNo, ushort group);

        /// <summary>
        /// 关闭一个多轴指令列表缓冲区，并释放资源。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="group">多轴指令组号。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_m_close_list(ushort ConnectNo, ushort group);

        /// <summary>
        /// 获取多轴指令列表的运行状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="group">多轴指令组号。</param>
        /// <param name="state">返回当前运行状态。</param>
        /// <param name="enable">返回使能状态。</param>
        /// <param name="stop_reason">返回停止原因代码。</param>
        /// <param name="trig_phase">返回触发阶段。</param>
        /// <param name="mark">返回当前或最近执行的指令标记。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_m_get_run_state(ushort ConnectNo, ushort group, ref ushort state, ref ushort enable, ref int stop_reason, ref ushort trig_phase, ref int mark);

        /// <summary>
        /// 停止正在执行的多轴指令列表。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="group">多轴指令组号。</param>
        /// <param name="stopMode">停止模式。0: 减速停止, 1: 立即停止。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_m_stop_list(ushort ConnectNo, ushort group, ushort stopMode);

        /// <summary>
        /// 暂停正在执行的多轴指令列表。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="group">多轴指令组号。</param>
        /// <param name="stop_mode">停止模式，通常为 0 (减速暂停)。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_m_pause_list(ushort ConnectNo, ushort group, ushort stop_mode);

        /// <summary>
        /// 在多轴指令列表中添加一个延时指令。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="group">多轴指令组号。</param>
        /// <param name="Time_delay">延迟时间，单位：秒。</param>
        /// <param name="mark">用户自定义的指令标记，用于追踪执行进度。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_m_add_time_delay(ushort ConnectNo, ushort group, double Time_delay, int mark);

        /// <summary>
        /// 在多轴指令列表中添加一个单轴运动段数据。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="group">多轴指令组号。</param>
        /// <param name="Axis">要运动的轴号。</param>
        /// <param name="Target_pos">目标位置，单位：unit。</param>
        /// <param name="mark">用户自定义的指令标记。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_m_add_sigaxis_moveseg_data_ex(ushort ConnectNo, ushort group, ushort Axis, double Target_pos, int mark);

        /// <summary>
        /// 在多轴指令列表中添加一个等待事件指令。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="group">多轴指令组号。</param>
        /// <param name="evet">要等待的事件类型。</param>
        /// <param name="num">事件源的编号（如IO号、轴号）。</param>
        /// <param name="CompareOperator">比较操作符 (例如: 0:==, 1:!=, 2:>, 3:<, ...)。</param>
        /// <param name="target_value">要与事件源当前值进行比较的目标值。</param>
        /// <param name="mark">用户自定义的指令标记。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_m_add_wait_event_data(ushort ConnectNo, ushort group, ushort evet, ushort num, ushort CompareOperator, double target_value, int mark);

        /// <summary>
        /// 在多轴指令列表中添加一个触发操作指令。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="group">多轴指令组号。</param>
        /// <param name="mode">触发模式 (例如: 位置触发, 时间触发)。</param>
        /// <param name="num">触发源或目标的编号 (如轴号, IO号)。</param>
        /// <param name="Target_Value">触发的目标值 (如位置值)。</param>
        /// <param name="mark">用户自定义的指令标记。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_m_add_trigger_data(ushort ConnectNo, ushort group, ushort mode, ushort num, double Target_Value, int mark);

        /// <summary>
        /// 为多轴指令列表中的指定轴设置运动速度参数（时间模式）。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="group">多轴指令组号。</param>
        /// <param name="axis">要设置的轴号。</param>
        /// <param name="start_vel">起始速度，单位：unit/s。</param>
        /// <param name="max_vel">最大运行速度，单位：unit/s。</param>
        /// <param name="tacc">加速时间，单位：s。</param>
        /// <param name="tdec">减速时间，单位：s。</param>
        /// <param name="stop_vel">停止速度，单位：unit/s。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名和 smc_set_profile_unit 推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_m_set_profile_unit(ushort ConnectNo, ushort group, ushort axis, double start_vel, double max_vel, double tacc, double tdec, double stop_vel);

        /// <summary>
        /// 设置总线同步位置改变模式。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="portno">总线端口号。</param>
        /// <param name="axis">轴号。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_sync_pos_change_mode(ushort ConnectNo, ushort portno, ushort axis);

        //批量传输
        /// <summary>
        /// 开启连续插补指令的打包传输模式。在此模式下，插补指令会先缓存，而不是立即发送。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_pack_on(ushort ConnectNo);

        /// <summary>
        /// 关闭连续插补指令的打包传输模式，并可能刷新（发送）已缓存的指令。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_pack_off(ushort ConnectNo);

        /// <summary>
        /// 在打包传输模式下，强制发送（刷新）所有已缓存的连续插补指令。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_conti_pack_flush(ushort ConnectNo);

        /// <summary>
        /// 获取总线主站的状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="States">返回状态信息的数组。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_master_state(ushort ConnectNo, UInt32[] States);

        /// <summary>
        /// 设置间隙比较的空间距离。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="crd">坐标系号，范围：0-1。</param>
        /// <param name="space">间隙空间距离。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_gap_cmp_space(ushort ConnectNo, ushort crd, double space);

        /// <summary>
        /// 获取间隙比较的空间距离设置。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="crd">坐标系号，范围：0-1。</param>
        /// <param name="space">返回当前设置的间隙空间距离。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_gap_cmp_space(ushort ConnectNo, ushort crd, ref double space);

        /// <summary>
        /// 设置总线停止IO的虚拟映射。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="VirtualBitNo">虚拟IO位号。</param>
        /// <param name="MapIoType">被映射的IO类型。</param>
        /// <param name="MapIoIndex">被映射的IO索引号。</param>
        /// <param name="MapIoLogic">映射逻辑 (0:低有效, 1:高有效)。</param>
        /// <param name="MapIoFilter">滤波时间。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_stop_io_map_virtual(ushort ConnectNo, ushort VirtualBitNo, ushort MapIoType, ushort MapIoIndex, ushort MapIoLogic, ushort MapIoFilter);

        /// <summary>
        /// 获取总线停止IO的虚拟映射配置。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="VirtualBitNo">虚拟IO位号。</param>
        /// <param name="MapIoType">返回被映射的IO类型。</param>
        /// <param name="MapIoIndex">返回被映射的IO索引号。</param>
        /// <param name="MapIoLogic">返回映射逻辑。</param>
        /// <param name="MapIoFilter">返回滤波时间。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_stop_io_map_virtual(ushort ConnectNo, ushort VirtualBitNo, ref ushort MapIoType, ref ushort MapIoIndex, ref ushort MapIoLogic, ref ushort MapIoFilter);

        /// <summary>
        /// 设置停止额外PDO的虚拟映射。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="VirtualExtraPdo">虚拟额外PDO索引。</param>
        /// <param name="MapExtraPdoType">映射的PDO类型。</param>
        /// <param name="MapExtraPdoAddress">映射的PDO地址。</param>
        /// <param name="MapDataLen">数据长度。</param>
        /// <param name="MapMaxData">数据最大值。</param>
        /// <param name="MapMinData">数据最小值。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_stop_extra_pdo_map_virtual(ushort ConnectNo, ushort VirtualExtraPdo, ushort MapExtraPdoType, ushort MapExtraPdoAddress, ushort MapDataLen, UInt32 MapMaxData, UInt32 MapMinData);

        /// <summary>
        /// 获取停止额外PDO的虚拟映射配置。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="VirtualExtraPdo">虚拟额外PDO索引。</param>
        /// <param name="MapExtraPdoType">返回映射的PDO类型。</param>
        /// <param name="MapExtraPdoAddress">返回映射的PDO地址。</param>
        /// <param name="MapDataLen">返回数据长度。</param>
        /// <param name="MapMaxData">返回数据最大值。</param>
        /// <param name="MapMinData">返回数据最小值。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_stop_extra_pdo_map_virtual(ushort ConnectNo, ushort VirtualExtraPdo, ref ushort MapExtraPdoType, ref ushort MapExtraPdoAddress, ref ushort MapDataLen, ref UInt32 MapMaxData, ref UInt32 MapMinData);

        /// <summary>
        /// 设置基于虚拟IO和PDO的额外停止条件。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Axis">应用该停止条件的轴号。</param>
        /// <param name="VirtualIoNum">参与条件的虚拟IO数量。</param>
        /// <param name="VirtualIoList">参与条件的虚拟IO列表。</param>
        /// <param name="VirtualExtraPdoNum">参与条件的虚拟PDO数量。</param>
        /// <param name="VirtualExtraPdoList">参与条件的虚拟PDO列表。</param>
        /// <param name="CmpMode">比较模式 (例如: AND, OR)。</param>
        /// <param name="StopMode">停止模式 (0:减速停止, 1:立即停止)。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_extra_stop_mode(ushort ConnectNo, ushort Axis, ushort VirtualIoNum, ushort[] VirtualIoList, ushort VirtualExtraPdoNum, ushort[] VirtualExtraPdoList, ushort CmpMode, ushort StopMode);

        /// <summary>
        /// (扩展)读取指定轴有关运动信号的状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="state">返回一个32位整数，表示多个信号的状态位集合。请参考API手册“表3.2 轴的运动信号状态”进行位解析。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>此函数是 smc_axis_io_status 的扩展版本，可返回更多状态位。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_axis_io_status_ex(ushort ConnectNo, ushort axis, ref int state);

        /// <summary>
        /// (扩展)读取指定轴特殊信号的使能状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="state">返回一个32位整数，表示多个特殊IO信号的使能状态位集合。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>此函数是 smc_axis_io_enable_status 的扩展版本。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_axis_io_enable_status_ex(ushort ConnectNo, ushort axis, ref int state);

        /// <summary>
        /// 配置二维螺距补偿参数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">需要补偿的目标轴号。</param>
        /// <param name="ref_axis">参考轴列表。</param>
        /// <param name="axis_start_pos">各轴的补偿起始位置。</param>
        /// <param name="axis_length">各轴的补偿总长度。</param>
        /// <param name="axis_segment">轴段数。</param>
        /// <param name="CompPos">补偿数据表，存储每个格网点的补偿值。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_leadscrew_comp_2D_config_unit(ushort ConnectNo, ushort axis, ref ushort ref_axis, ref double axis_start_pos, ref double axis_length, ref double axis_segment, double[] CompPos);

        /// <summary>
        /// 读取二维螺距补偿参数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">需要补偿的目标轴号。</param>
        /// <param name="ref_axis">返回参考轴列表。</param>
        /// <param name="axis_start_pos">返回各轴的补偿起始位置。</param>
        /// <param name="axis_length">返回各轴的补偿总长度。</param>
        /// <param name="axis_segment">返回轴段数。</param>
        /// <param name="CompPos">返回补偿数据表。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_leadscrew_comp_2D_config_unit(ushort ConnectNo, ushort axis, ref ushort ref_axis, ref double axis_start_pos, ref double axis_length, ref ushort axis_segment, double[] CompPos);

        /// <summary>
        /// 设置二维螺距补偿的旋转角度。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">需要补偿的目标轴号。</param>
        /// <param name="ref_axis">参考轴列表。</param>
        /// <param name="axis_start_pos">各轴的补偿起始位置。</param>
        /// <param name="axis_length">各轴的补偿总长度。</param>
        /// <param name="angle">补偿坐标系的旋转角度。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_leadscrew_comp_2D_angle_unit(ushort ConnectNo, ushort axis, ref ushort ref_axis, double[] axis_start_pos, double[] axis_length, double angle);

        /// <summary>
        /// 读取二维螺距补偿的旋转角度。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">需要补偿的目标轴号。</param>
        /// <param name="ref_axis">返回参考轴列表。</param>
        /// <param name="axis_start_pos">返回各轴的补偿起始位置。</param>
        /// <param name="axis_length">返回各轴的补偿总长度。</param>
        /// <param name="angle">返回补偿坐标系的旋转角度。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_leadscrew_comp_2D_angle_unit(ushort ConnectNo, ushort axis, ref ushort ref_axis, ref double axis_start_pos, ref double axis_length, ref double angle);

        /// <summary>
        /// 设置二维螺距补偿功能的使能状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">需要补偿的目标轴号。</param>
        /// <param name="mode">补偿模式。</param>
        /// <param name="enable">使能状态。0: 禁止, 1: 使能。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_leadscrew_comp_2D_enable(ushort ConnectNo, ushort axis, ushort mode, ushort enable);

        /// <summary>
        /// 获取二维螺距补偿功能的使能状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">需要补偿的目标轴号。</param>
        /// <param name="mode">返回补偿模式。</param>
        /// <param name="enable">返回使能状态。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_leadscrew_comp_2D_enable(ushort ConnectNo, ushort axis, ref ushort mode, ref ushort enable);

        /// <summary>
        /// (扩展)配置二维螺距补偿参数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="table_index">补偿表索引。</param>
        /// <param name="axis">需要补偿的目标轴号。</param>
        /// <param name="ref_axis">参考轴列表。</param>
        /// <param name="axis_start_pos">各轴的补偿起始位置。</param>
        /// <param name="axis_length">各轴的补偿总长度。</param>
        /// <param name="axis_segment">轴段数。</param>
        /// <param name="value">补偿值。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_leadscrew_comp_2D_config_unit_ex(ushort ConnectNo, ushort table_index, ushort axis, ref ushort ref_axis, ref double axis_start_pos, ref double axis_length, ref ushort axis_segment, ref double value);

        /// <summary>
        /// (扩展)读取二维螺距补偿参数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="table_index">补偿表索引。</param>
        /// <param name="axis">返回需要补偿的目标轴号。</param>
        /// <param name="axis_start_pos">返回各轴的补偿起始位置。</param>
        /// <param name="axis_length">返回各轴的补偿总长度。</param>
        /// <param name="axis_segment">返回轴段数。</param>
        /// <param name="value">返回补偿值。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_leadscrew_comp_2D_config_unit_ex(ushort ConnectNo, ushort table_index, ref ushort axis, ref double axis_start_pos, ref double axis_length, ref ushort axis_segment, double value);

        /// <summary>
        /// (扩展)设置二维螺距补偿的旋转角度。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="table_index">补偿表索引。</param>
        /// <param name="comp_axis">补偿轴。</param>
        /// <param name="axis">轴号列表。</param>
        /// <param name="axis_start_pos">各轴的补偿起始位置。</param>
        /// <param name="axis_length">各轴的补偿总长度。</param>
        /// <param name="angle">补偿坐标系的旋转角度。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_leadscrew_comp_2D_angle_unit_ex(ushort ConnectNo, ushort table_index, ushort comp_axis, ushort[] axis, double[] axis_start_pos, double[] axis_length, double angle);

        /// <summary>
        /// (扩展)读取二维螺距补偿的旋转角度。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="table_index">补偿表索引。</param>
        /// <param name="comp_axis">返回补偿轴。</param>
        /// <param name="axis">返回轴号列表。</param>
        /// <param name="axis_start_pos">返回各轴的补偿起始位置。</param>
        /// <param name="axis_length">返回各轴的补偿总长度。</param>
        /// <param name="angle">返回补偿坐标系的旋转角度。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_leadscrew_comp_2D_angle_unit_ex(ushort ConnectNo, ushort table_index, ushort[] comp_axis, ushort[] axis, double[] axis_start_pos, double[] axis_length, ref double angle);

        /// <summary>
        /// (扩展)设置二维螺距补偿功能的使能状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="table_index">补偿表索引。</param>
        /// <param name="mode">补偿模式。</param>
        /// <param name="enable">使能状态。0: 禁止, 1: 使能。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_leadscrew_comp_2D_enable_ex(ushort ConnectNo, ushort table_index, ushort mode, ushort enable);

        /// <summary>
        /// (扩展)获取二维螺距补偿功能的使能状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="table_index">补偿表索引。</param>
        /// <param name="mode">返回补偿模式。</param>
        /// <param name="enable">返回使能状态。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_leadscrew_comp_2D_enable_ex(ushort ConnectNo, ushort table_index, ref ushort mode, ref ushort enable);

        //螺距补偿
        /// <summary>
        /// 设置螺距补偿的使能与禁止 (API 3.5)。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="enable">螺距补偿使能状态：0 - 禁止；1 - 使能。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_enable_leadscrew_comp(ushort ConnectNo, ushort axis, ushort enable);

        /// <summary>
        /// 配置螺距补偿参数 (API 3.5)。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="n">补偿点数，最大为 256。</param>
        /// <param name="startpos">补偿起始位置，单位：unit。</param>
        /// <param name="lenpos">补偿段的总长度，单位：unit。</param>
        /// <param name="pCompPos">对应为正方向运动时，各点位置需要补偿的位置值数组，单位：unit。</param>
        /// <param name="pCompNeg">对应为负方向运动时，各点位置需要补偿的脉冲数数组，单位：unit。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：一般在回零完成之后使用该功能，起点补偿值为 0。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_leadscrew_comp_config_unit(ushort ConnectNo, ushort axis, ushort n, double startpos, double lenpos, double[] pCompPos, double[] pCompNeg);

        /// <summary>
        /// 读取螺距补偿参数 (API 3.5)。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="n">返回补偿点数。</param>
        /// <param name="startpos">返回补偿起始位置，单位：unit。</param>
        /// <param name="lenpos">返回补偿段的总长度，单位：unit。</param>
        /// <param name="pCompPos">返回正向运动补偿位置值数组。</param>
        /// <param name="pCompNeg">返回负向运动补偿脉冲数数组。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_leadscrew_comp_config_unit(ushort ConnectNo, ushort axis, ref ushort n, ref double startpos, ref double lenpos, double[] pCompPos, double[] pCompNeg);

        /// <summary>
        /// 读取指定轴的螺距补偿后的指令位置 (API 3.5)。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="pos">返回应用螺距补偿后的当前位置值，单位：unit。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_position_ex_unit(ushort ConnectNo, ushort axis, ref double pos);//读取补偿后的位置

        //龙门功能
        /// <summary>
        /// 设置龙门（Gantry）跟随模式参数 (API 3.6)。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">跟随轴（从轴）的轴号。</param>
        /// <param name="enable">使能状态：0 - 禁止；1 - 使能。</param>
        /// <param name="master_axis">主轴的轴号。</param>
        /// <param name="ratio">保留参数，固定设为 1。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：一个主轴可对应多个从轴，但一个从轴只能对应一个主轴。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_gear_follow_profile(UInt16 ConnectNo, UInt16 axis, UInt16 enable, UInt16 master_axis, double ratio);

        /// <summary>
        /// 读取龙门（Gantry）跟随模式参数 (API 3.6)。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">跟随轴（从轴）的轴号。</param>
        /// <param name="enable">返回使能状态。</param>
        /// <param name="master_axis">返回主轴的轴号。</param>
        /// <param name="ratio">返回保留参数。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_gear_follow_profile(UInt16 ConnectNo, UInt16 axis, ref UInt16 enable, ref UInt16 master_axis, ref double ratio);

        /// <summary>
        /// 设置龙门模式下主从轴编码器跟随误差保护阈值 (API 3.6)。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">主轴轴号。</param>
        /// <param name="enable">误差保护使能状态：0 - 禁止；1 - 使能。</param>
        /// <param name="dstp_error">减速停止的误差阈值，单位：unit。</param>
        /// <param name="emg_error">立即停止的误差阈值，单位：unit。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>此函数必须在 smc_set_gear_follow_profile 建立龙门关系后调用。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_set_grant_error_protect_unit(UInt16 ConnectNo, UInt16 axis, UInt16 enable, double dstp_error, double emg_error);

        /// <summary>
        /// 读取龙门模式下编码器位置跟随误差保护设置 (API 3.6)。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">主轴轴号。</param>
        /// <param name="enable">返回误差保护使能状态。</param>
        /// <param name="dstp_error">返回减速停止的误差阈值。</param>
        /// <param name="emg_error">返回立即停止的误差阈值。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_get_grant_error_protect_unit(UInt16 ConnectNo, UInt16 axis, ref UInt16 enable, ref double dstp_error, ref double emg_error);

        //软启动/软着陆
        /// <summary>
        /// 执行单轴软着陆运动 (API 3.10)。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">指定轴号，范围：0 至 实际轴数-1。</param>
        /// <param name="MidPos">第一段高速运动的终点位置。</param>
        /// <param name="TargetPos">第二段低速运动的终点位置。</param>
        /// <param name="Min_Vel">起始速度 (Vs)，单位：unit/s。</param>
        /// <param name="Max_Vel">最大速度 (Vm)，单位：unit/s。</param>
        /// <param name="stop_Vel">第二段低速运动的停止速度 (Ve)，单位：unit/s。</param>
        /// <param name="acc_time">加速时间，单位：s，范围：0.001-2~31s。</param>
        /// <param name="dec_time">减速时间，单位：s，范围：0.001-2~31s。</param>
        /// <param name="posi_mode">运动模式：0 - 相对模式；1 - 绝对模式。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_t_pmove_extern_unit(UInt16 ConnectNo, UInt16 axis, double MidPos, double TargetPos, double Min_Vel, double Max_Vel, double stop_Vel, double acc_time, double dec_time, UInt16 posi_mode);

        /// <summary>
        /// 执行单轴软启动运动 (API 3.10)。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">指定轴号，范围：0 至 实际轴数-1。</param>
        /// <param name="MidPos">第一段低速运动的终点位置。</param>
        /// <param name="TargetPos">第二段高速运动的终点位置。</param>
        /// <param name="start_Vel">第一段 pmove 的起始速度。</param>
        /// <param name="Max_Vel">第一段 pmove 的最大速度。</param>
        /// <param name="stop_Vel">第一段 pmove 的停止速度。</param>
        /// <param name="delay_time">第一阶段完成后延迟时间，单位：ms，范围：0-100s。</param>
        /// <param name="Max_Vel2">第二段 pmove 的最大速度。</param>
        /// <param name="stop_vel2">第二段 pmove 的停止速度。</param>
        /// <param name="acc_time">加速时间，单位：s，范围：0.001s-100s。</param>
        /// <param name="dec_time">减速时间，单位：s，范围：0.001s-100s。</param>
        /// <param name="posi_mode">运动模式：0 - 相对模式；1 - 绝对模式。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_t_pmove_extern_softstart_unit(UInt16 ConnectNo, UInt16 axis, double MidPos, double TargetPos, double start_Vel, double Max_Vel, double stop_Vel, double delay_time, double Max_Vel2, double stop_vel2, double acc_time, double dec_time, UInt16 posi_mode);

        /// <summary>
        /// 强行改变指定轴的目标位置并实现软着陆 (API 3.10)。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">指定轴号，范围：0 至 实际轴数-1。</param>
        /// <param name="mid_pos">第一段运动的终点位置，单位：unit。以此位置的速度规划指令中的最大速度运行。</param>
        /// <param name="aim_pos">第二段运动的终点位置，单位：unit。以此位置的速度规划指令中的停止速度运行。</param>
        /// <param name="vel">保留参数，固定值为 0。</param>
        /// <param name="posi_mode">保留参数，固定值为 0。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>适用于轴停止或点位运动中。参数 mid_pos 和 aim_pos 均为绝对坐标位置值。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_update_target_position_extern_unit(UInt16 ConnectNo, UInt16 axis, double mid_pos, double aim_pos, double vel, UInt16 posi_mode);

        //椭圆插补
        /// <summary>
        /// 启动椭圆插补运动 (API 3.12)。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="Crd">坐标系号，范围 0-7。</param>
        /// <param name="axisNum">坐标系轴数量，固定为 2。</param>
        /// <param name="Axis_List">坐标系轴号列表数组。</param>
        /// <param name="Target_Pos">目标位置列表数组。</param>
        /// <param name="Cen_Pos">椭圆圆心位置列表数组。</param>
        /// <param name="A_Axis_Len">长半轴长度。</param>
        /// <param name="B_Axis_Len">短半轴长度。</param>
        /// <param name="Dir">插补方向：0 - 顺时针；1 - 逆时针。</param>
        /// <param name="Pos_Mode">插补模式：0 - 相对运动；1 - 绝对运动。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        [DllImport("LTSMC.dll")]
        public static extern short smc_ellipse_move(UInt16 ConnectNo, ushort Crd, UInt16 axisNum, UInt16[] Axis_List, double[] Target_Pos, double[] Cen_Pos, double A_Axis_Len, double B_Axis_Len, UInt16 Dir, UInt16 Pos_Mode);

        /// <summary>
        /// 设置总线型驱动器的回零参数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="home_mode">回零模式，具体模式定义请参考对应总线驱动器手册。</param>
        /// <param name="High_Vel">回零高速。</param>
        /// <param name="Low_Vel">回零低速。</param>
        /// <param name="Tacc">加速时间。</param>
        /// <param name="Tdec">减速时间。</param>
        /// <param name="offsetpos">回零完成后的位置偏移量。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_set_home_profile(ushort ConnectNo, ushort axis, ushort home_mode, double High_Vel, double Low_Vel, double Tacc, double Tdec, double offsetpos);

        /// <summary>
        /// 读取总线型驱动器的回零参数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号。</param>
        /// <param name="home_mode">返回回零模式。</param>
        /// <param name="High_Vel">返回回零高速。</param>
        /// <param name="Low_Vel">返回回零低速。</param>
        /// <param name="Tacc">返回加速时间。</param>
        /// <param name="Tdec">返回减速时间。</param>
        /// <param name="offsetpos">返回位置偏移量。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_home_profile(ushort ConnectNo, ushort axis, ref ushort home_mode, ref double High_Vel, ref double Low_Vel, ref double Tacc, ref double Tdec, ref double offsetpos);

        /// <summary>
        /// 获取总线上指定端口的从站总数。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="TotalSlaves">返回该端口下的从站总数。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_get_total_slaves(ushort ConnectNo, ushort PortNum, ref ushort TotalSlaves);

        /// <summary>
        /// 写入总线上指定从站节点的单个输出位状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="NodeID">从站节点ID。</param>
        /// <param name="IoBit">要操作的输出位索引。</param>
        /// <param name="IoValue">要写入的值 (0 或 1)。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_write_outbit(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort IoBit, ushort IoValue);

        /// <summary>
        /// 读取总线上指定从站节点的单个输出位状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="NodeID">从站节点ID。</param>
        /// <param name="IoBit">要读取的输出位索引。</param>
        /// <param name="IoValue">返回当前输出位的值。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_outbit(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort IoBit, ref ushort IoValue);

        /// <summary>
        /// 读取总线上指定从站节点的单个输入位状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="NodeID">从站节点ID。</param>
        /// <param name="IoBit">要读取的输入位索引。</param>
        /// <param name="IoValue">返回当前输入位的值。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_inbit(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort IoBit, ref ushort IoValue);

        /// <summary>
        /// 写入总线上指定从站节点的整个输出端口的值。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="NodeID">从站节点ID。</param>
        /// <param name="PortNo">要操作的输出端口号。</param>
        /// <param name="IoValue">要写入的端口值（通常为16位或32位整数）。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_write_outport(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort PortNo, int IoValue);

        /// <summary>
        /// 读取总线上指定从站节点的整个输出端口的值。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="NodeID">从站节点ID。</param>
        /// <param name="PortNo">要读取的输出端口号。</param>
        /// <param name="IoValue">返回当前输出端口的值。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_outport(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort PortNo, ref int IoValue);

        /// <summary>
        /// 读取总线上指定从站节点的整个输入端口的值。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="PortNum">总线端口号。</param>
        /// <param name="NodeID">从站节点ID。</param>
        /// <param name="PortNo">要读取的输入端口号。</param>
        /// <param name="IoValue">返回当前输入端口的值。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short nmcs_read_inport(ushort ConnectNo, ushort PortNum, ushort NodeID, ushort PortNo, ref int IoValue);

        /// <summary>
        /// (扩展)检查指定轴的运动完成状态。
        /// </summary>
        /// <param name="ConnectNo">控制器链接号，范围：0-254。</param>
        /// <param name="axis">轴号，范围：0 至 控制器最大轴数-1。</param>
        /// <param name="state">返回运动状态。可能的值包括但不限于: 0-运动中, 1-正常停止。</param>
        /// <returns>返回 0 表示执行成功，非 0 表示错误代码。</returns>
        /// <remarks>注意：此函数定义未在提供的 V2.1 API 文档中找到，注释基于函数命名推断，是 smc_check_done 的扩展。</remarks>
        [DllImport("LTSMC.dll")]
        public static extern short smc_check_done_ex(ushort ConnectNo, ushort axis, ref ushort state);
    }
}

