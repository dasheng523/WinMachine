using System.Linq.Expressions;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Machine.Framework.Core.Ui;

public static partial class UI
{
    public static Ui<Unit> Title(string title) => Emit(new TextNode(title));

    public static Ui<Unit> Page(string title, Ui<Unit> body) => Wrap(body, n => new PageNode(title, n));

    public static Ui<Unit> Scroll(Ui<Unit> body) => Wrap(body, n => new ScrollNode(n));

    public static Ui<Unit> Tabs(params Ui<TabNode>[] tabs) =>
        new(s =>
        {
            var list = new List<TabNode>();
            foreach (var t in tabs)
            {
                var (_, _, tab) = t.Run(s);
                list.Add(tab);
            }

            return (Arr.create<Node>(new TabsNode(list)), Arr<Binding>.Empty, unit);
        });

    public static Ui<TabNode> Tab(string title, Ui<Unit> body) =>
        new(s =>
        {
            var (nodes, _, _) = body.Run(s);
            var tab = new TabNode(title, ToContainer(nodes));
            return (Arr<Node>.Empty, Arr<Binding>.Empty, tab);
        });

    public static Ui<Unit> Section(string title, Ui<Unit> body) => Wrap(body, n => new SectionNode(title, n));

    public static Ui<Unit> Expander(string title, Ui<Unit> body, bool initiallyExpanded = true) =>
        Wrap(body, n => new ExpanderNode(title, n, initiallyExpanded));

    public static Ui<Unit> Split(Orientation orientation, Ui<Unit> first, Ui<Unit> second) =>
        new(s =>
        {
            var (n1, _, _) = first.Run(s);
            var (n2, _, _) = second.Run(s);
            var node = new SplitNode(orientation, ToContainer(n1), ToContainer(n2));
            return (Arr.create<Node>(node), Arr<Binding>.Empty, unit);
        });

    public static Ui<Unit> VStack(params Ui<Unit>[] items) =>
        new(s =>
        {
            var nodes = new List<Node>();
            foreach (var it in items)
            {
                var (ns, _, _) = it.Run(s);
                nodes.AddRange(ns);
            }

            return (Arr.create<Node>(new VStackNode(nodes)), Arr<Binding>.Empty, unit);
        });

    public static Ui<Unit> HStack(params Ui<Unit>[] items) =>
        new(s =>
        {
            var nodes = new List<Node>();
            foreach (var it in items)
            {
                var (ns, _, _) = it.Run(s);
                nodes.AddRange(ns);
            }

            return (Arr.create<Node>(new HStackNode(nodes)), Arr<Binding>.Empty, unit);
        });

    public static Ui<Unit> Grid(int cols, params Ui<Unit>[] items) =>
        new(s =>
        {
            var nodes = new List<Node>();
            foreach (var it in items)
            {
                var (ns, _, _) = it.Run(s);
                nodes.AddRange(ns);
            }

            return (Arr.create<Node>(new GridNode(cols, nodes)), Arr<Binding>.Empty, unit);
        });

    public static Ui<Unit> Text(string text) => Emit(new TextNode(text));
    public static Ui<Unit> Label(string text) => Emit(new LabelNode(text));
    public static Ui<Unit> Help(string text) => Emit(new HelpNode(text));

    public static Ui<Unit> Key(string current, IReadOnlyList<string> suggested, bool allowFreeText) =>
        new(_ => (Arr.create<Node>(new KeyEditorNode(current, suggested, allowFreeText)), Arr<Binding>.Empty, unit));

    public static FieldBuilder<TModel, TProp> Field<TModel, TProp>(Expression<Func<TModel, TProp>> expr) =>
        FieldBuilder<TModel, TProp>.Create(expr);

    public static Ui<Unit> When<TModel>(Func<TModel, bool> predicate, Ui<Unit> thenUi) =>
        new(_ => (Arr.create<Node>(new ConditionalNode(typeof(TModel), o => predicate((TModel)o), thenUi)), Arr<Binding>.Empty, unit));

    public static Ui<Unit> List<TModel, TItem>(
        Expression<Func<TModel, List<TItem>>> get,
        Func<TItem, string>? itemKey,
        Func<int, Ui<Unit>> itemUi)
        where TItem : class, new() =>
        new(_ =>
        {
            var (getter, setter) = ExprAccessors.Compile(get);
            var id = ExprAccessors.Path(get);
            var binding = new ListBinding(id, typeof(TModel), typeof(TItem), getter, setter, itemKey is null ? null : o => itemKey((TItem)o));
            return (Arr.create<Node>(new ListNode(binding, itemUi)), Arr<Binding>.Empty, unit);
        });

    public static Ui<Unit> Dictionary<TModel, TValue>(
        Expression<Func<TModel, Dictionary<string, TValue>>> get,
        Func<string, Ui<Unit>> keyUi,
        Func<TValue, Ui<Unit>> valueUi)
        where TValue : class, new() =>
        new(_ =>
        {
            var (getter, setter) = ExprAccessors.Compile(get);
            var id = ExprAccessors.Path(get);
            var binding = new DictionaryBinding(id, typeof(TModel), typeof(TValue), getter, setter);
            return (Arr.create<Node>(new DictionaryNode(binding, keyUi, v => valueUi((TValue)v))), Arr<Binding>.Empty, unit);
        });

    public static Ui<Unit> OptionalObject<TModel, TObject>(
        Expression<Func<TModel, TObject?>> get,
        string title,
        Ui<Unit> body,
        bool initiallyExpanded = true,
        bool defaultEnabled = false,
        Func<TObject>? create = null)
        where TObject : class, new() =>
        new(_ =>
        {
            var (getter, setter) = ExprAccessors.Compile(get);
            var id = ExprAccessors.Path(get);
            var binding = new ObjectBinding(
                Id: id,
                ModelType: typeof(TModel),
                ObjectType: typeof(TObject),
                Get: getter,
                Set: setter,
                Create: () => (create is null ? new TObject() : create())
            );

            return (Arr.create<Node>(new OptionalObjectNode(title, binding, body, InitiallyExpanded: initiallyExpanded, DefaultEnabled: defaultEnabled)), Arr<Binding>.Empty, unit);
        });

    public static Ui<FormSpec<TModel>> Form<TModel>(Ui<Unit> body) =>
        new(s =>
        {
            var (nodes, _, _) = body.Run(s);
            return (Arr<Node>.Empty, Arr<Binding>.Empty, new FormSpec<TModel>(ToContainer(nodes), Arr<Binding>.Empty));
        });

    private static Ui<Unit> Emit(Node node) =>
        new(_ => (Arr.create(node), Arr<Binding>.Empty, unit));

    private static Ui<Unit> Wrap(Ui<Unit> body, Func<Node, Node> wrap) =>
        new(s =>
        {
            var (nodes, _, _) = body.Run(s);
            return (Arr.create(wrap(ToContainer(nodes))), Arr<Binding>.Empty, unit);
        });

    private static Node ToContainer(Arr<Node> nodes) =>
        nodes.Count switch
        {
            0 => new VStackNode(global::System.Array.Empty<Node>()),
            1 => nodes[0],
            _ => new VStackNode(nodes.ToArray())
        };
}

internal static class ExprAccessors
{
    public static string Path(LambdaExpression expr)
    {
        var parts = new List<string>();
        Expression? current = expr.Body;
        while (current is MemberExpression m)
        {
            parts.Add(m.Member.Name);
            current = m.Expression;
        }

        parts.Reverse();
        return string.Join(".", parts);
    }

    public static (Func<object, object?> Get, Action<object, object?> Set) Compile<TModel, TProp>(Expression<Func<TModel, TProp>> expr)
    {
        var modelParam = Expression.Parameter(typeof(object), "model");
        var valueParam = Expression.Parameter(typeof(object), "value");

        var typedModel = Expression.Convert(modelParam, typeof(TModel));
        var body = ReplaceParameter(expr.Body, expr.Parameters[0], typedModel);

        var getExpr = Expression.Lambda<Func<object, object?>>(Expression.Convert(body, typeof(object)), modelParam);

        if (body is not MemberExpression member || member.Member is not System.Reflection.PropertyInfo prop || !prop.CanWrite)
        {
            throw new InvalidOperationException($"Expression must be a writable property access: {expr}");
        }

        var convertedValue = Expression.Convert(valueParam, prop.PropertyType);
        var setBody = Expression.Assign(member, convertedValue);
        var setExpr = Expression.Lambda<Action<object, object?>>(setBody, modelParam, valueParam);

        return (getExpr.Compile(), setExpr.Compile());
    }

    private static Expression ReplaceParameter(Expression expression, ParameterExpression source, Expression target) =>
        new ReplaceVisitor(source, target).Visit(expression);

    private sealed class ReplaceVisitor(ParameterExpression source, Expression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == source ? target : base.VisitParameter(node);
    }
}


