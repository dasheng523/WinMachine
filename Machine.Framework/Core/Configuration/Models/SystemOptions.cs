using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Machine.Framework.Core.Configuration.Models
{
    public class SystemOptions
    {
        public List<AxisRefOptions> Axes { get; set; } = [];
        public List<IoRefOptions> Ios { get; set; } = [];
        public List<CylinderConfig> Cylinders { get; set; } = [];
    }

    public class AxisRefOptions
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

}
