using Common.Ui;

namespace WinMachine.ConfigUi.WinForms.Rendering.Renderers;

public sealed class GridRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is GridNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var grid = (GridNode)node;

        var tlp = new TableLayoutPanel
        {
            ColumnCount = Math.Max(1, grid.Columns),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            GrowStyle = TableLayoutPanelGrowStyle.AddRows
        };

        for (var i = 0; i < tlp.ColumnCount; i++)
        {
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        }

        static bool IsFullRowNode(Node n) =>
            n is not LabelNode
            && n is not FieldNode
            && n is not HelpNode
            && n is not TextNode;

        var r = 0;
        var c = 0;
        foreach (var child in grid.Children)
        {
            var ctrl = ctx.RenderNode(child, model, rootModel);
            ctrl.Margin = new Padding(6);
            ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            if (IsFullRowNode(child))
            {
                // 对“块级节点”做跨列展示：先对齐到新行，再占满全部列。
                if (c != 0)
                {
                    c = 0;
                    r++;
                }

                ctrl.Dock = DockStyle.Top;
                ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                tlp.Controls.Add(ctrl, 0, r);
                tlp.SetColumnSpan(ctrl, tlp.ColumnCount);

                r++;
                c = 0;
                continue;
            }

            if (c % 2 == 1)
            {
                ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            }

            tlp.Controls.Add(ctrl, c, r);

            c++;
            if (c >= grid.Columns)
            {
                c = 0;
                r++;
            }
        }

        return tlp;
    }
}

public sealed class VStackRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is VStackNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel) =>
        StackRenderer.RenderStack(ctx, ((VStackNode)node).Children, model, rootModel, flow: FlowDirection.TopDown);
}

public sealed class HStackRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is HStackNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel) =>
        StackRenderer.RenderStack(ctx, ((HStackNode)node).Children, model, rootModel, flow: FlowDirection.LeftToRight);
}

internal static class StackRenderer
{
    public static Control RenderStack(RenderContext ctx, IReadOnlyList<Node> children, object model, object rootModel, FlowDirection flow)
    {
        if (flow == FlowDirection.TopDown)
        {
            var tlp = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (var i = 0; i < children.Count; i++)
            {
                tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var ctrl = ctx.RenderNode(children[i], model, rootModel);
                ctrl.Margin = new Padding(6);
                ctrl.Dock = DockStyle.Top;
                ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                tlp.Controls.Add(ctrl, 0, i);
            }

            return tlp;
        }

        var flp = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            FlowDirection = flow,
            WrapContents = false
        };

        foreach (var child in children)
        {
            var ctrl = ctx.RenderNode(child, model, rootModel);
            ctrl.Margin = new Padding(6);
            flp.Controls.Add(ctrl);
        }

        return flp;
    }
}
