using Infernity.Framework.Core.Text;

namespace Infernity.Framework.Json.Converters;

public sealed class ScientificQuantityJsonConverter : StringProxyJsonConverter<long>
{
    protected override bool TryParse(string value,
        out long parsedValue)
    {
        return ScientificQuantityParser.TryParse(value,
            out parsedValue);
    }

    protected override string ToString(long value)
    {
        return ScientificQuantityParser.ToString(value);
    }
}