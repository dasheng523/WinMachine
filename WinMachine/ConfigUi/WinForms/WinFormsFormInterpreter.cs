using System.Text.Json;
using System.Text.Json.Nodes;
using Common.Ui;
using LanguageExt;
using LanguageExt.Common;
using WinMachine.ConfigUi.WinForms.Rendering;
using WinMachine.ConfigUi.WinForms.Rendering.Renderers;
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
        var conditionals = new ConditionalManager();

        var registry = new RendererRegistry(
        [
            new PageRenderer(),
            new ScrollRenderer(),
            new TabsRenderer(),
            new SectionRenderer(),
            new GridRenderer(),
            new VStackRenderer(),
            new HStackRenderer(),
            new SplitRenderer(),
            new ExpanderRenderer(),
            new TextNodeRenderer(),
            new LabelRenderer(),
            new HelpRenderer(),
            new FieldRenderer(),
            new ConditionalRenderer(),
            new ListRenderer(),
            new DictionaryRenderer(),
            new KeyEditorRenderer(),
            new ObjectRenderer(),
            new OptionalObjectRenderer(),
            new FallbackRenderer()
        ]);

        RenderContext? ctx = null;

        Control RenderNode(Node node, object m, object rm) =>
            registry.Resolve(node).Render(ctx!, node, m, rm);

        void RefreshConditionals() =>
            conditionals.Refresh((n, m, rm) => RenderNode(n, m, rm));

        ctx = new RenderContext(
            bindings,
            dictionaries,
            conditionals,
            (n, m, rm) => RenderNode(n, m, rm),
            RefreshConditionals,
            registry);

        var root = RenderNode(spec.Root, model, model);
        root.Dock = DockStyle.Fill;
        RefreshConditionals();
        return new RenderedForm(root, bindings, dictionaries);
    }

    public Fin<TModel> Commit<TModel>(RenderedForm form, TModel model)
        where TModel : class
    {
        static Fin<Unit> Sequence(IEnumerable<Fin<Unit>> actions) =>
            actions.Aggregate(FinSucc(unit), (Fin<Unit> acc, Fin<Unit> next) => acc.Bind(_ => next));

        Fin<Unit> CommitDictionary(AppliedDictionary d)
        {
            var dictType = typeof(Dictionary<,>).MakeGenericType(typeof(string), d.Binding.ValueType);
            var newDict = (System.Collections.IDictionary)Activator.CreateInstance(dictType)!;

            foreach (var row in d.Rows)
            {
                var key = ReadKeyValue(row.KeyControl);
                if (string.IsNullOrWhiteSpace(key))
                {
                    return FinFail<Unit>(Error.New("AxisMap 的 Key 不能为空"));
                }

                if (newDict.Contains(key))
                {
                    return FinFail<Unit>(Error.New($"AxisMap 的 Key 重复: {key}"));
                }

                newDict[key] = row.Value;
            }

            try
            {
                d.Binding.Set(d.ParentModel, newDict);
                return FinSucc(unit);
            }
            catch (Exception e)
            {
                return FinFail<Unit>(e);
            }
        }

        Fin<Unit> CommitBinding(AppliedBinding b)
        {
            if (b.Control.IsDisposed || b.Control.Parent is null)
            {
                return FinSucc(unit);
            }

            var value = ControlValueCodec.Read(b.Control, b.Spec.Presentation.Kind, b.Spec.ValueType);

            var validated = b.Spec.Validators.Fold(
                FinSucc(value),
                (Fin<object?> acc, IValidator v) => acc.Bind(v.Validate));

            return validated.Bind(v =>
            {
                try
                {
                    b.Spec.Set(b.TargetModel, v);
                    return FinSucc(unit);
                }
                catch (Exception e)
                {
                    return FinFail<Unit>(e);
                }
            });
        }

        var committed = Sequence(form.Dictionaries.Select(CommitDictionary))
            .Bind(_ => Sequence(form.Bindings.Select(CommitBinding)));

        return committed.Map(_ => model);
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

    private static string ReadKeyValue(Control control)
    {
        return control switch
        {
            ComboBox cb => cb.Text ?? string.Empty,
            TextBox tb => tb.Text ?? string.Empty,
            _ => control.Text ?? string.Empty
        };
    }
}
