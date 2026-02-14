using Machine.Framework.Core.Primitives;

namespace WinMachine;

public static class MachineDevices
{
    // --- 左侧机构 ---
    public static readonly AxisID Z1_Lift = new("Z1_Lift");          // 左侧升降轴
    public static readonly AxisID R1_Rotate = new("R1_Rotate");      // 左侧旋转轴
    public static readonly CylinderID C1_Left_Grip1 = new("C1_G1");  // 左侧夹爪1
    public static readonly CylinderID C1_Left_Grip2 = new("C1_G2");  // 左侧夹爪2
    public static readonly CylinderID C1_Left_Grip3 = new("C1_G3");  // 左侧夹爪3
    public static readonly CylinderID C1_Left_Grip4 = new("C1_G4");  // 左侧夹爪4

    // --- 右侧机构 ---
    public static readonly AxisID Z2_Lift = new("Z2_Lift");          // 右侧升降轴
    public static readonly AxisID R2_Rotate = new("R2_Rotate");      // 右侧旋转轴
    public static readonly CylinderID C2_Right_Grip1 = new("C2_G1"); // 右侧夹爪1
    public static readonly CylinderID C2_Right_Grip2 = new("C2_G2"); // 右侧夹爪2
    public static readonly CylinderID C2_Right_Grip3 = new("C2_G3"); // 右侧夹爪3
    public static readonly CylinderID C2_Right_Grip4 = new("C2_G4"); // 右侧夹爪4

    // --- 公共机构 ---
    public static readonly CylinderID SlideCyl = new("Slide");       // 滑台气缸

    // --- 测试专用设备 ---
    public static readonly CylinderID Test_Slide = new("Test_Slide");
    public static readonly CylinderID Test_Elevator = new("Test_Elevator");
    public static readonly CylinderID Test_Gripper = new("Test_Gripper");
    public static readonly CylinderID Test_Suction = new("Test_Suction");
    public static readonly AxisID Test_Linear = new("Test_Linear");
    public static readonly AxisID Test_Rotary = new("Test_Rotary");

    // 辅助定义：保留旧的以防编译错误，后续可以逐步清理
    public static readonly AxisID X_Axis = new("X");
    public static readonly AxisID LeftRotate = R1_Rotate;
    public static readonly AxisID RightRotate = R2_Rotate;
    public static readonly CylinderID LeftLift = new("LeftLift_Cyl"); // 旧定义
    public static readonly CylinderID RightLift = new("RightLift_Cyl"); // 旧定义
    public static readonly CylinderID LeftGrip = new("LeftGrip_Old");
    public static readonly CylinderID RightGrip = new("RightGrip_Old");
    public static readonly AxisID Z1_Axis = Z1_Lift;
    public static readonly AxisID Z2_Axis = Z2_Lift;
}
