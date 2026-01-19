using Machine.Framework.Core.Primitives;

namespace WinMachine;

public static class MachineDevices
{
    // 轴
    public static readonly AxisID SlideAxis = new("SlideAxis"); // 如果作为轴
    public static readonly AxisID LeftRotate = new("LeftRotate");
    public static readonly AxisID RightRotate = new("RightRotate");
    
    // 气缸
    public static readonly CylinderID SlideCyl = new("Slide"); // 如果作为气缸
    public static readonly CylinderID LeftLift = new("LeftLift");
    public static readonly CylinderID RightLift = new("RightLift");
    public static readonly CylinderID LeftGrip = new("LeftGrip");
    public static readonly CylinderID RightGrip = new("RightGrip");
    
    // X轴 (为了演示思路A中提到的X轴)
    public static readonly AxisID X_Axis = new("X");
    public static readonly AxisID Z1_Axis = new("Z1_Axis");
    public static readonly AxisID Z2_Axis = new("Z2_Axis");
}
