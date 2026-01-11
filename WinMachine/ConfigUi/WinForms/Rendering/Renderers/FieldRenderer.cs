using Common.Ui;
using WinMachine.ConfigUi.WinForms.Rendering;

namespace WinMachine.ConfigUi.WinForms.Rendering.Renderers;

public sealed class FieldRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is FieldNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var f = (FieldNode)node;
        var spec = f.Spec;

        Control ctrl = spec.Presentation.Kind switch
        {
            FieldKind.CheckBox => new CheckBox { AutoSize = true },
            FieldKind.Combo => new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 },
            FieldKind.UInt16 => new NumericUpDown { Minimum = 0, Maximum = ushort.MaxValue, Width = 120 },
            _ => new TextBox { Width = 220 }
        };

        if (ctrl is TextBox tb && spec.Presentation.Placeholder is { Length: > 0 })
        {
            tb.PlaceholderText = spec.Presentation.Placeholder;
        }

        if (ctrl is ComboBox cb)
        {
            var opts = spec.Presentation.OptionsProvider?.Invoke(rootModel) ?? spec.Presentation.Options;
            if (opts is { Count: > 0 })
            {
                cb.Items.AddRange(opts.Cast<object>().ToArray());
            }
        }

        var v = spec.Get(model);
        ControlValueCodec.Write(ctrl, spec.Presentation.Kind, v);

        void Sync()
        {
            try
            {
                var value = ControlValueCodec.Read(ctrl, spec.Presentation.Kind, spec.ValueType);
                spec.Set(model, value);
            }
            catch
            {
                // ignore; validation happens on Save
            }

            ctx.RefreshConditionals();
        }

        switch (ctrl)
        {
            case TextBox tb2:
                tb2.TextChanged += (_, _) => Sync();
                break;
            case CheckBox chk:
                chk.CheckedChanged += (_, _) => Sync();
                break;
            case ComboBox combo:
                combo.SelectedIndexChanged += (_, _) => Sync();
                combo.TextChanged += (_, _) => Sync();
                break;
            case NumericUpDown nud:
                nud.ValueChanged += (_, _) => Sync();
                break;
        }

        ctx.Bindings.Add(new WinFormsFormInterpreter.AppliedBinding(model, spec, ctrl));
        return ctrl;
    }
}
