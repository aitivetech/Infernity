using System.Diagnostics.CodeAnalysis;

using Infernity.Framework.Core.Text;
using Infernity.Framework.Core.Versioning;
using Infernity.GeneratedCode;

using Semver;

namespace Infernity.Inference.Abstractions.Models;

[TypedId]
public readonly partial record struct ModelSetId(string Value)
{
    
    
}

public readonly record struct VersionedModelSetId(
    ModelSetId Id,
    SemVersion Version) : IParsable<VersionedModelSetId>
{
    public override string ToString() => $"{Id}.{Version}";

    public static VersionedModelSetId Parse(string s,
        IFormatProvider? provider)
    {
        return ParsingUtilities.ParseCore<VersionedModelSetId>(s,provider);
    }

    public static bool TryParse([NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out VersionedModelSetId result)
    {
        if (ParsingUtilities.TryParseConcatenatedValues<ModelSetId, SemVersion>(s,
                ".",
                SemVersion.ParseOptional,
                out var id,
                out var version))
        {
            result = new VersionedModelSetId(id,version);
            return true;
        }

        result = default;
        return false;
    }
}