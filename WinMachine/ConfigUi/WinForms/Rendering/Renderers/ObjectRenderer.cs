using Common.Ui;
using LanguageExt;
using static LanguageExt.Prelude;
using WinMachine.ConfigUi.WinForms.Rendering;

namespace WinMachine.ConfigUi.WinForms.Rendering.Renderers;

public sealed class ObjectRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is ObjectNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var o = (ObjectNode)node;

        var current = o.Binding.Get(model);
        if (current is null)
        {
            current = o.Binding.Create();
            o.Binding.Set(model, current);
        }

        var expanderNode = new ExpanderNode(o.Title, ToBodyNode(o.Body), o.InitiallyExpanded);
        return ctx.RenderNode(expanderNode, current, rootModel);
    }

    private static Node ToBodyNode(Ui<Unit> body)
    {
        var (nodes, _, _) = body.Run(BuildState.Empty);
        return nodes.Count switch
        {
            0 => new VStackNode(global::System.Array.Empty<Node>()),
            1 => nodes[0],
            _ => new VStackNode(nodes.ToArray())
        };
    }
}

public sealed class OptionalObjectRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is OptionalObjectNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var o = (OptionalObjectNode)node;

        var currentValue = o.Binding.Get(model);
        if (o.DefaultEnabled && currentValue is null)
        {
            currentValue = o.Binding.Create();
            o.Binding.Set(model, currentValue);
            UiDebug.Log($"[UI] OptionalObject auto-enable: title={o.Title}, model={model.GetType().Name}");
        }

        UiDebug.Log($"[UI] OptionalObject render: title={o.Title}, model={model.GetType().Name}, hasValue={currentValue is not null}");

        var container = new DoubleBufferedPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };

        var header = new DoubleBufferedPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
        var enable = new CheckBox { Text = $"启用 {o.Title}", AutoSize = true };
        var expand = new CheckBox { Text = "展开", AutoSize = true, Checked = o.InitiallyExpanded, Left = 180 };
        header.Controls.Add(enable);
        header.Controls.Add(expand);
        enable.Dock = DockStyle.Left;
        expand.Dock = DockStyle.Left;

        var bodyPanel = new DoubleBufferedPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            Padding = new Padding(16, 6, 6, 6)
        };

        var suppressEvents = false;

        void ClearBody()
        {
            using var _ = WinFormsUiHelpers.Suspend(bodyPanel);
            WinFormsUiHelpers.ClearAndDisposeChildren(bodyPanel);
        }

        void EnsureBody(object target)
        {
            if (bodyPanel.Controls.Count > 0) return;

            var (nodes, _, _) = o.Body.Run(BuildState.Empty);
            foreach (var n in nodes)
            {
                var child = ctx.RenderNode(n, target, rootModel);
                child.Dock = DockStyle.Top;
                bodyPanel.Controls.Add(child);
                bodyPanel.Controls.SetChildIndex(child, 0);
            }
        }

        void Refresh()
        {
            var enabled = o.Binding.Get(model) is not null;

            suppressEvents = true;
            try
            {
                enable.Checked = enabled;
                expand.Enabled = enabled;
                bodyPanel.Visible = enabled && expand.Checked;
            }
            finally
            {
                suppressEvents = false;
            }
        }

        enable.CheckedChanged += (_, _) =>
        {
            if (suppressEvents) return;

            if (enable.Checked)
            {
                var current = o.Binding.Get(model) ?? o.Binding.Create();
                o.Binding.Set(model, current);
                EnsureBody(current);
            }
            else
            {
                o.Binding.Set(model, null);
                ClearBody();
            }

            Refresh();
            ctx.RefreshConditionals();
        };

        expand.CheckedChanged += (_, _) =>
        {
            if (suppressEvents) return;
            Refresh();
        };

        if (currentValue is not null)
        {
            EnsureBody(currentValue);
        }

        container.Controls.Add(bodyPanel);
        container.Controls.Add(header);
        Refresh();

        return container;
    }
}
