using Machine.Framework.Core.Ui;

namespace WinMachine.ConfigUi.WinForms.Rendering.Renderers;

public sealed class FallbackRenderer : INodeRenderer
{
    public bool CanRender(Node node) => true;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel) =>
        new Panel { AutoSize = true };
}


