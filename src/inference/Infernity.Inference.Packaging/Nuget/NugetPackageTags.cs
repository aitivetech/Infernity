using System.Globalization;

using Infernity.Framework.Core.Text;
using Infernity.Framework.Core.Versioning;
using Infernity.Inference.Abstractions;
using Infernity.Inference.Abstractions.Models;
using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Packaging.Nuget;

public static class NugetPackageTags
{
    private const string Seperator = "-";

    public static ISet<string> Encode(ModelManifest manifest)
    {
        var result = new HashSet<string>();

        result.AddConditional(nameof(ModelManifest.Architecture),
                manifest.Architecture,
                ModelArchitectureId.Unknown)
            .AddConditional(nameof(ModelManifest.Family),
                manifest.Family,
                ModelFamilyId.Unknown)
            .AddConditional(nameof(ModelManifest.SubFamily),
                manifest.SubFamily,
                ModelFamilyId.Unknown)
            .AddConditional(nameof(ModelManifest.Provider),
                manifest.Provider,
                InferenceProviderId.Unknown);

        result.Add(EncodeSingle(nameof(manifest.CountryOfOrigin),
            manifest.CountryOfOrigin.TwoLetterISORegionName.ToLowerInvariant()));
        
        if (manifest is LocalModelManifest localManifest)
        {
            result.Add(EncodeSingle(nameof(LocalModelManifest.Quantization),localManifest.Quantization));

            if (localManifest.ParameterCount > 0L)
            {
                result.Add(EncodeSingle(nameof(LocalModelManifest.ParameterCount),
                    ModelQuantityParser.ToString(localManifest.ParameterCount)));
            }
        }

        return result;
    }

    public static ModelDescription Decode(
        ModelId id,
        SemanticVersion version,
        string description,
        string tagsValue)
    {
        var tagValues = tagsValue.Split(' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var tags = new Dictionary<string, string>();

        foreach (var tagValue in tagValues)
        {
            var parts = tagValue.Split(Seperator,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            tags.Add(parts[0], parts[1]);
        }

        return Decode(id,version,description,tags);
    }

    public static ModelDescription Decode(
        ModelId id,
        SemanticVersion version,
        string description,
        IReadOnlyDictionary<string,string> tags
        )
    {
        return new(id,
            version,
            tags.Decode<InferenceProviderId>(nameof(ModelManifest.Provider),
                InferenceProviderId.Unknown),
            tags.Decode(nameof(ModelManifest.Family),
                ModelFamilyId.Unknown),
            tags.Decode(nameof(ModelManifest.SubFamily),
                ModelFamilyId.Unknown),
            tags.Decode(nameof(ModelManifest.Architecture),
                ModelArchitectureId.Unknown),
            tags.Decode(nameof(ModelManifest.CountryOfOrigin),
                new RegionInfo("US"),
                s => new RegionInfo(s)),
            tags.DecodeEnum(nameof(LocalModelManifest.Quantization),
                ModelQuantizationType.F32),
            tags.Decode<long>(nameof(LocalModelManifest.ParameterCount),
                0L,
                ModelQuantityParser.Parse),
            description
        );
    }
    
    private static string EncodeSingle<T>(string name,
        T value,
        bool nameToLower = true,
        bool valueToLower = true)
        where T : notnull
    {
        var finalName = nameToLower ? name.ToLowerInvariant() : name;
        var finalValue = valueToLower ? value.ToString()!.ToLowerInvariant() : value.ToString()!;

        return $"{finalName}{Seperator}{finalValue}";
    }
    
    extension(ISet<string> tags)
    {
        private ISet<string> AddConditional<T>(string name,
            T value,
            T comparand,
            bool nameToLower = true,
            bool valueToLower = true)
            where T : IEquatable<T>
        {
            if (!value.Equals(comparand))
            {
                tags.Add(EncodeSingle(name,
                    value,
                    nameToLower,
                    valueToLower));
            }

            return tags;
        }
    }

    extension(IReadOnlyDictionary<string, string> tags)
    {
        private T DecodeEnum<T>(string name,
            T defaultValue)
            where T: struct,Enum
        {
            return tags.Decode<T>(name,defaultValue,s => Enum.Parse<T>(s,true));
        }
        
        private T Decode<T>(string name,
            T defaultValue)
            where T: notnull,IParsable<T>
        {
            return tags.Decode<T>(name,defaultValue,s => T.Parse(s,null));
        }

        private T Decode<T>(string name,
            T defaultValue,
            Func<string,T> converter)
            where T: notnull
        {
            string? tagValue = null;
            
            if (tags.TryGetValue(name,
                    out tagValue) || tags.TryGetValue(name.ToLowerInvariant(),
                    out tagValue))
            {
                return converter(tagValue);
            }

            return defaultValue;
        }
    }
}