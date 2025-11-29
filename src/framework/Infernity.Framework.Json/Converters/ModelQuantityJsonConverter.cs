using Infernity.Framework.Core.Text;

namespace Infernity.Framework.Json.Converters;

public sealed class ModelQuantityJsonConverter : StringProxyJsonConverter<long>
{
    protected override bool TryParse(string value,
        out long parsedValue)
    {
        return ModelQuantityParser.TryParse(value, out parsedValue);
    }

    protected override string ToString(long value)
    {
        return ModelQuantityParser.ToString(value);
    }
}