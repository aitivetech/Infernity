using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Infernity.Framework.Core.Reflection;

public class ParsableTypeConverter<T> : TypeConverter
    where T : IParsable<T>
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context,
        Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context,
        [NotNullWhen(true)] Type? destinationType)
    {
        return destinationType == typeof(string);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context,
        CultureInfo? culture,
        object? value,
        Type destinationType)
    {
        if (value is T t)
        {
            return t.ToString();
        }

        return null;
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value)
    {
        if (value is string s)
        {
            return T.Parse(s,
                culture);
        }

        return null;
    }
}