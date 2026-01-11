using LanguageExt;

namespace Common.Ui;

public abstract record Node;

public sealed record PageNode(string Title, Node Body) : Node;
public sealed record ScrollNode(Node Body) : Node;
public sealed record TabsNode(IReadOnlyList<TabNode> Tabs) : Node;
public sealed record TabNode(string Title, Node Body) : Node;
public sealed record SectionNode(string Title, Node Body) : Node;
public sealed record GridNode(int Columns, IReadOnlyList<Node> Children) : Node;
public sealed record VStackNode(IReadOnlyList<Node> Children) : Node;
public sealed record HStackNode(IReadOnlyList<Node> Children) : Node;
public sealed record SplitNode(Orientation Orientation, Node First, Node Second) : Node;
public sealed record ExpanderNode(string Title, Node Body, bool InitiallyExpanded = true) : Node;

public sealed record TextNode(string Text) : Node;
public sealed record LabelNode(string Text) : Node;
public sealed record HelpNode(string Text) : Node;

public sealed record ListNode(ListBinding Binding, Func<int, Ui<Unit>> ItemUi) : Node;
public sealed record DictionaryNode(DictionaryBinding Binding, Func<string, Ui<Unit>> KeyUi, Func<object, Ui<Unit>> ValueUi) : Node;

public sealed record FieldNode(FieldSpec Spec) : Node;

public sealed record ConditionalNode(Type ModelType, Func<object, bool> Predicate, Ui<Unit> Body) : Node;

public sealed record KeyEditorNode(string Current, IReadOnlyList<string> Suggested, bool AllowFreeText) : Node;

public sealed record ObjectBinding(
    string Id,
    Type ModelType,
    Type ObjectType,
    Func<object, object?> Get,
    Action<object, object?> Set,
    Func<object> Create
) : Binding;

/// <summary>
/// 非可选对象：确保对象存在并递归渲染其 Body。
/// </summary>
public sealed record ObjectNode(string Title, ObjectBinding Binding, Ui<Unit> Body, bool InitiallyExpanded = true) : Node;

/// <summary>
/// 可选对象：通过一个勾选启用/禁用，并在启用时递归渲染其 Body。
/// 禁用时会把对象设为 null。
/// </summary>
public sealed record OptionalObjectNode(
    string Title,
    ObjectBinding Binding,
    Ui<Unit> Body,
    bool InitiallyExpanded = true,
    bool DefaultEnabled = false) : Node;

public enum Orientation
{
    Horizontal,
    Vertical
}
