using Machine.Framework.Core.Primitives; 

namespace Machine.Framework.Core.Hardware.Models
{
    public enum MotionDirection { Positive, Negative }
    
    public struct AxisSpeed
    {
        public double Min;
        public double Max;
        public double Tacc;
        public double Tdec;
        public double Stop;
        public double S_Para;
    }

    public struct AxisStatus
    {
        public bool Moving;
        public bool Error;
        public Level Origin;
        public Level PositiveHardLimit;
        public Level NegativeHardLimit;
    }
}
