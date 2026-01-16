using System;
using Machine.Framework.Configuration.Models;
using Machine.Framework.Configuration.Interpreters;

namespace Machine.Framework
{
    // 这个文件位于更上层，可以同时看到 Config 和 Interpreters
    public static class DslExtensions
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
