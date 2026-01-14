using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Machine.Framework.Configuration
{
    public class SystemOptions
    {
        public List<AxisConfig> Axes { get; set; } = new();
        public List<IoRefOptions> Ios { get; set; } = new();
        public List<CylinderConfig> Cylinders { get; set; } = new();
    }

    public class AxisConfig
    {
        public string Name { get; set; }
        public string Board { get; set; }
        public ushort Axis { get; set; }
    }

    public class IoRefOptions
    {
         public string Name { get; set; }
         public string Board { get; set; }
         public int Channel { get; set; }
         public bool IsOutput { get; set; }
    }

    public class CylinderConfig
    {
        public string Name { get; set; }
        public string MoveDo { get; set; }
        public string ExtendedDi { get; set; }
        public string RetractedDi { get; set; }
        public int MoveTime { get; set; } = 1000;
    }
}
