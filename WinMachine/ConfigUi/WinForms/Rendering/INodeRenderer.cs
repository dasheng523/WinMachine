using Machine.Framework.Core.Ui;

namespace WinMachine.ConfigUi.WinForms.Rendering;

public interface INodeRenderer
{
    bool CanRender(Node node);

    Control Render(RenderContext ctx, Node node, object model, object rootModel);
}


