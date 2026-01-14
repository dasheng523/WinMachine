using LanguageExt;
using static LanguageExt.Prelude;

namespace Machine.Framework.Core.Ui;

public readonly record struct Ui<A>(Func<BuildState, (Arr<Node> Nodes, Arr<Binding> Bindings, A Value)> Run)
{
    public static Ui<A> Pure(A value) => new(_ => (Arr<Node>.Empty, Arr<Binding>.Empty, value));

    public Ui<B> Select<B>(Func<A, B> f) =>
        CaptureRun(Run, f);

    private static Ui<B> CaptureRun<B>(
        Func<BuildState, (Arr<Node> Nodes, Arr<Binding> Bindings, A Value)> run,
        Func<A, B> f) =>
        new(s =>
        {
            var (nodes, bindings, a) = run(s);
            return (nodes, bindings, f(a));
        });

    public Ui<B> SelectMany<B>(Func<A, Ui<B>> bind) =>
        CaptureRun(Run, bind);

    private static Ui<B> CaptureRun<B>(
        Func<BuildState, (Arr<Node> Nodes, Arr<Binding> Bindings, A Value)> run,
        Func<A, Ui<B>> bind) =>
        new(s =>
        {
            var (nodes1, bindings1, a) = run(s);
            var (nodes2, bindings2, b) = bind(a).Run(s);
            return (nodes1 + nodes2, bindings1 + bindings2, b);
        });

    public Ui<C> SelectMany<B, C>(Func<A, Ui<B>> bind, Func<A, B, C> project) =>
        SelectMany(a => bind(a).Select(b => project(a, b)));
}

public readonly record struct BuildState
{
    public static readonly BuildState Empty = new();
}

public static class Ui
{
    public static Ui<Unit> Unit { get; } = Ui<Unit>.Pure(unit);
}


