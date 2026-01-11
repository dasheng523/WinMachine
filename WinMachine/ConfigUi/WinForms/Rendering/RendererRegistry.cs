using Common.Ui;

namespace WinMachine.ConfigUi.WinForms.Rendering;

public sealed class RendererRegistry
{
    private readonly IReadOnlyList<INodeRenderer> _renderers;

    public RendererRegistry(IEnumerable<INodeRenderer> renderers)
    {
        _renderers = renderers.ToList();
    }

    public INodeRenderer Resolve(Node node)
    {
        foreach (var r in _renderers)
        {
            if (r.CanRender(node)) return r;
        }

        throw new NotSupportedException($"No renderer for node type: {node.GetType().Name}");
    }
}
