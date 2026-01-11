using Common.Ui;
using WinMachine.ConfigUi.WinForms.Rendering;

namespace WinMachine.ConfigUi.WinForms.Rendering;

public sealed class ConditionalManager
{
    private readonly List<ConditionalReg> _regs = new();

    public void Register(Panel panel, ConditionalNode node, object model, object rootModel)
    {
        _regs.Add(new ConditionalReg(panel, node, model, rootModel));
    }

    public void Refresh(Func<Node, object, object, Control> renderNode)
    {
        for (var i = _regs.Count - 1; i >= 0; i--)
        {
            var c = _regs[i];

            if (c.Panel.IsDisposed || c.Panel.Parent is null)
            {
                _regs.RemoveAt(i);
                continue;
            }

            try
            {
                var target = c.Node.ModelType.IsInstanceOfType(c.Model) ? c.Model : c.RootModel;
                var shouldShow = c.Node.Predicate(target);

                UiDebug.Log($"[UI] Conditional refresh: model={c.Model.GetType().Name}, nodeModel={c.Node.ModelType.Name}, show={shouldShow}, built={c.Built}");

                if (shouldShow && !c.Built)
                {
                    c.Built = true;
                    var (nodes, _, _) = c.Node.Body.Run(BuildState.Empty);

                    foreach (var n in nodes)
                    {
                        var child = renderNode(n, target, c.RootModel);
                        child.Dock = DockStyle.Top;
                        c.Panel.Controls.Add(child);
                        c.Panel.Controls.SetChildIndex(child, 0);
                    }
                }

                c.Panel.Visible = shouldShow;
            }
            catch (Exception ex)
            {
                UiDebug.Log(ex, "[UI] Conditional refresh failed");
                // isolate failures: keep going so other conditionals still render
            }
        }
    }

    private sealed record ConditionalReg(Panel Panel, ConditionalNode Node, object Model, object RootModel)
    {
        public bool Built { get; set; }
    }
}
