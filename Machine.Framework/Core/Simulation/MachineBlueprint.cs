using System;
using System.Collections.Generic;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Core.Configuration.Models;

namespace Machine.Framework.Core.Simulation
{
    public static class MachineBlueprint
    {
        public static IMachineBlueprintBuilder Define(string name) => new BlueprintBuilders.MachineBuilder(name);
    }

    public interface IMachineBlueprintBuilder
    {
        string Name { get; }
        IBoardBuilder AddBoard(string name, int cardId);
        IMachineBlueprintBuilder AddBoard(string name, int cardId, Action<IBoardBuilder> configure);
        IMachineBlueprintBuilder AddDevice(string name, Action<DeviceBuilder> configure);
        IMachineBlueprintBuilder AddBus(string name, Action<BusBuilder> configure);
        IMountPointBuilder Mount(string name);
        IMachineBlueprintBuilder Mount(string name, Action<IMountPointBuilder> configure);

        // LINQ Support
        IMachineBlueprintBuilder Select(Func<IMachineBlueprintBuilder, IMachineBlueprintBuilder> selector);
        TResult SelectMany<TIntermediate, TResult>(
            Func<IMachineBlueprintBuilder, TIntermediate> intermediateSelector,
            Func<IMachineBlueprintBuilder, TIntermediate, TResult> resultSelector);
    }

    public interface IBoardBuilder
    {
        IBoardBuilder UseSimulator();
        IBoardBuilder UseLeadshine(Action<ILeadshineDriverBuilder> configure);
        IBoardBuilder UseZMotion(Action<IZMotionDriverBuilder> configure);
        
        // 映射方法保留，用于纯 IO 映射（如 DI/DO）
        IBoardBuilder MapInput(Enum input, int port);
        IBoardBuilder MapOutput(Enum output, int port);

        // 设备定义：直接在板卡上定义轴/气缸，并指定通道/端口
        IAxisBuilder AddAxis(AxisID axis, int channel);
        IBoardBuilder AddAxis(AxisID axis, int channel, Action<IAxisBuilder> configure);
        
        ICylinderBuilder AddCylinder(CylinderID cylinder, int doOut, int doIn);
        IBoardBuilder AddCylinder(CylinderID cylinder, int doOut, int doIn, Action<ICylinderBuilder> configure);
    }

    public interface ILeadshineDriverBuilder
    {
        ILeadshineDriverBuilder Model(LeadshineModel model);
        ILeadshineDriverBuilder CardId(int id);
        ILeadshineDriverBuilder ConfigAxis(Enum axis, Action<AxisConfigBuilder> configure);
    }

    public interface IZMotionDriverBuilder
    {
        IZMotionDriverBuilder Model(ZMotionModel model);
        IZMotionDriverBuilder IpAddress(string ip);
    }

    public interface IAxisBuilder
    {
        IAxisBuilder WithKinematics(double maxVel, double maxAcc);
        IAxisBuilder WithRange(double min, double max);
        IAxisBuilder Vertical();
        IAxisBuilder Horizontal();
        IAxisBuilder Reversed();
    }

    public interface ICylinderBuilder
    {
        ICylinderBuilder WithFeedback(int diOut, int diIn);
        ICylinderBuilder WithDynamics(int actionTimeMs);
        ICylinderBuilder Vertical();
        ICylinderBuilder Horizontal();
    }

    public interface IMountPointBuilder
    {
        IMountPointBuilder AttachedTo(object parent);
        IMountPointBuilder LinkTo(object axisOrId);
        IMountPointBuilder WithTransform(Func<double, double> transform);
        IMountPointBuilder WithOffset(double x = 0, double y = 0, double z = 0);
        IMountPointBuilder Mount(string name);
        IMountPointBuilder Mount(string name, Action<IMountPointBuilder> configure);
    }
}
