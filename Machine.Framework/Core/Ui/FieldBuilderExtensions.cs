using LanguageExt;
using static LanguageExt.Prelude;

namespace Machine.Framework.Core.Ui;

public static class FieldBuilderExtensions
{
    /// <summary>
    /// C2 DSL：把 FieldBuilder 变成一个“Label + Field”行�? �?Node）�?
    /// 用法：UI.Field&lt;TModel, TProp&gt;(x =&gt; x.Prop).AsTextBox().Validate(...).Labeled("标题")
    /// </summary>
    public static Ui<Unit> Labeled<TModel, TProp>(this FieldBuilder<TModel, TProp> field, string label) =>
        new(s =>
        {
            var (nodes, bindings, _) = field.ToUi().Run(s);
            return (Arr.create<Node>(new LabelNode(label)) + nodes, bindings, unit);
        });
}


