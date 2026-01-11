using LanguageExt;

namespace WinMachine.ConfigUi;

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

public enum Orientation
{
    Horizontal,
    Vertical
}
