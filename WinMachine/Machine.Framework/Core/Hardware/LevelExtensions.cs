using Machine.Framework.Core.Primitives;

namespace Machine.Framework.Core.Hardware;

public static class LevelExtensions
{
    public static bool IsOn(this Level level) => level == Level.On;

    public static bool IsOff(this Level level) => level == Level.Off;

    public static Level ToLevel(this bool value) => value ? Level.On : Level.Off;
}


