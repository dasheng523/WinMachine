namespace Machine.Framework.Core.Configuration.Models
{
    // ===========================================
    // Universal / DSL Enums (Vendor Agnostic)
    // ===========================================

    /// <summary>
    /// 脉冲输出模式 (组合了模式与极性)
    /// </summary>
    public enum PulseOutputMode
    {
        /// <summary>Pulse/Dir, Pulse High Active, Dir High=Positive</summary>
        PulseDir_High_PosHigh = 0,
        /// <summary>Pulse/Dir, Pulse High Active, Dir Low=Positive</summary>
        PulseDir_High_PosLow = 1,
        /// <summary>Pulse/Dir, Pulse Low Active, Dir High=Positive</summary>
        PulseDir_Low_PosHigh = 2,
        /// <summary>Pulse/Dir, Pulse Low Active, Dir Low=Positive</summary>
        PulseDir_Low_PosLow = 3,
        /// <summary>CW/CCW, High Active</summary>
        CwCcw_High = 4,
        /// <summary>CW/CCW, Low Active</summary>
        CwCcw_Low = 5,
        /// <summary>AB Phase</summary>
        AbPhase = 6
    }

    /// <summary>
    /// 编码器输入模式
    /// </summary>
    public enum EncoderInputMode
    {
        /// <summary>非A/B相 (脉冲/方向)</summary>
        PulseDir = 0,
        /// <summary>1倍频 A/B相</summary>
        AbPhase_1x = 1,
        /// <summary>2倍频 A/B相</summary>
        AbPhase_2x = 2,
        /// <summary>4倍频 A/B相</summary>
        AbPhase_4x = 3
    }

    /// <summary>
    /// 编码器计数方向
    /// </summary>
    public enum EncoderDir
    {
        /// <summary>A超前B为增加</summary>
        A_Lead_B = 0,
        /// <summary>B超前A为增加</summary>
        B_Lead_A = 1
    }

    /// <summary>
    /// 有效电平逻辑
    /// </summary>
    public enum ActiveLevel
    {
        Low = 0,
        High = 1
    }

    /// <summary>
    /// 停止动作模式
    /// </summary>
    public enum StopAction
    {
        /// <summary>立即停止 (Emergency Stop)</summary>
        Immediate = 0,
        /// <summary>减速停止 (Decelerate Stop)</summary>
        Decelerate = 1
    }

    /// <summary>
    /// 轴专用IO类型
    /// </summary>
    public enum AxisIoType
    {
        LimitPositive = 0, 
        LimitNegative = 1,
        Org = 2,
        Emg = 3,
        Alarm = 5,
        InPosition = 7
    }

    /// <summary>
    /// 回原点方向
    /// </summary>
    public enum HomeDir
    {
        Negative = 0,
        Positive = 1
    }

    /// <summary>
    /// IO映射目标类型
    /// </summary>
    public enum IoMapType
    {
        /// <summary>专用输入口 (Default)</summary>
        Dedicated = -1, 

        // Target Pin Types (Maps to Leadshine MapIoType 0-5)
        LimitPositive = 0,
        LimitNegative = 1,
        Org = 2,
        Alarm = 3,  // Note: Leadshine SDK says 3=ALM input
        InPosition = 5,
        GeneralInput = 6
    }

    /// <summary>
    /// 回原点模式 (基于 Leadshine 定义扩展)
    /// </summary>
    public enum HomeMode
    {
        /// <summary>0: 一次回零</summary>
        Once = 0,
        /// <summary>1: 一次回零加反找</summary>
        Once_Reverse = 1,
        /// <summary>2: 二次回零</summary>
        Twice = 2,
        /// <summary>3: 一次回零 + EZ</summary>
        Once_EZ = 3,
        /// <summary>4: 单独 EZ 回零</summary>
        EZ_Only = 4,
        /// <summary>5: 一次回零再反找 EZ</summary>
        Once_Reverse_EZ = 5,
        /// <summary>6: 原点锁存</summary>
        Latch_Org = 6,
        /// <summary>7: 原点锁存 + 同向 EZ 锁存</summary>
        Latch_Org_SameDirEZ = 7,
        /// <summary>8: 单独 EZ 锁存</summary>
        Latch_EZ_Only = 8,
        /// <summary>9: 原点锁存 + 反向 EZ 锁存</summary>
        Latch_Org_ReverseEZ = 9
    }

    // ===========================================
    // Leadshine Specific Enums (Strict Mapping)
    // ===========================================
    
    public enum LeadshineModel
    {
        DMC3000,
        SMC600,
        Unknown
    }

    public enum LeadshineLimitEnable : ushort
    {
        DisableAll = 0,
        EnableAll = 1,
        PosDisable_NegEnable = 2,
        PosEnable_NegDisable = 3
    }

    public enum LeadshineLimitLogic : ushort
    {
        AllLow = 0,
        AllHigh = 1,
        PosLow_NegHigh = 2,
        PosHigh_NegLow = 3
    }

    public enum LeadshineStopMode : ushort
    {
        AllImmediate = 0,
        AllDecel = 1,
        PosImm_NegDecel = 2,
        PosDecel_NegImm = 3
    }

    // ===========================================
    // ZMotion Specific Enums
    // ===========================================

    public enum ZMotionModel
    {
        ZMC432,
        ECI3808,
        Unknown
    }
    
    public enum SerialProtocol
    {
        ModbusRTU,
        ModbusASCII,
        HostLink
    }

    public enum SensorFeature
    {
        HeightValue,
        PressureValue,
        Temperature
    }
}
