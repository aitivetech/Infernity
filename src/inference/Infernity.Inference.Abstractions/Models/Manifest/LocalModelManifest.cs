using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

using Infernity.Framework.Core.Text;
using Infernity.Framework.Json.Converters;
using Infernity.Framework.Security.Hashing;

namespace Infernity.Inference.Abstractions.Models.Manifest;

public abstract class LocalModelManifest : ModelManifest
{
    [JsonConverter(typeof(ModelQuantityJsonConverter))]
    public long ParameterCount { get; set; }

    public ModelQuantizationType Quantization { get; set; }

    public Sha256Value Hash { get; set; }

    public Sha256Value CompressedHash { get; set; }

    public long Size { get; set; }

    public long CompressedSize { get; set; }

    public override ModelId GenerateId()
    {
        List<string> values =
        [
            Provider,
            Family
        ];

        if (SubFamily != ModelFamilyId.Unknown)
        {
            values.Add(SubFamily);
        }
        
        values.Add(Architecture);
        values.Add(ModelQuantityParser.ToString(ParameterCount));
        values.Add(Quantization.ToString());
        
        return string.Join("_",
            values.Select(v => v.ToLowerInvariant()));
    }
}

