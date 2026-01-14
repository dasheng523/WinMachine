namespace Machine.Framework.Core.Core
{
    /// <summary>
    /// 逻辑电平或使能状�?
    /// </summary>
    public enum Level { Off, On }

    /// <summary>
    /// 设备基础接口
    /// </summary>
    public interface IDevice
    {
        bool IsInit { get; }
        void Init();
    }
}


