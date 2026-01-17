using System;
using System.Collections.Concurrent;
using System.Threading;
using Machine.Framework.Core.Configuration.Models;

namespace Machine.Framework.Core.Flow
{
    /// <summary>
    /// Flow DSL 执行上下文。
    /// 承载了硬件配置、运行时的设备实例、取消令牌以及中间变量。
    /// </summary>
    public class FlowContext
    {
        /// <summary>
        /// 机器硬件配置元数据
        /// </summary>
        public MachineConfig Config { get; }

        /// <summary>
        /// 运行时设备实例映射表 (Key 为设备 ID，如 "X", "PickVacuum")
        /// </summary>
        public ConcurrentDictionary<string, object> Devices { get; } = new();

        /// <summary>
        /// 运行时变量池
        /// </summary>
        public ConcurrentDictionary<string, object> Variables { get; } = new();

        /// <summary>
        /// 取消令牌
        /// </summary>
        public CancellationToken CancellationToken { get; }

        public FlowContext(MachineConfig config, CancellationToken ct = default)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            CancellationToken = ct;
        }

        /// <summary>
        /// 获取或注册一个设备
        /// </summary>
        public T GetDevice<T>(string id) where T : class
        {
            if (Devices.TryGetValue(id, out var device))
            {
                return (T)device;
            }
            return null;
        }

        public void RegisterDevice(string id, object device)
        {
            Devices[id] = device;
        }
    }
}
