using LanguageExt;

namespace Machine.Framework.Core.Ui;

public sealed record FormSpec<TModel>(Node Root, Arr<Binding> Bindings)
{
    public static FormSpec<TModel> Empty { get; } = new(new VStackNode(global::System.Array.Empty<Node>()), Arr<Binding>.Empty);
}

public abstract record Binding;

public sealed record FieldSpec(
    string Id,
    Type ModelType,
    Type ValueType,
    Func<object, object?> Get,
    Action<object, object?> Set,
    FieldPresentation Presentation,
    Arr<IValidator> Validators
);

public sealed record FieldPresentation(
    FieldKind Kind,
    string? Placeholder = null,
    IReadOnlyList<string>? Options = null,
    Func<object, IReadOnlyList<string>>? OptionsProvider = null
);

public enum FieldKind
{
    Text,
    CheckBox,
    Combo,
    UInt16
}

public interface IValidator
{
    Fin<object?> Validate(object? value);
}

public sealed record FuncValidator(Func<object?, Fin<object?>> F) : IValidator
{
    public Fin<object?> Validate(object? value) => F(value);
}

public sealed record FieldBinding(FieldSpec Spec) : Binding;

public sealed record ListBinding(
    string Id,
    Type ModelType,
    Type ItemType,
    Func<object, object?> Get,
    Action<object, object?> Set,
    Func<object, string>? ItemKey
) : Binding;

public sealed record DictionaryBinding(
    string Id,
    Type ModelType,
    Type ValueType,
    Func<object, object?> Get,
    Action<object, object?> Set
) : Binding;


