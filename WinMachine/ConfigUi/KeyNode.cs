using LanguageExt;
using static LanguageExt.Prelude;

namespace WinMachine.ConfigUi;

public sealed record KeyEditorNode(string Current, IReadOnlyList<string> Suggested, bool AllowFreeText) : Node;

public static partial class UI
{
    public static Ui<Unit> Key(string current, IReadOnlyList<string> suggested, bool allowFreeText) =>
        new(_ => (Arr.create<Node>(new KeyEditorNode(current, suggested, allowFreeText)), Arr<Binding>.Empty, unit));
}
