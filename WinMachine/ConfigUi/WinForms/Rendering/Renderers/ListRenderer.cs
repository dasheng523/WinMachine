using Machine.Framework.Core.Ui;
using WinMachine.ConfigUi.WinForms.Rendering;

namespace WinMachine.ConfigUi.WinForms.Rendering.Renderers;

public sealed class ListRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is ListNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var list = (ListNode)node;

        var panel = new DoubleBufferedPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top
        };

        var add = new Button { Text = "添加", AutoSize = true, Dock = DockStyle.Top };
        panel.Controls.Add(add);

        var listObj = list.Binding.Get(model) as System.Collections.IList;
        if (listObj is null)
        {
            listObj = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(list.Binding.ItemType))!;
            list.Binding.Set(model, listObj);
        }

        var itemPanels = new List<Control>();

        void PruneDeadBindings()
        {
            ctx.Bindings.RemoveAll(b => b.Control.IsDisposed || b.Control.Parent is null);
        }

        Control CreateItemPanel(int index, object item)
        {
            var header = new DoubleBufferedPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
            var remove = new Button { Text = "删除", AutoSize = true };

            remove.Click += (_, _) =>
            {
                var idx = listObj.IndexOf(item);
                if (idx < 0 || idx >= listObj.Count) return;

                listObj.RemoveAt(idx);

                using var _ = WinFormsUiHelpers.Suspend(panel);
                PruneDeadBindings();

                // remove the panel that corresponds to the removed item
                if (idx < itemPanels.Count)
                {
                    var removedPanel = itemPanels[idx];
                    itemPanels.RemoveAt(idx);
                    panel.Controls.Remove(removedPanel);
                    removedPanel.Dispose();
                }

                // indices after idx have changed; rebuild tail to keep ItemUi(index) consistent
                RebuildFrom(idx);
                ctx.RefreshConditionals();
            };

            var bodySpec = list.ItemUi(index);
            var (nodes, _, _) = bodySpec.Run(BuildState.Empty);
            var body = new DoubleBufferedPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };

            foreach (var n in nodes)
            {
                var ctrl = ctx.RenderNode(n, item, rootModel);
                ctrl.Dock = DockStyle.Top;
                body.Controls.Add(ctrl);
                body.Controls.SetChildIndex(ctrl, 0);
            }

            header.Controls.Add(remove);
            header.Controls.Add(body);
            remove.Dock = DockStyle.Top;
            body.Dock = DockStyle.Top;
            return header;
        }

        void RebuildFrom(int startIndex)
        {
            startIndex = Math.Max(0, startIndex);
            if (startIndex > listObj.Count) startIndex = listObj.Count;

            // dispose existing panels from startIndex..
            for (var i = itemPanels.Count - 1; i >= startIndex; i--)
            {
                var p = itemPanels[i];
                itemPanels.RemoveAt(i);
                panel.Controls.Remove(p);
                p.Dispose();
            }

            // re-create panels from startIndex..
            for (var i = startIndex; i < listObj.Count; i++)
            {
                var item = listObj[i] ?? Activator.CreateInstance(list.Binding.ItemType)!;
                listObj[i] = item;

                var itemPanel = CreateItemPanel(i, item);
                itemPanels.Add(itemPanel);
                panel.Controls.Add(itemPanel);
            }
        }

        add.Click += (_, _) =>
        {
            var item = Activator.CreateInstance(list.Binding.ItemType)!;
            listObj.Add(item);

            using var _ = WinFormsUiHelpers.Suspend(panel);
            PruneDeadBindings();

            var index = listObj.Count - 1;
            var itemPanel = CreateItemPanel(index, item);
            itemPanels.Add(itemPanel);
            panel.Controls.Add(itemPanel);

            ctx.RefreshConditionals();
        };

        using (WinFormsUiHelpers.Suspend(panel))
        {
            RebuildFrom(0);
        }
        return panel;
    }
}


