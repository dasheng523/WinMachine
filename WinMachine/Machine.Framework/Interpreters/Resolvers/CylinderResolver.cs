using System;
using System.Linq;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Core.Hardware;
using Machine.Framework.Core.Hardware.Interfaces;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using Machine.Framework.Core.Configuration.Models;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace Machine.Framework.Interpreters.Resolvers
{
    public class CylinderResolver : ICylinderResolver
    {
        private readonly IIoResolver _io;
        private readonly SystemOptions _options;

        public CylinderResolver(IIoResolver io, IOptions<SystemOptions> options)
        {
            _io = io;
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public Fin<ISingleSolenoidCylinder> Resolve(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return FinFail<ISingleSolenoidCylinder>(Error.New("Cylinder name cannot be empty"));
            
            var conf = _options.Cylinders?.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (conf == null)
                 return FinFail<ISingleSolenoidCylinder>(Error.New($"Cylinder config not found: {name}"));

            return _io.ResolveDo(conf.MoveDo)
                .Map(d => (ISingleSolenoidCylinder)new SingleSolenoidCylinder(name, d));
        }

        private class SingleSolenoidCylinder : ISingleSolenoidCylinder
        {
             private readonly string _name;
             private readonly IDigitalOutput _moveOut;

             public SingleSolenoidCylinder(string name, IDigitalOutput moveOut)
             {
                 _name = name;
                 _moveOut = moveOut;
             }

             public Fin<LUnit> Set(Level level) => _moveOut.Write(level);
             
             // Simplification: just return current DO state if not using feedback sensors yet
             public Fin<Level> Get() => FinSucc(Level.Off); 
        }
    }
}
