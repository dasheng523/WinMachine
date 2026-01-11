using Common.Ui;

namespace WinMachine.ConfigUi.WinForms.Rendering.Renderers;

public sealed class TextNodeRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is TextNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var t = (TextNode)node;
        return new Label { Text = t.Text, AutoSize = true };
    }
}

public sealed class LabelRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is LabelNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var l = (LabelNode)node;
        return new Label { Text = l.Text, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
    }
}

public sealed class HelpRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is HelpNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var h = (HelpNode)node;
        return new Label { Text = h.Text, AutoSize = true, ForeColor = SystemColors.GrayText };
    }
}
