using Microsoft.Extensions.Options;
using LanguageExt;
using static LanguageExt.Prelude;
using WinMachine.Configuration;

namespace WinMachine.ConfigUi.WinForms;

public sealed class SystemOptionsEditorForm : Form
{
    private readonly IOptions<SystemOptions> _options;

    private readonly WinFormsFormInterpreter _interpreter = new();

    private WinFormsFormInterpreter.RenderedForm? _rendered;
    private SystemOptions? _model;

    public SystemOptionsEditorForm(IOptions<SystemOptions> options)
    {
        _options = options;

        Text = "系统配置";
        Width = 900;
        Height = 700;
        StartPosition = FormStartPosition.CenterParent;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            Padding = new Padding(12),
            FlowDirection = FlowDirection.RightToLeft
        };

        var btnCancel = new Button { Text = "关闭", Width = 100 };
        var btnSave = new Button { Text = "保存", Width = 100 };
        buttons.Controls.Add(btnCancel);
        buttons.Controls.Add(btnSave);

        btnCancel.Click += (_, _) => Close();
        btnSave.Click += (_, _) => Save();

        Controls.Add(buttons);

        Load += (_, _) => BuildUi();
    }

    private void BuildUi()
    {
        _model = Clone(_options.Value);

        // ensure at least one board
        _model.MotionBoards ??= [];
        if (_model.MotionBoards.Count == 0)
        {
            _model.MotionBoards.Add(new MotionBoardOptions());
        }

        var spec = SystemOptionsUi.Spec().Run(BuildState.Empty).Value;
        _rendered = _interpreter.Render(spec, _model);

        var host = new Panel { Dock = DockStyle.Fill };
        host.Controls.Add(_rendered.RootControl);

        // replace previous
        foreach (Control c in Controls.OfType<Panel>().ToArray())
        {
            if (c.Dock == DockStyle.Fill) Controls.Remove(c);
        }

        Controls.Add(host);
        host.BringToFront();
    }

    private void Save()
    {
        if (_rendered is null || _model is null) return;

        var committed = _interpreter.Commit(_rendered, _model);
        committed.Match(
            Succ: saved =>
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                var r = WinFormsFormInterpreter.SaveSystemOptionsToAppSettings(path, sectionName: "System", saved);
                r.Match(
                    Succ: _ =>
                    {
                        MessageBox.Show(this, "已写入 appsettings.json（需要重启应用生效）", "保存成功", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return unit;
                    },
                    Fail: e =>
                    {
                        MessageBox.Show(this, e.ToString(), "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return unit;
                    }
                );

                return unit;
            },
            Fail: e =>
            {
                MessageBox.Show(this, e.ToString(), "校验失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return unit;
            }
        );
    }

    private static SystemOptions Clone(SystemOptions src)
    {
        // 简单 clone：避免直接改 IOptions 的实例
        return new SystemOptions
        {
            UseSimulator = src.UseSimulator,
            MotionBoards = src.MotionBoards?.Select(b => new MotionBoardOptions
            {
                Name = b.Name,
                ControllerType = b.ControllerType,
                DeviceIp = b.DeviceIp,
                DeviceCardNo = b.DeviceCardNo,
                LeadshineInit = b.LeadshineInit
            }).ToList() ?? [],
            AxisMap = src.AxisMap?.ToDictionary(k => k.Key, v => new AxisRefOptions
            {
                Board = v.Value.Board,
                Axis = v.Value.Axis
            }) ?? []
        };
    }
}
