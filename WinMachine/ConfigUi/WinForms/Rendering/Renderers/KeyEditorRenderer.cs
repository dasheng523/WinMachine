using Common.Ui;

namespace WinMachine.ConfigUi.WinForms.Rendering.Renderers;

public sealed class KeyEditorRenderer : INodeRenderer
{
    public bool CanRender(Node node) => node is KeyEditorNode;

    public Control Render(RenderContext ctx, Node node, object model, object rootModel)
    {
        var k = (KeyEditorNode)node;

        if (k.Suggested.Count > 0)
        {
            var cb = new ComboBox
            {
                Width = 120,
                DropDownStyle = k.AllowFreeText ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList
            };
            cb.Items.AddRange(k.Suggested.Cast<object>().ToArray());
            cb.Text = k.Current;
            return cb;
        }

        return new TextBox { Width = 120, Text = k.Current };
    }
}
