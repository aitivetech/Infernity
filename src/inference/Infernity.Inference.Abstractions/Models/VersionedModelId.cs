using System.Diagnostics.CodeAnalysis;

using Infernity.Framework.Core.Versioning;

namespace Infernity.Inference.Abstractions.Models;

public readonly record struct VersionedModelId(
    ModelId Id,
    SemanticVersion Version) : IParsable<VersionedModelId>
{
    public override string ToString() => $"{Id}.{Version}";

    public static VersionedModelId Parse(string s,
        IFormatProvider? provider)
    {
        if (!TryParse(s,
                provider,
                out VersionedModelId result))
        {
            throw new FormatException($"Could not parse {nameof(VersionedModelId)}: {s}");
        }
        
        return result;
    }

    public static bool TryParse([NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out VersionedModelId result)
    {
        if (s == null)
        {
            result = default;
            return false;
        }

        var firstDotIndex = s.IndexOf('.');

        var modelIdString = s.Substring(0,
            firstDotIndex);

        var versionString = s.Substring(firstDotIndex + 1);

        if (ModelId.TryParse(modelIdString,
                provider,
                out var id) && 
            SemanticVersion.TryParse(versionString, 
                null,
                out var version))
        {
            result = new VersionedModelId(id, version);
            return true;
        }

        result = default;
        return false;
    }
}