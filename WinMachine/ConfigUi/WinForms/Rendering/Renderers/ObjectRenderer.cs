using Machine.Framework.Core.Ui;
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

        // 注意：Panel �?AutoSize/PreferredSize �?TableLayoutPanel 中不稳定（尤其搭�?Dock）�?
        // 这里�?TableLayoutPanel/FlowLayoutPanel 作为“可测量”的容器，避�?OptionalObject 头部/整体高度�?0 导致不显示�?
        var container = new TableLayoutPanel
        {
            ColumnCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top
        };
        container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var header = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0)
        };

        var enable = new CheckBox { Text = $"启用 {o.Title}", AutoSize = true };
        var expand = new CheckBox { Text = "展开", AutoSize = true, Checked = o.InitiallyExpanded };
        header.Controls.Add(enable);
        header.Controls.Add(expand);

        var bodyPanel = new TableLayoutPanel
        {
            ColumnCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            Padding = new Padding(16, 6, 6, 6),
            Margin = new Padding(0)
        };
        bodyPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var suppressEvents = false;

        void ClearBody()
        {
            using var _ = WinFormsUiHelpers.Suspend(bodyPanel);
            WinFormsUiHelpers.ClearAndDisposeChildren(bodyPanel);
        }

        void EnsureBody(object target)
        {
            if (bodyPanel.Controls.Count > 0) return;

            var bodyNode = ToBodyNode(o.Body);
            var child = ctx.RenderNode(bodyNode, target, rootModel);
            child.Dock = DockStyle.Top;
            child.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            bodyPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            bodyPanel.Controls.Add(child, 0, 0);
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

        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.Controls.Add(header, 0, 0);
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.Controls.Add(bodyPanel, 0, 1);
        Refresh();

        return container;
    }
}


