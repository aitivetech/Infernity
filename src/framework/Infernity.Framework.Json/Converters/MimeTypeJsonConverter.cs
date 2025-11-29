using System.Diagnostics.CodeAnalysis;

using Infernity.Framework.Core.Collections;
using Infernity.Framework.Core.Content;

namespace Infernity.Framework.Json.Converters;

public sealed class MimeTypeJsonConverter : StringProxyJsonConverter<MimeType>
{
    protected override bool TryParse(
        string value,
        [NotNullWhen(true)] out MimeType? parsedValue
    )
    {
        var result = MimeTypes.GetById(value);
        if (result)
        {
            parsedValue = result.Value;
            return true;
        }

        result = MimeTypes.GetByExtension(value).FirstOrNone();

        if (result)
        {
            parsedValue = result.Value;
            return true;
        }

        parsedValue = null;
        return false;
    }

    protected override string ToString(MimeType value)
    {
        return value.Id;
    }
}
