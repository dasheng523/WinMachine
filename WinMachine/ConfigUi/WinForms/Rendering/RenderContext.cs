using Common.Ui;

namespace WinMachine.ConfigUi.WinForms.Rendering;

public sealed record RenderContext(
    List<WinFormsFormInterpreter.AppliedBinding> Bindings,
    List<WinFormsFormInterpreter.AppliedDictionary> Dictionaries,
    ConditionalManager Conditionals,
    Func<Node, object, object, Control> Render,
    Action RefreshConditionals,
    RendererRegistry Registry)
{
    public Control RenderNode(Node node, object model, object rootModel) =>
        Registry.Resolve(node).Render(this, node, model, rootModel);
}
