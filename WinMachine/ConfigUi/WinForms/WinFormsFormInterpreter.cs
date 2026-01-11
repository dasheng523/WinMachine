using System.Text.Json;
using System.Text.Json.Nodes;
using System.Globalization;
using System.Reflection;
using Common.Ui;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace WinMachine.ConfigUi.WinForms;

public sealed class WinFormsFormInterpreter
{
    public sealed record RenderedForm(Control RootControl, IReadOnlyList<AppliedBinding> Bindings, IReadOnlyList<AppliedDictionary> Dictionaries);

    public sealed record AppliedBinding(object TargetModel, FieldSpec Spec, Control Control);

    public sealed record AppliedDictionary(object ParentModel, DictionaryBinding Binding, IReadOnlyList<DictionaryRow> Rows);

    public sealed record DictionaryRow(string OldKey, Control KeyControl, object Value);

    public RenderedForm Render<TModel>(FormSpec<TModel> spec, TModel model)
        where TModel : class
    {
        var bindings = new List<AppliedBinding>();
        var dictionaries = new List<AppliedDictionary>();
        var root = RenderNode(spec.Root, model, rootModel: model, bindings, dictionaries);
        root.Dock = DockStyle.Fill;
        return new RenderedForm(root, bindings, dictionaries);
    }

    public Fin<TModel> Commit<TModel>(RenderedForm form, TModel model)
        where TModel : class
    {
        foreach (var d in form.Dictionaries)
        {
            var dictType = typeof(Dictionary<,>).MakeGenericType(typeof(string), d.Binding.ValueType);
            var newDict = (System.Collections.IDictionary)Activator.CreateInstance(dictType)!;

            foreach (var row in d.Rows)
            {
                var key = ReadKeyValue(row.KeyControl);
                if (string.IsNullOrWhiteSpace(key))
                {
                    return FinFail<TModel>(Error.New("AxisMap 的 Key 不能为空"));
                }

                if (newDict.Contains(key))
                {
                    return FinFail<TModel>(Error.New($"AxisMap 的 Key 重复: {key}"));
                }

                newDict[key] = row.Value;
            }

            try
            {
                d.Binding.Set(d.ParentModel, newDict);
            }
            catch (Exception e)
            {
                return FinFail<TModel>(e);
            }
        }

        foreach (var b in form.Bindings)
        {
            if (b.Control.IsDisposed || b.Control.Parent is null)
            {
                // 典型场景：List/Dictionary 重新渲染后旧控件被移除。
                // 这里跳过，避免提交时访问已销毁控件导致异常。
                continue;
            }

            var value = ReadControlValue(b.Control, b.Spec.Presentation.Kind, b.Spec.ValueType);

            var validated = b.Spec.Validators.Fold(
                FinSucc(value),
                (Fin<object?> acc, IValidator v) => acc.Bind(v.Validate));

            if (validated.IsFail)
            {
                return validated.Match(
                    Succ: _ => FinFail<TModel>(Error.New("校验失败")),
                    Fail: e => FinFail<TModel>(e));
            }

            value = validated.Match(Succ: x => x, Fail: _ => value);

            try
            {
                b.Spec.Set(b.TargetModel, value);
            }
            catch (Exception e)
            {
                return FinFail<TModel>(e);
            }
        }

        return FinSucc(model);
    }

    public static Fin<Unit> SaveSystemOptionsToAppSettings(string appSettingsPath, string sectionName, object options)
    {
        try
        {
            var json = File.ReadAllText(appSettingsPath);
            var root = JsonNode.Parse(json) as JsonObject ?? new JsonObject();

            var obj = JsonSerializer.SerializeToNode(options, new JsonSerializerOptions
            {
                WriteIndented = true
            }) as JsonObject ?? new JsonObject();

            root[sectionName] = obj;

            File.WriteAllText(appSettingsPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            return FinSucc(unit);
        }
        catch (Exception e)
        {
            return FinFail<Unit>(e);
        }
    }

    private Control RenderNode(Node node, object model, object rootModel, List<AppliedBinding> bindings, List<AppliedDictionary> dictionaries)
    {
        return node switch
        {
            PageNode p => WrapPage(p, RenderNode(p.Body, model, rootModel, bindings, dictionaries)),
            ScrollNode s => WrapScroll(RenderNode(s.Body, model, rootModel, bindings, dictionaries)),
            TabsNode t => RenderTabs(t, model, rootModel, bindings, dictionaries),
            SectionNode s => RenderSection(s, model, rootModel, bindings, dictionaries),
            GridNode g => RenderGrid(g, model, rootModel, bindings, dictionaries),
            VStackNode v => RenderStack(v.Children, model, rootModel, bindings, dictionaries, flow: FlowDirection.TopDown),
            HStackNode h => RenderStack(h.Children, model, rootModel, bindings, dictionaries, flow: FlowDirection.LeftToRight),
            SplitNode sp => RenderSplit(sp, model, rootModel, bindings, dictionaries),
            ExpanderNode ex => RenderExpander(ex, model, rootModel, bindings, dictionaries),
            TextNode t => new Label { Text = t.Text, AutoSize = true },
            LabelNode l => new Label { Text = l.Text, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft },
            HelpNode h => new Label { Text = h.Text, AutoSize = true, ForeColor = SystemColors.GrayText },
            FieldNode f => RenderField(f, model, bindings),
            ConditionalNode c => RenderConditional(c, model, rootModel, bindings, dictionaries),
            ListNode l => RenderList(l, model, rootModel, bindings, dictionaries),
            DictionaryNode d => RenderDictionary(d, model, rootModel, bindings, dictionaries),
            KeyEditorNode k => RenderKeyEditor(k),
            ObjectNode o => RenderObject(o, model, rootModel, bindings, dictionaries),
            OptionalObjectNode o => RenderOptionalObject(o, model, rootModel, bindings, dictionaries),
            _ => new Panel { AutoSize = true }
        };
    }

    private Control RenderObject(ObjectNode o, object model, object rootModel, List<AppliedBinding> bindings, List<AppliedDictionary> dictionaries)
    {
        var current = o.Binding.Get(model);
        if (current is null)
        {
            current = o.Binding.Create();
            o.Binding.Set(model, current);
        }

        return RenderExpander(new ExpanderNode(o.Title, ToBodyNode(o.Body), o.InitiallyExpanded), current, rootModel, bindings, dictionaries);
    }

    private Control RenderOptionalObject(OptionalObjectNode o, object model, object rootModel, List<AppliedBinding> bindings, List<AppliedDictionary> dictionaries)
    {
        var container = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };

        var header = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
        var enable = new CheckBox { Text = $"启用 {o.Title}", AutoSize = true };
        var expand = new CheckBox { Text = "展开", AutoSize = true, Checked = o.InitiallyExpanded, Left = 180 };
        header.Controls.Add(enable);
        header.Controls.Add(expand);
        enable.Dock = DockStyle.Left;
        expand.Dock = DockStyle.Left;

        var bodyPanel = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, Padding = new Padding(16, 6, 6, 6) };

        void ClearBody()
        {
            foreach (Control c in bodyPanel.Controls.Cast<Control>().ToArray())
            {
                bodyPanel.Controls.Remove(c);
                c.Dispose();
            }
        }

        void EnsureBody(object target)
        {
            if (bodyPanel.Controls.Count > 0) return;

            var (nodes, _, _) = o.Body.Run(BuildState.Empty);
            foreach (var n in nodes)
            {
                var child = RenderNode(n, target, rootModel, bindings, dictionaries);
                child.Dock = DockStyle.Top;
                bodyPanel.Controls.Add(child);
                bodyPanel.Controls.SetChildIndex(child, 0);
            }
        }

        void Refresh()
        {
            var enabled = o.Binding.Get(model) is not null;
            enable.Checked = enabled;
            expand.Enabled = enabled;
            bodyPanel.Visible = enabled && expand.Checked;
        }

        enable.CheckedChanged += (_, _) =>
        {
            if (enable.Checked)
            {
                var current = o.Binding.Get(model) ?? o.Binding.Create();
                o.Binding.Set(model, current);
                EnsureBody(current);
            }
            else
            {
                o.Binding.Set(model, null);
                ClearBody();
            }

            Refresh();
        };

        expand.CheckedChanged += (_, _) => Refresh();

        // init state
        var init = o.Binding.Get(model);
        if (init is not null)
        {
            EnsureBody(init);
        }

        container.Controls.Add(bodyPanel);
        container.Controls.Add(header);
        Refresh();

        return container;
    }

    private static Node ToBodyNode(Ui<Unit> body)
    {
        var (nodes, _, _) = body.Run(BuildState.Empty);
        return nodes.Count switch
        {
            0 => new VStackNode(global::System.Array.Empty<Node>()),
            1 => nodes[0],
            _ => new VStackNode(nodes.ToArray())
        };
    }

    private static Control WrapPage(PageNode p, Control body)
    {
        // Page 是“页面根容器”，不应自带滚动；滚动由 TabPage/ScrollNode 承担。
        // 否则会形成嵌套滚动条且 TabControl 容易只显示一条。
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        panel.Controls.Add(body);
        body.Dock = DockStyle.Fill;
        return panel;
    }

    private static Control WrapScroll(Control body)
    {
        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        panel.Controls.Add(body);
        body.Dock = DockStyle.Top;
        return panel;
    }

    private Control RenderTabs(TabsNode tabs, object model, object rootModel, List<AppliedBinding> bindings, List<AppliedDictionary> dictionaries)
    {
        var tc = new TabControl { Dock = DockStyle.Fill };
        foreach (var t in tabs.Tabs)
        {
            var page = new TabPage(t.Title);
            var body = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12) };
            var rendered = RenderNode(t.Body, model, rootModel, bindings, dictionaries);
            body.Controls.Add(rendered);
            rendered.Dock = DockStyle.Top;
            page.Controls.Add(body);
            tc.TabPages.Add(page);
        }

        return tc;
    }

    private Control RenderSection(SectionNode sec, object model, object rootModel, List<AppliedBinding> bindings, List<AppliedDictionary> dictionaries)
    {
        var gb = new GroupBox { Text = sec.Title, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, Padding = new Padding(10) };
        var body = RenderNode(sec.Body, model, rootModel, bindings, dictionaries);
        body.Dock = DockStyle.Top;
        gb.Controls.Add(body);
        return gb;
    }

    private Control RenderGrid(GridNode grid, object model, object rootModel, List<AppliedBinding> bindings, List<AppliedDictionary> dictionaries)
    {
        var tlp = new TableLayoutPanel
        {
            ColumnCount = Math.Max(1, grid.Columns),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            GrowStyle = TableLayoutPanelGrowStyle.AddRows
        };

        for (var i = 0; i < tlp.ColumnCount; i++)
        {
            // 注意：TableLayoutPanel 在 AutoSize=true 时，Percent 列宽经常会被计算为 0，
            // 造成“只看到左侧 Label，右侧输入控件像消失/被挤到不可见区域”。
            // 这里统一用 AutoSize，字段控件本身可通过 Anchor/Dock 表现为可拉伸。
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        }

        var r = 0;
        var c = 0;
        foreach (var child in grid.Children)
        {
            var ctrl = RenderNode(child, model, rootModel, bindings, dictionaries);
            ctrl.Margin = new Padding(6);
            ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            // 约定：Grid 的奇数列通常是“字段控件”，尽量允许横向拉伸。
            if (c % 2 == 1)
            {
                ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            }

            tlp.Controls.Add(ctrl, c, r);

            c++;
            if (c >= grid.Columns)
            {
                c = 0;
                r++;
            }
        }

        return tlp;
    }

    private Control RenderStack(IReadOnlyList<Node> children, object model, object rootModel, List<AppliedBinding> bindings, List<AppliedDictionary> dictionaries, FlowDirection flow)
    {
        // FlowLayoutPanel(TopDown) 不会横向拉伸子控件，
        // 在 AutoScroll 容器里容易出现“看似每个 GroupBox 都有滚动条/内容看不全”的错觉。
        // 垂直栈改为 TableLayoutPanel(1列, 100%宽) 以获得更稳定的填充行为。
        if (flow == FlowDirection.TopDown)
        {
            var tlp = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (var i = 0; i < children.Count; i++)
            {
                tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var ctrl = RenderNode(children[i], model, rootModel, bindings, dictionaries);
                ctrl.Margin = new Padding(6);
                ctrl.Dock = DockStyle.Top;
                ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                tlp.Controls.Add(ctrl, 0, i);
            }

            return tlp;
        }

        var flp = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            FlowDirection = flow,
            WrapContents = false
        };

        foreach (var child in children)
        {
            var ctrl = RenderNode(child, model, rootModel, bindings, dictionaries);
            ctrl.Margin = new Padding(6);
            flp.Controls.Add(ctrl);
        }

        return flp;
    }

    private Control RenderSplit(SplitNode sp, object model, object rootModel, List<AppliedBinding> bindings, List<AppliedDictionary> dictionaries)
    {
        var sc = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = sp.Orientation == Common.Ui.Orientation.Horizontal ? System.Windows.Forms.Orientation.Vertical : System.Windows.Forms.Orientation.Horizontal
        };

        sc.Panel1.Controls.Add(RenderNode(sp.First, model, rootModel, bindings, dictionaries));
        sc.Panel2.Controls.Add(RenderNode(sp.Second, model, rootModel, bindings, dictionaries));
        sc.Panel1.Controls[0].Dock = DockStyle.Fill;
        sc.Panel2.Controls[0].Dock = DockStyle.Fill;
        return sc;
    }

    private Control RenderExpander(ExpanderNode ex, object model, object rootModel, List<AppliedBinding> bindings, List<AppliedDictionary> dictionaries)
    {
        var container = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
        var toggle = new CheckBox { Text = ex.Title, Checked = ex.InitiallyExpanded, AutoSize = true, Dock = DockStyle.Top };
        var body = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, Padding = new Padding(16, 6, 6, 6) };
        var inner = RenderNode(ex.Body, model, rootModel, bindings, dictionaries);
        inner.Dock = DockStyle.Top;
        body.Controls.Add(inner);
        body.Visible = toggle.Checked;
        toggle.CheckedChanged += (_, _) => body.Visible = toggle.Checked;

        container.Controls.Add(body);
        container.Controls.Add(toggle);
        return container;
    }

    private static Control RenderField(FieldNode f, object model, List<AppliedBinding> bindings)
    {
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

        if (ctrl is ComboBox cb && spec.Presentation.Options is { Count: > 0 } opts)
        {
            cb.Items.AddRange(opts.Cast<object>().ToArray());
        }

        // init
        var v = spec.Get(model);
        WriteControlValue(ctrl, spec.Presentation.Kind, v);

        bindings.Add(new AppliedBinding(model, spec, ctrl));
        return ctrl;
    }

    private Control RenderConditional(ConditionalNode c, object model, object rootModel, List<AppliedBinding> bindings, List<AppliedDictionary> dictionaries)
    {
        // 条件是针对某个 model 类型；如果当前 model 不是该类型，向上用 rootModel。
        var target = c.ModelType.IsInstanceOfType(model) ? model : rootModel;
        var shouldShow = c.Predicate(target);

        var panel = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
        if (!shouldShow)
        {
            panel.Visible = false;
            return panel;
        }

        var (nodes, bs, _) = c.Body.Run(BuildState.Empty);
        // 注意：这里会重复构建 body 的节点；但这是纯 spec，接受。
        // 绑定由子节点在 RenderNode 时注册。
        foreach (var n in nodes)
        {
            var child = RenderNode(n, target, rootModel, bindings, dictionaries);
            child.Dock = DockStyle.Top;
            panel.Controls.Add(child);
            panel.Controls.SetChildIndex(child, 0);
        }

        return panel;
    }

    private Control RenderList(ListNode list, object model, object rootModel, List<AppliedBinding> bindings, List<AppliedDictionary> dictionaries)
    {
        // 垂直列表不要用 FlowLayoutPanel：它会忽略子控件的 Dock，导致宽度无法传播，
        // 进而出现“右侧输入控件被挤没/看不全”。
        var panel = new Panel
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

        void RenderItems()
        {
            // remove old item panels (keep add button)
            while (panel.Controls.Count > 1) panel.Controls.RemoveAt(1);

            for (var i = 0; i < listObj.Count; i++)
            {
                var index = i;
                var item = listObj[i] ?? Activator.CreateInstance(list.Binding.ItemType)!;
                listObj[i] = item;

                var header = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
                var remove = new Button { Text = "删除", AutoSize = true };
                remove.Click += (_, _) =>
                {
                    if (index >= 0 && index < listObj.Count)
                    {
                        listObj.RemoveAt(index);
                    }
                    RenderItems();
                };

                var bodySpec = list.ItemUi(i);
                var (nodes, _, _) = bodySpec.Run(BuildState.Empty);
                var body = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
                foreach (var n in nodes)
                {
                    var ctrl = RenderNode(n, item, rootModel, bindings, dictionaries);
                    ctrl.Dock = DockStyle.Top;
                    body.Controls.Add(ctrl);
                    body.Controls.SetChildIndex(ctrl, 0);
                }

                header.Controls.Add(remove);
                header.Controls.Add(body);
                remove.Dock = DockStyle.Top;
                body.Dock = DockStyle.Top;

                panel.Controls.Add(header);
            }
        }

        add.Click += (_, _) =>
        {
            var item = Activator.CreateInstance(list.Binding.ItemType)!;
            listObj.Add(item);
            RenderItems();
        };

        RenderItems();
        return panel;
    }

    private Control RenderDictionary(DictionaryNode dict, object model, object rootModel, List<AppliedBinding> bindings, List<AppliedDictionary> dictionaries)
    {
        var panel = new Panel
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

        var rows = new List<DictionaryRow>();

        void RenderRows()
        {
            while (panel.Controls.Count > 1) panel.Controls.RemoveAt(1);
            rows.Clear();

            foreach (System.Collections.DictionaryEntry entry in dictObj)
            {
                var key = (string)entry.Key;
                var value = entry.Value ?? Activator.CreateInstance(dict.Binding.ValueType)!;
                dictObj[key] = value;

                var row = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
                var remove = new Button { Text = "删除", AutoSize = true };

                // key editor
                var (keyNodes, _, _) = dict.KeyUi(key).Run(BuildState.Empty);
                var keyPanel = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
                Control? keyEditor = null;
                foreach (var n in keyNodes)
                {
                    var ctrl = RenderNode(n, model, rootModel, bindings, dictionaries);
                    keyPanel.Controls.Add(ctrl);
                    keyEditor ??= ctrl;
                }

                keyEditor ??= new TextBox { Width = 120, Text = key };

                // value editor
                var (valueNodes, _, _) = dict.ValueUi(value).Run(BuildState.Empty);
                var valuePanel = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top };
                foreach (var n in valueNodes)
                {
                    var ctrl = RenderNode(n, value, rootModel, bindings, dictionaries);
                    ctrl.Dock = DockStyle.Top;
                    valuePanel.Controls.Add(ctrl);
                    valuePanel.Controls.SetChildIndex(ctrl, 0);
                }

                remove.Click += (_, _) =>
                {
                    dictObj.Remove(key);
                    RenderRows();
                };

                row.Controls.Add(remove);
                row.Controls.Add(keyPanel);
                row.Controls.Add(valuePanel);
                remove.Dock = DockStyle.Top;
                keyPanel.Dock = DockStyle.Top;
                valuePanel.Dock = DockStyle.Top;

                panel.Controls.Add(row);

                rows.Add(new DictionaryRow(key, keyEditor, value));
            }
        }

        add.Click += (_, _) =>
        {
            var newKey = "";
            var newVal = Activator.CreateInstance(dict.Binding.ValueType)!;
            dictObj[newKey] = newVal;
            RenderRows();
        };

        RenderRows();

        dictionaries.Add(new AppliedDictionary(model, dict.Binding, rows));
        return panel;
    }

    private static string ReadKeyValue(Control control)
    {
        return control switch
        {
            ComboBox cb => cb.Text ?? string.Empty,
            TextBox tb => tb.Text ?? string.Empty,
            _ => control.Text ?? string.Empty
        };
    }

    private static Control RenderKeyEditor(KeyEditorNode k)
    {
        if (k.Suggested.Count > 0)
        {
            var cb = new ComboBox { Width = 120, DropDownStyle = k.AllowFreeText ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList };
            cb.Items.AddRange(k.Suggested.Cast<object>().ToArray());
            cb.Text = k.Current;
            return cb;
        }

        return new TextBox { Width = 120, Text = k.Current };
    }

    private static object? ReadControlValue(Control control, FieldKind kind, Type valueType)
    {
        object? raw = kind switch
        {
            FieldKind.CheckBox when control is CheckBox cb => cb.Checked,
            FieldKind.Combo when control is ComboBox combo => combo.SelectedItem?.ToString() ?? combo.Text,
            FieldKind.UInt16 when control is NumericUpDown nud => Convert.ToUInt16(nud.Value),
            _ when control is TextBox tb => tb.Text,
            _ => null
        };

        return ConvertToValue(raw, valueType);
    }

    private static object? ConvertToValue(object? raw, Type valueType)
    {
        if (valueType == typeof(object)) return raw;

        // Nullable<T>
        var underlying = Nullable.GetUnderlyingType(valueType);
        if (underlying is not null)
        {
            if (raw is null) return null;
            if (raw is string s && string.IsNullOrWhiteSpace(s)) return null;
            return ConvertToValue(raw, underlying);
        }

        // ref type null
        if (raw is null) return null;
        if (valueType.IsInstanceOfType(raw)) return raw;

        // enums
        if (valueType.IsEnum)
        {
            var text = raw.ToString() ?? string.Empty;
            return Enum.Parse(valueType, text, ignoreCase: true);
        }

        // List<string>/List<ushort> (AutoEditor 简化编辑)
        if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var itemType = valueType.GetGenericArguments()[0];
            var text = raw.ToString() ?? string.Empty;

            if (itemType == typeof(string))
            {
                var parts = text
                    .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .ToList();
                return parts;
            }

            if (itemType == typeof(ushort))
            {
                var parts = text
                    .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .Select(x => Convert.ToUInt16(x, CultureInfo.InvariantCulture))
                    .ToList();
                return parts;
            }
        }

        // numeric
        if (valueType == typeof(string)) return raw.ToString();
        if (valueType == typeof(bool)) return Convert.ToBoolean(raw, CultureInfo.InvariantCulture);
        if (valueType == typeof(byte)) return Convert.ToByte(raw, CultureInfo.InvariantCulture);
        if (valueType == typeof(short)) return Convert.ToInt16(raw, CultureInfo.InvariantCulture);
        if (valueType == typeof(int)) return Convert.ToInt32(raw, CultureInfo.InvariantCulture);
        if (valueType == typeof(long)) return Convert.ToInt64(raw, CultureInfo.InvariantCulture);
        if (valueType == typeof(ushort)) return Convert.ToUInt16(raw, CultureInfo.InvariantCulture);
        if (valueType == typeof(uint)) return Convert.ToUInt32(raw, CultureInfo.InvariantCulture);
        if (valueType == typeof(ulong)) return Convert.ToUInt64(raw, CultureInfo.InvariantCulture);
        if (valueType == typeof(float)) return Convert.ToSingle(raw, CultureInfo.InvariantCulture);
        if (valueType == typeof(double)) return Convert.ToDouble(raw, CultureInfo.InvariantCulture);
        if (valueType == typeof(decimal)) return Convert.ToDecimal(raw, CultureInfo.InvariantCulture);

        return raw;
    }

    private static void WriteControlValue(Control control, FieldKind kind, object? value)
    {
        switch (kind)
        {
            case FieldKind.CheckBox when control is CheckBox cb:
                cb.Checked = value is bool b && b;
                break;
            case FieldKind.Combo when control is ComboBox combo:
                combo.SelectedItem = value?.ToString();
                if (combo.SelectedIndex < 0) combo.Text = value?.ToString() ?? "";
                break;
            case FieldKind.UInt16 when control is NumericUpDown nud:
                nud.Value = value is null ? 0 : Convert.ToDecimal(value);
                break;
            default:
                if (control is TextBox tb)
                {
                    // List<string>/List<ushort> 简化展示
                    if (value is System.Collections.IEnumerable e && value is not string)
                    {
                        var list = new List<string>();
                        foreach (var x in e) list.Add(x?.ToString() ?? "");
                        tb.Text = string.Join(",", list);
                    }
                    else
                    {
                        tb.Text = value?.ToString() ?? "";
                    }
                }
                break;
        }
    }
}
