using System;
using System.Collections.Generic;
using System.Text;

namespace Devices.Shared
{
    public interface IDevice
    {
        bool IsInit { get; }
        void Init();
    }
}
