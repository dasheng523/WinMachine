using Common.Ui;

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
        foreach (var c in _regs)
        {
            var target = c.Node.ModelType.IsInstanceOfType(c.Model) ? c.Model : c.RootModel;
            var shouldShow = c.Node.Predicate(target);

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
    }

    private sealed record ConditionalReg(Panel Panel, ConditionalNode Node, object Model, object RootModel)
    {
        public bool Built { get; set; }
    }
}
