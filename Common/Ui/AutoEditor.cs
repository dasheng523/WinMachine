using System.Reflection;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Common.Ui;

internal static class AutoEditorBuilder
{
    public static Node Build(Type modelType) =>
        new VStackNode(BuildChildren(modelType, idPrefix: modelType.Name));

    private static IReadOnlyList<Node> BuildChildren(Type modelType, string idPrefix)
    {
        var nodes = new List<Node>();

        // 一个对象默认用 2 列 Grid（Label + Editor）
        var gridChildren = new List<Node>();

        foreach (var prop in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            if (prop.GetIndexParameters().Length != 0) continue;

            var propType = prop.PropertyType;
            var (isNullable, underlying) = UnwrapNullable(propType);
            var effectiveType = underlying;

            // 1) 标量/枚举
            if (TryCreateScalarField(modelType, prop, effectiveType, idPrefix, out var label, out var field))
            {
                gridChildren.Add(label);
                gridChildren.Add(field);
                continue;
            }

            // 2) List<T>
            if (TryCreateListEditor(modelType, prop, propType, idPrefix, out var listLabel, out var listNode))
            {
                nodes.Add(new SectionNode(prop.Name, listNode));
                continue;
            }

            // 3) 复杂对象（可选/非可选）
            if (effectiveType.IsClass && effectiveType != typeof(string))
            {
                var body = new Ui<Unit>(_ => (Arr.create<Node>(Build(effectiveType)), Arr<Binding>.Empty, unit));

                if (isNullable)
                {
                    var binding = CreateObjectBinding(modelType, prop, idPrefix);
                    nodes.Add(new OptionalObjectNode(prop.Name, binding, body, InitiallyExpanded: false));
                }
                else
                {
                    // 非空对象：用 Expander 包一下，默认折叠
                    // 注意：若运行时真的为 null，解释器会补一个 new() 实例。
                    var binding = CreateObjectBinding(modelType, prop, idPrefix);
                    nodes.Add(new ObjectNode(prop.Name, binding, body, InitiallyExpanded: false));
                }

                continue;
            }

            // fallback：把未知类型当成字符串
            var fallbackLabel = new LabelNode(prop.Name);
            var fallbackField = new FieldNode(new FieldSpec(
                Id: $"{idPrefix}.{prop.Name}",
                ModelType: modelType,
                ValueType: typeof(string),
                Get: o => prop.GetValue(o)?.ToString(),
                Set: (o, v) => prop.SetValue(o, v?.ToString()),
                Presentation: new FieldPresentation(FieldKind.Text),
                Validators: Arr<IValidator>.Empty
            ));
            gridChildren.Add(fallbackLabel);
            gridChildren.Add(fallbackField);
        }

        if (gridChildren.Count > 0)
        {
            nodes.Insert(0, new GridNode(2, gridChildren));
        }

        return nodes;
    }

    private static bool TryCreateScalarField(Type modelType, PropertyInfo prop, Type effectiveType, string idPrefix, out Node label, out Node field)
    {
        label = new LabelNode(prop.Name);
        field = new FieldNode(new FieldSpec(
            Id: $"{idPrefix}.{prop.Name}",
            ModelType: modelType,
            ValueType: prop.PropertyType,
            Get: o => prop.GetValue(o),
            Set: (o, v) => prop.SetValue(o, v),
            Presentation: GuessPresentation(prop.PropertyType),
            Validators: Arr<IValidator>.Empty
        ));

        if (effectiveType == typeof(bool)) return true;
        if (effectiveType == typeof(string)) return true;
        if (effectiveType.IsEnum) return true;

        if (effectiveType == typeof(byte) || effectiveType == typeof(short) || effectiveType == typeof(int) || effectiveType == typeof(long) ||
            effectiveType == typeof(ushort) || effectiveType == typeof(uint) || effectiveType == typeof(ulong) ||
            effectiveType == typeof(float) || effectiveType == typeof(double) || effectiveType == typeof(decimal))
        {
            return true;
        }

        return false;
    }

    private static bool TryCreateListEditor(Type modelType, PropertyInfo prop, Type propType, string idPrefix, out Node label, out Node listNode)
    {
        label = new LabelNode(prop.Name);
        listNode = new TextNode("");

        if (!propType.IsGenericType) return false;
        if (propType.GetGenericTypeDefinition() != typeof(List<>)) return false;

        var itemType = propType.GetGenericArguments()[0];

        // List<string>/List<ushort>：先用一个文本框做简单编辑（逗号/换行分隔）
        if (itemType == typeof(string) || itemType == typeof(ushort))
        {
            listNode = new GridNode(2, new Node[]
            {
                new LabelNode(prop.Name),
                new FieldNode(new FieldSpec(
                    Id: $"{idPrefix}.{prop.Name}",
                    ModelType: modelType,
                    ValueType: propType,
                    Get: o => prop.GetValue(o),
                    Set: (o, v) => prop.SetValue(o, v),
                    Presentation: new FieldPresentation(FieldKind.Text, Placeholder: itemType == typeof(string) ? "用逗号或换行分隔" : "用逗号分隔(如 0,1,2)"),
                    Validators: Arr<IValidator>.Empty
                ))
            });
            return true;
        }

        if (!itemType.IsClass) return false;

        // List<复杂对象>
        var getter = new Func<object, object?>(o => prop.GetValue(o));
        var setter = new Action<object, object?>((o, v) => prop.SetValue(o, v));

        Func<int, Ui<Unit>> itemUi = i =>
            new(_ => (Arr.create<Node>(new SectionNode($"Item #{i}", Build(itemType))), Arr<Binding>.Empty, unit));

        var binding = new ListBinding(
            Id: $"{idPrefix}.{prop.Name}",
            ModelType: modelType,
            ItemType: itemType,
            Get: getter,
            Set: setter,
            ItemKey: null
        );

        listNode = new ListNode(binding, itemUi);
        return true;
    }

    private static ObjectBinding CreateObjectBinding(Type modelType, PropertyInfo prop, string idPrefix)
    {
        var propType = prop.PropertyType;
        var (_, underlying) = UnwrapNullable(propType);
        var objectType = underlying;

        Func<object> create = () =>
        {
            var ctor = objectType.GetConstructor(Type.EmptyTypes);
            if (ctor is null)
            {
                throw new InvalidOperationException($"Type {objectType.FullName} requires a parameterless constructor for auto editor.");
            }

            return Activator.CreateInstance(objectType)!;
        };

        return new ObjectBinding(
            Id: $"{idPrefix}.{prop.Name}",
            ModelType: modelType,
            ObjectType: objectType,
            Get: o => prop.GetValue(o),
            Set: (o, v) => prop.SetValue(o, v),
            Create: create
        );
    }

    private static FieldPresentation GuessPresentation(Type type)
    {
        var (isNullable, underlying) = UnwrapNullable(type);
        var effective = underlying;

        if (effective == typeof(bool)) return new FieldPresentation(FieldKind.CheckBox);
        if (effective == typeof(ushort)) return new FieldPresentation(FieldKind.UInt16);

        if (effective.IsEnum)
        {
            var names = Enum.GetNames(effective);
            return new FieldPresentation(FieldKind.Combo, Options: names);
        }

        // 默认 Text
        return new FieldPresentation(FieldKind.Text);
    }

    private static (bool IsNullable, Type Underlying) UnwrapNullable(Type t)
    {
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return (true, Nullable.GetUnderlyingType(t)!);
        }

        if (!t.IsValueType) return (true, t); // reference type: treat as nullable

        return (false, t);
    }
}
