using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Machine.Framework.Configuration
{
    public class SystemOptions
    {
        public List<AxisConfig> Axes { get; set; } = [];
        public List<IoRefOptions> Ios { get; set; } = [];
        public List<CylinderConfig> Cylinders { get; set; } = [];
    }

    public class AxisConfig
    {
        public required string Name { get; set; }
        public required string Board { get; set; }
        public ushort Axis { get; set; }
    }

    public class IoRefOptions
    {
         public required string Name { get; set; }
         public required string Board { get; set; }
         public int Channel { get; set; }
         public bool IsOutput { get; set; }
    }

    public class CylinderConfig
    {
        public required string Name { get; set; }
        public required string MoveDo { get; set; }
        public required string ExtendedDi { get; set; }
        public required string RetractedDi { get; set; }
        public int MoveTime { get; set; } = 1000;
    }
}
