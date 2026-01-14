namespace WinMachine.ConfigUi.WinForms.Rendering;

public static class WinFormsUiHelpers
{
    public static IDisposable Suspend(Control control) => new LayoutSuspender(control);

    public static void ClearAndDisposeChildren(Control parent)
    {
        foreach (Control c in parent.Controls.Cast<Control>().ToArray())
        {
            parent.Controls.Remove(c);
            c.Dispose();
        }
    }

    private sealed class LayoutSuspender : IDisposable
    {
        private readonly Control _control;
        public LayoutSuspender(Control control)
        {
            _control = control;
            _control.SuspendLayout();
        }

        public void Dispose() => _control.ResumeLayout(performLayout: true);
    }
}

public sealed class DoubleBufferedPanel : Panel
{
    public DoubleBufferedPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();
    }
}


