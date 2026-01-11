using Common.Ui;
using WinMachine.ConfigUi.WinForms.Rendering;

namespace WinMachine.ConfigUi.WinForms.Rendering.Renderers;

public sealed class DictionaryRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is DictionaryNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var dict = (DictionaryNode)node;

        var panel = new DoubleBufferedPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top
        };

        var add = new Button { Text = "添加", AutoSize = true, Dock = DockStyle.Top };
        panel.Controls.Add(add);

        var dictObj = dict.Binding.Get(model) as System.Collections.IDictionary;
        if (dictObj is null)
        {
            var t = typeof(Dictionary<,>).MakeGenericType(typeof(string), dict.Binding.ValueType);
            dictObj = (System.Collections.IDictionary)Activator.CreateInstance(t)!;
            dict.Binding.Set(model, dictObj);
        }

        var rows = new List<WinFormsFormInterpreter.DictionaryRow>();

        var rowPanels = new Dictionary<string, Control>();

        void PruneDeadBindings()
        {
            ctx.Bindings.RemoveAll(b => b.Control.IsDisposed || b.Control.Parent is null);
        }

        (Control RowPanel, WinFormsFormInterpreter.DictionaryRow Row) CreateRow(string key, object value)
        {
            var row = new DoubleBufferedPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
            var remove = new Button { Text = "删除", AutoSize = true };

            var (keyNodes, _, _) = dict.KeyUi(key).Run(BuildState.Empty);
            var keyPanel = new DoubleBufferedPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
            Control? keyEditor = null;
            foreach (var n in keyNodes)
            {
                var ctrl = ctx.RenderNode(n, model, rootModel);
                keyPanel.Controls.Add(ctrl);
                keyEditor ??= ctrl;
            }

            keyEditor ??= new TextBox { Width = 120, Text = key };

            var (valueNodes, _, _) = dict.ValueUi(value).Run(BuildState.Empty);
            var valuePanel = new DoubleBufferedPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
            foreach (var n in valueNodes)
            {
                var ctrl = ctx.RenderNode(n, value, rootModel);
                ctrl.Dock = DockStyle.Top;
                valuePanel.Controls.Add(ctrl);
                valuePanel.Controls.SetChildIndex(ctrl, 0);
            }

            remove.Click += (_, _) =>
            {
                if (!dictObj.Contains(key)) return;

                dictObj.Remove(key);

                using var _ = WinFormsUiHelpers.Suspend(panel);
                PruneDeadBindings();

                if (rowPanels.TryGetValue(key, out var rowPanel))
                {
                    rowPanels.Remove(key);
                    panel.Controls.Remove(rowPanel);
                    rowPanel.Dispose();
                }

                rows.RemoveAll(r => r.OldKey == key);
                ctx.RefreshConditionals();
            };

            row.Controls.Add(remove);
            row.Controls.Add(keyPanel);
            row.Controls.Add(valuePanel);
            remove.Dock = DockStyle.Top;
            keyPanel.Dock = DockStyle.Top;
            valuePanel.Dock = DockStyle.Top;

            return (row, new WinFormsFormInterpreter.DictionaryRow(key, keyEditor, value));
        }

        void BuildInitialRows()
        {
            using var _ = WinFormsUiHelpers.Suspend(panel);
            rows.Clear();
            rowPanels.Clear();

            foreach (System.Collections.DictionaryEntry entry in dictObj)
            {
                var key = (string)entry.Key;
                var value = entry.Value ?? Activator.CreateInstance(dict.Binding.ValueType)!;
                dictObj[key] = value;

                var (rowPanel, row) = CreateRow(key, value);
                panel.Controls.Add(rowPanel);
                rowPanels[key] = rowPanel;
                rows.Add(row);
            }
        }

        add.Click += (_, _) =>
        {
            var baseKey = "NewKey";
            var newKey = baseKey;
            var i = 1;
            while (dictObj.Contains(newKey))
            {
                newKey = $"{baseKey}{i++}";
            }

            var newVal = Activator.CreateInstance(dict.Binding.ValueType)!;
            dictObj[newKey] = newVal;

            using var _ = WinFormsUiHelpers.Suspend(panel);
            PruneDeadBindings();

            var (rowPanel, row) = CreateRow(newKey, newVal);
            panel.Controls.Add(rowPanel);
            rowPanels[newKey] = rowPanel;
            rows.Add(row);

            ctx.RefreshConditionals();
        };

        BuildInitialRows();

        ctx.Dictionaries.Add(new WinFormsFormInterpreter.AppliedDictionary(model, dict.Binding, rows));
        return panel;
    }
}
