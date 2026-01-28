using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Reactive.Subjects;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Telemetry.Contracts;

namespace Machine.Framework.Core.Flow
{
    internal sealed class DeviceHub
    {
        private readonly ConcurrentDictionary<Type, object> _byType = new();

        public DeviceHub(params object[] devices)
        {
            foreach (var d in devices)
            {
                if (d == null) continue;
                _byType[d.GetType()] = d;
            }
        }

        public void Add(object device)
        {
            if (device == null) return;
            _byType[device.GetType()] = device;
        }

        public bool TryGet<T>(out T value) where T : class
        {
            // 1) 精确类型命中
            if (_byType.TryGetValue(typeof(T), out var v) && v is T exact)
            {
                value = exact;
                return true;
            }

            // 2) 支持按接口/基类查询
            foreach (var obj in _byType.Values)
            {
                if (obj is T t)
                {
                    value = t;
                    return true;
                }
            }

            value = null!;
            return false;
        }

        public static object Merge(object existing, object incoming)
        {
            if (existing is DeviceHub hub)
            {
                hub.Add(incoming);
                return hub;
            }
            if (incoming is DeviceHub incomingHub)
            {
                incomingHub.Add(existing);
                return incomingHub;
            }

            return new DeviceHub(existing, incoming);
        }
    }

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
        /// 物料状态表 (Key: StationID)
        /// </summary>
        public ConcurrentDictionary<string, MaterialInfo> MaterialStates { get; } = new();

        /// <summary>
        /// 事件广播流
        /// </summary>
        public Subject<TelemetryEvent> EventStream { get; } = new();

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
        public T? GetDevice<T>(string id) where T : class
        {
            if (Devices.TryGetValue(id, out var device))
            {
                if (device is T typed) return typed;

                if (device is DeviceHub hub && hub.TryGet<T>(out var hubTyped))
                {
                    return hubTyped;
                }
            }
            return null;
        }

        public void RegisterDevice(string id, object device)
        {
            Devices.AddOrUpdate(
                id,
                _ => device,
                (_, existing) => DeviceHub.Merge(existing, device));
        }
    }
}
