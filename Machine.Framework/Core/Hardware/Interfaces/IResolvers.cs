using LanguageExt;

namespace Machine.Framework.Core.Hardware.Interfaces
{
    public interface IResolver<T>
    {
        Fin<T> Resolve(string name);
    }

    // Specialized resolvers used in HardwareFacade
    // NOTE: This interface depends on IMotionController. 
    // We should move IMotionController to Interfaces logic too to avoid circular dep or weirdness.
    // For now we use full name or fix after moving IMotionController.
    public interface IAxisResolver
    {
         // We will update this return type when IMotionController is standardized.
         // For now keeping it compatible if possible, or using generic ID.
         // Wait, the original used: Machine.Framework.Devices.Motion.Abstractions.IMotionController<ushort, ushort, ushort>
         // We want to verify untypped interface or clean it up. 
         // But let's copy as is for now, but referenced via whatever namespace it is continuously.
         // Assuming we also migrate IMotionController next.
         Fin<(object Controller, ushort Axis)> Resolve(string name); 
         // Updated to object Controller temporarily or use dynamic? 
         // Actually the project says "Define unified IMotionController". 
         // So I should define `IMotionController` in Interfaces first.
         
         Fin<ushort> ResolveOnPrimary(string name);
    }

    public interface IIoResolver 
    {
         Fin<IDigitalInput> ResolveDi(string name);
         Fin<IDigitalOutput> ResolveDo(string name);
    }

    public interface ICylinderResolver 
    {
        Fin<ISingleSolenoidCylinder> Resolve(string name);
    }
}
