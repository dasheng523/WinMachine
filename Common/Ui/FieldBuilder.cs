using System.Linq.Expressions;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Common.Ui;

public sealed class FieldBuilder<TModel, TProp>
{
    private readonly Expression<Func<TModel, TProp>> _expr;
    private FieldPresentation _presentation = new(FieldKind.Text);
    private Arr<IValidator> _validators = Arr<IValidator>.Empty;

    private FieldBuilder(Expression<Func<TModel, TProp>> expr)
    {
        _expr = expr;
    }

    public static FieldBuilder<TModel, TProp> Create(Expression<Func<TModel, TProp>> expr) => new(expr);

    public FieldBuilder<TModel, TProp> AsTextBox(string? placeholder = null)
    {
        _presentation = new FieldPresentation(FieldKind.Text, Placeholder: placeholder);
        return this;
    }

    public FieldBuilder<TModel, TProp> AsCheckBox()
    {
        _presentation = new FieldPresentation(FieldKind.CheckBox);
        return this;
    }

    public FieldBuilder<TModel, TProp> AsCombo(IReadOnlyList<string> options)
    {
        _presentation = new FieldPresentation(FieldKind.Combo, Options: options);
        return this;
    }

    public FieldBuilder<TModel, TProp> AsCombo(Func<object, IReadOnlyList<string>> optionsProvider)
    {
        _presentation = new FieldPresentation(FieldKind.Combo, OptionsProvider: optionsProvider);
        return this;
    }

    public FieldBuilder<TModel, TProp> AsUInt16()
    {
        _presentation = new FieldPresentation(FieldKind.UInt16);
        return this;
    }

    public FieldBuilder<TModel, TProp> Validate(Func<TProp, Fin<TProp>> validator)
    {
        _validators = _validators.Add(new FuncValidator(v =>
        {
            try
            {
                var cast = v is null ? default! : (TProp)v;
                return validator(cast).Map(x => (object?)x);
            }
            catch (Exception e)
            {
                return FinFail<object?>(e);
            }
        }));

        return this;
    }

    public FieldBuilder<TModel, TProp> Validate(Func<TProp, Fin<Unit>> validator)
    {
        _validators = _validators.Add(new FuncValidator(v =>
        {
            try
            {
                var cast = v is null ? default! : (TProp)v;
                return validator(cast).Map(_ => v);
            }
            catch (Exception e)
            {
                return FinFail<object?>(e);
            }
        }));

        return this;
    }

    public Ui<Unit> ToUi()
    {
        var (get, set) = ExprAccessors.Compile(_expr);
        var id = ExprAccessors.Path(_expr);
        var spec = new FieldSpec(
            Id: id,
            ModelType: typeof(TModel),
            ValueType: typeof(TProp),
            Get: get,
            Set: set,
            Presentation: _presentation,
            Validators: _validators
        );

        return new Ui<Unit>(_ => (Arr.create<Node>(new FieldNode(spec)), Arr<Binding>.Empty, unit));
    }

    public static implicit operator Ui<Unit>(FieldBuilder<TModel, TProp> b) => b.ToUi();
}
