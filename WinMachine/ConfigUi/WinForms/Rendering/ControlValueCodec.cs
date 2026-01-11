using System.Globalization;
using Common.Ui;

namespace WinMachine.ConfigUi.WinForms.Rendering;

public static class ControlValueCodec
{
    public static object? Read(Control control, FieldKind kind, Type valueType)
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

    public static void Write(Control control, FieldKind kind, object? value)
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

    private static object? ConvertToValue(object? raw, Type valueType)
    {
        if (valueType == typeof(object)) return raw;

        var underlying = Nullable.GetUnderlyingType(valueType);
        if (underlying is not null)
        {
            if (raw is null) return null;
            if (raw is string s && string.IsNullOrWhiteSpace(s)) return null;
            return ConvertToValue(raw, underlying);
        }

        if (raw is null) return null;
        if (valueType.IsInstanceOfType(raw)) return raw;

        if (valueType.IsEnum)
        {
            var text = raw.ToString() ?? string.Empty;
            return Enum.Parse(valueType, text, ignoreCase: true);
        }

        if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var itemType = valueType.GetGenericArguments()[0];
            var text = raw.ToString() ?? string.Empty;

            if (itemType == typeof(string))
            {
                return text
                    .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .ToList();
            }

            if (itemType == typeof(ushort))
            {
                return text
                    .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .Select(x => Convert.ToUInt16(x, CultureInfo.InvariantCulture))
                    .ToList();
            }
        }

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
}
