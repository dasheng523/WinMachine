using Common.Ui;

namespace WinMachine.ConfigUi.WinForms.Rendering.Renderers;

public sealed class PageRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is PageNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var p = (PageNode)node;

        var body = ctx.RenderNode(p.Body, model, rootModel);

        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        panel.Controls.Add(body);
        body.Dock = DockStyle.Fill;
        return panel;
    }
}

public sealed class ScrollRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is ScrollNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var s = (ScrollNode)node;
        var body = ctx.RenderNode(s.Body, model, rootModel);

        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        panel.Controls.Add(body);
        body.Dock = DockStyle.Top;
        return panel;
    }
}

public sealed class TabsRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is TabsNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var tabs = (TabsNode)node;
        var tc = new TabControl { Dock = DockStyle.Fill };

        foreach (var t in tabs.Tabs)
        {
            var page = new TabPage(t.Title);
            var body = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12) };
            var rendered = ctx.RenderNode(t.Body, model, rootModel);
            body.Controls.Add(rendered);
            rendered.Dock = DockStyle.Top;
            page.Controls.Add(body);
            tc.TabPages.Add(page);
        }

        return tc;
    }
}

public sealed class SectionRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is SectionNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var sec = (SectionNode)node;
        var gb = new GroupBox
        {
            Text = sec.Title,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            Padding = new Padding(10)
        };

        var body = ctx.RenderNode(sec.Body, model, rootModel);
        body.Dock = DockStyle.Top;
        gb.Controls.Add(body);
        return gb;
    }
}

public sealed class SplitRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is SplitNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var sp = (SplitNode)node;
        var sc = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = sp.Orientation == Common.Ui.Orientation.Horizontal
                ? System.Windows.Forms.Orientation.Vertical
                : System.Windows.Forms.Orientation.Horizontal
        };

        sc.Panel1.Controls.Add(ctx.RenderNode(sp.First, model, rootModel));
        sc.Panel2.Controls.Add(ctx.RenderNode(sp.Second, model, rootModel));
        sc.Panel1.Controls[0].Dock = DockStyle.Fill;
        sc.Panel2.Controls[0].Dock = DockStyle.Fill;
        return sc;
    }
}

public sealed class ExpanderRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is ExpanderNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var ex = (ExpanderNode)node;

        var container = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
        var toggle = new CheckBox { Text = ex.Title, Checked = ex.InitiallyExpanded, AutoSize = true, Dock = DockStyle.Top };
        var body = new Panel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            Padding = new Padding(16, 6, 6, 6)
        };

        var inner = ctx.RenderNode(ex.Body, model, rootModel);
        inner.Dock = DockStyle.Top;
        body.Controls.Add(inner);
        body.Visible = toggle.Checked;
        toggle.CheckedChanged += (_, _) => body.Visible = toggle.Checked;

        container.Controls.Add(body);
        container.Controls.Add(toggle);
        return container;
    }
}
