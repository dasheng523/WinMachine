using Common.Ui;

namespace WinMachine.ConfigUi.WinForms.Rendering.Renderers;

public sealed class ConditionalRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is ConditionalNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var c = (ConditionalNode)node;
        var panel = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };

        ctx.Conditionals.Register(panel, c, model, rootModel);
        panel.Visible = false;

        return panel;
    }
}
