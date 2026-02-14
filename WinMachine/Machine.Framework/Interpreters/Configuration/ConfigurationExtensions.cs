using System;
using Machine.Framework.Core.Configuration.Models;

namespace Machine.Framework.Interpreters.Configuration
{
    // 这个文件位于 Configuration 模块根目录，作为便捷扩展入口
    public static class ConfigurationExtensions
    {
        public static string ToJson(this MachineConfig config, bool indented = true)
        {
            var interpreter = new JsonExportInterpreter(indented);
            return interpreter.Interpret(config);
        }

        public static MachineConfig? FromJson(string json)
        {
            var interpreter = new JsonImportInterpreter();
            return interpreter.Interpret(json);
        }
    }
}
