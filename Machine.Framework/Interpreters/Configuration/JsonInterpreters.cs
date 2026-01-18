using System;
using System.Text.Json;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Core.Configuration;

namespace Machine.Framework.Interpreters.Configuration
{
    /// <summary>
    /// JSON 导出解释器
    /// 将 MachineConfig DSL 解释为 JSON 字符串
    /// </summary>
    public class JsonExportInterpreter : IConfigInterpreter<string>
    {
        private readonly JsonSerializerOptions _options;

        public JsonExportInterpreter(bool indented = true)
        {
            _options = new JsonSerializerOptions 
            { 
                WriteIndented = indented
            };
        }

        public string Interpret(MachineConfig config)
        {
            return JsonSerializer.Serialize(config, _options);
        }
    }

    /// <summary>
    /// JSON 导入解释器 (反向解释器)
    /// 将 JSON 字符串解释为 MachineConfig DSL 对象
    /// 注意：这个稍微特殊，因为它是从外部数据源构建 DSL，类似于 "Loader"
    /// </summary>
    public class JsonImportInterpreter
    {
        public MachineConfig? Interpret(string json)
        {
            return JsonSerializer.Deserialize<MachineConfig>(json);
        }
    }
}
