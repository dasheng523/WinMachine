using System;
using System.Collections.Generic;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Core.Configuration.Models;

namespace Machine.Framework.Core.Blueprint
{
    /// <summary>
    /// 机器蓝图定义的入口点。
    /// 提供静态方法用于开始定义一个新的机器配置。
    /// </summary>
    public static class MachineBlueprint
    {
        /// <summary>
        /// 定义一个新的机器蓝图。
        /// </summary>
        /// <param name="name">机器名称</param>
        /// <returns>机器蓝图构建器接口</returns>
        public static IMachineBlueprintBuilder Define(string name) => new Builders.MachineBuilder(name);
    }

    /// <summary>
    /// 机器蓝图构建器接口。
    /// 用于配置板卡、设备、总线以及机械结构挂载点。
    /// </summary>
    public interface IMachineBlueprintBuilder
    {
        /// <summary>
        /// 机器名称。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 添加控制板卡。
        /// </summary>
        /// <param name="name">板卡名称</param>
        /// <param name="cardId">板卡ID</param>
        /// <returns>板卡构建器接口</returns>
        IBoardBuilder AddBoard(string name, int cardId);

        /// <summary>
        /// 添加并配置控制板卡。
        /// </summary>
        /// <param name="name">板卡名称</param>
        /// <param name="cardId">板卡ID</param>
        /// <param name="configure">配置委托</param>
        /// <returns>机器蓝图构建器接口（支持链式调用）</returns>
        IMachineBlueprintBuilder AddBoard(string name, int cardId, Action<IBoardBuilder> configure);

        /// <summary>
        /// 添加通用设备（预留扩展）。
        /// </summary>
        IMachineBlueprintBuilder AddDevice(string name, Action<DeviceBuilder> configure);

        /// <summary>
        /// 添加总线（预留扩展）。
        /// </summary>
        IMachineBlueprintBuilder AddBus(string name, Action<BusBuilder> configure);

        /// <summary>
        /// 定义根级机械挂载点。
        /// </summary>
        /// <param name="name">挂载点名称</param>
        /// <returns>挂载点构建器接口</returns>
        IMountPointBuilder Mount(string name);

        /// <summary>
        /// 定义并配置根级机械挂载点。
        /// </summary>
        /// <param name="name">挂载点名称</param>
        /// <param name="configure">配置委托</param>
        /// <returns>机器蓝图构建器接口（支持链式调用）</returns>
        IMachineBlueprintBuilder Mount(string name, Action<IMountPointBuilder> configure);

        // LINQ Support
        IMachineBlueprintBuilder Select(Func<IMachineBlueprintBuilder, IMachineBlueprintBuilder> selector);
        TResult SelectMany<TIntermediate, TResult>(
            Func<IMachineBlueprintBuilder, TIntermediate> intermediateSelector,
            Func<IMachineBlueprintBuilder, TIntermediate, TResult> resultSelector);
    }

    /// <summary>
    /// 板卡构建器接口。
    /// 用于配置板卡类型（仿真/雷赛/正运动）及映射物理轴和IO信号。
    /// </summary>
    public interface IBoardBuilder
    {
        /// <summary>
        /// 使用仿真驱动（默认）。
        /// </summary>
        IBoardBuilder UseSimulator();

        /// <summary>
        /// 使用雷赛运动控制卡驱动。
        /// </summary>
        IBoardBuilder UseLeadshine(Action<ILeadshineDriverBuilder> configure);

        /// <summary>
        /// 使用正运动控制卡驱动。
        /// </summary>
        IBoardBuilder UseZMotion(Action<IZMotionDriverBuilder> configure);
        
        // 映射方法保留，用于纯 IO 映射（如 DI/DO）
        /// <summary>
        /// 映射通用输入信号。
        /// </summary>
        IBoardBuilder MapInput(Enum input, int port);

        /// <summary>
        /// 映射通用输出信号。
        /// </summary>
        IBoardBuilder MapOutput(Enum output, int port);

        // 设备定义：直接在板卡上定义轴/气缸，并指定通道/端口
        /// <summary>
        /// 添加轴定义。
        /// </summary>
        /// <param name="axis">轴ID标识</param>
        /// <param name="channel">物理通道号</param>
        IBoardBuilder AddAxis(AxisID axis, int channel);

        /// <summary>
        /// 添加并配置轴定义。
        /// </summary>
        IBoardBuilder AddAxis(AxisID axis, int channel, Action<IAxisBuilder> configure);
        
        /// <summary>
        /// 添加气缸定义（单输出，无反馈）。
        /// <para>ON = 推出动作，OFF = 缩回动作。</para>
        /// </summary>
        /// <param name="cylinder">气缸ID标识</param>
        /// <param name="doOut">动作控制输出端口</param>
        IBoardBuilder AddCylinder(CylinderID cylinder, int doOut);

        /// <summary>
        /// 添加气缸定义（单输出，双反馈）。
        /// </summary>
        /// <param name="cylinder">气缸ID标识</param>
        /// <param name="doOut">动作控制输出端口</param>
        /// <param name="diOut">推出到位输入端口</param>
        /// <param name="diIn">缩回到位输入端口</param>
        IBoardBuilder AddCylinder(CylinderID cylinder, int doOut, int diOut, int diIn);

        /// <summary>
        /// 添加并配置气缸定义（如需定义双输出气缸，请在此配置中设置）。
        /// </summary>
        IBoardBuilder AddCylinder(CylinderID cylinder, int doOut, Action<ICylinderBuilder> configure);
    }

    /// <summary>
    /// 雷赛驱动配置构建器。
    /// </summary>
    public interface ILeadshineDriverBuilder
    {
        ILeadshineDriverBuilder Model(LeadshineModel model);
        ILeadshineDriverBuilder CardId(int id);
        ILeadshineDriverBuilder ConfigAxis(Enum axis, Action<AxisConfigBuilder> configure);
    }

    /// <summary>
    /// 正运动驱动配置构建器。
    /// </summary>
    public interface IZMotionDriverBuilder
    {
        IZMotionDriverBuilder Model(ZMotionModel model);
        IZMotionDriverBuilder IpAddress(string ip);
    }

    /// <summary>
    /// 轴配置构建器。
    /// 用于设置轴的运动参数、行程范围和物理属性。
    /// </summary>
    public interface IAxisBuilder
    {
        /// <summary>
        /// 配置运动学参数（速度、加速度）。
        /// </summary>
        /// <param name="maxVel">最大速度</param>
        /// <param name="maxAcc">最大加速度</param>
        IAxisBuilder WithKinematics(double maxVel, double maxAcc);

        /// <summary>
        /// 配置软限位行程范围。
        /// </summary>
        IAxisBuilder WithRange(double min, double max);

        /// <summary>
        /// 标记为垂直轴（受重力影响）。
        /// </summary>
        IAxisBuilder Vertical();

        /// <summary>
        /// 标记为水平轴（默认）。
        /// </summary>
        IAxisBuilder Horizontal();

        /// <summary>
        /// 配置电机反向。
        /// </summary>
        IAxisBuilder Reversed();
    }

    /// <summary>
    /// 气缸配置构建器。
    /// 用于配置气缸的传感器反馈和动作时间。
    /// </summary>
    public interface ICylinderBuilder
    {
        /// <summary>
        /// 配置缩回控制输出端口（用于双输出/双电控气缸）。
        /// </summary>
        ICylinderBuilder WithRetractPort(int port);

        /// <summary>
        /// 配置到位反馈传感器端口。
        /// </summary>
        /// <param name="diOut">推出到位输入端口</param>
        /// <param name="diIn">缩回到位输入端口</param>
        ICylinderBuilder WithFeedback(int diOut, int diIn);

        /// <summary>
        /// 配置动作模拟时间（无反馈时使用）。
        /// </summary>
        ICylinderBuilder WithDynamics(int actionTimeMs);

        /// <summary>
        /// 标记为垂直安装。
        /// </summary>
        ICylinderBuilder Vertical();

        /// <summary>
        /// 标记为水平安装。
        /// </summary>
        ICylinderBuilder Horizontal();
    }

    /// <summary>
    /// 机械挂载点构建器。
    /// 用于构建机器的物理层级结构（树状结构）。
    /// </summary>
    public interface IMountPointBuilder
    {
        /// <summary>
        /// （废弃）附着在父对象上。
        /// </summary>
        IMountPointBuilder AttachedTo(object parent);

        /// <summary>
        /// 将挂载点链接到物理设备（由轴或气缸驱动）。
        /// </summary>
        /// <param name="axisOrId">物理设备对象或ID</param>
        IMountPointBuilder LinkTo(object axisOrId);

        /// <summary>
        /// 自定义坐标变换函数。
        /// </summary>
        IMountPointBuilder WithTransform(Func<double, double> transform);
        
        // Pose Definitions (Static)
        /// <summary>
        /// 设置相对于父节点的静态偏移量（即局部坐标系的原点/锚点位置）。
        /// <para>注意：此坐标定义了该节点的**旋转中心**和挂载基准，是否对应几何中心取决于该节点挂载的可视化模型自身的原点设定。</para>
        /// </summary>
        IMountPointBuilder WithOffset(double x = 0, double y = 0, double z = 0);
        /// <summary>
        /// 设置相对于父节点的静态位姿（WithOffset 的别名）。
        /// </summary>
        IMountPointBuilder AtPose(double x, double y, double z); // Alias for WithOffset

        /// <summary>
        /// 设置相对于父节点的静态旋转（欧拉角，度）。
        /// </summary>
        IMountPointBuilder WithRotation(double x = 0, double y = 0, double z = 0); // Euler angles (deg)

        // Actuation Definitions (Dynamic)
        /// <summary>
        /// 配置动态行程向量（用于链接了轴或气缸的挂载点）。
        /// 定义从 0 到 1 的运动方向和距离。
        /// </summary>
        IMountPointBuilder WithStroke(double x, double y, double z); // Vector for 0->1 motion
        
        /// <summary>
        /// 创建子挂载点。
        /// </summary>
        IMountPointBuilder Mount(string name);

        /// <summary>
        /// 创建并配置子挂载点。
        /// </summary>
        IMountPointBuilder Mount(string name, Action<IMountPointBuilder> configure);

        // ------------------------------------------------------------------
        // 物理属性声明 (Physical Property Declarations)
        // ------------------------------------------------------------------

        /// <summary>
        /// 声明为通用立方体碰撞体。
        /// </summary>
        /// <param name="width">X 方向尺寸 (mm)</param>
        /// <param name="height">Y 方向尺寸 (mm)</param>
        /// <param name="depth">Z 方向尺寸 (mm)</param>
        IMountPointBuilder AsBox(double width, double height, double depth);

        /// <summary>
        /// 声明为吸笔（圆柱体碰撞）。
        /// </summary>
        /// <param name="diameter">直径 (mm)</param>
        /// <param name="length">长度 (mm)</param>
        IMountPointBuilder AsSuctionPen(double diameter, double length);

        /// <summary>
        /// 声明为旋转台。
        /// </summary>
        /// <param name="radius">半径 (mm)</param>
        IMountPointBuilder AsRotaryTable(double radius);

        /// <summary>
        /// 声明为直线导轨。
        /// </summary>
        /// <param name="length">行程长度 (mm)</param>
        IMountPointBuilder AsLinearGuide(double length);

        /// <summary>
        /// 声明为夹爪。
        /// </summary>
        IMountPointBuilder AsGripper();

        /// <summary>
        /// 声明为物料槽/托盘（用于物料边界检测）。
        /// </summary>
        /// <param name="width">宽度 (mm)</param>
        /// <param name="height">高度 (mm)</param>
        IMountPointBuilder AsMaterialSlot(double width, double height);

        /// <summary>
        /// 设置物理锚点位置。
        /// </summary>
        IMountPointBuilder WithAnchor(PhysicalAnchor anchor);

        /// <summary>
        /// 标记为垂直方向（沿 Z 轴）。
        /// </summary>
        IMountPointBuilder Vertical();

        /// <summary>
        /// 标记为水平方向（默认）。
        /// </summary>
        IMountPointBuilder Horizontal();

        /// <summary>
        /// 标记为反向安装。
        /// </summary>
        IMountPointBuilder Inverted();
    }
}
