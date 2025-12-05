using System.Globalization;

using Infernity.Framework.Core.Versioning;

namespace Infernity.Inference.Abstractions.Models;

public record ModelDescription(
    ModelId Id,  
    SemanticVersion Version,
    InferenceProviderId Provider,
    ModelFamilyId Family,
    ModelFamilyId SubFamily,
    ModelArchitectureId Architecture,
    RegionInfo CountryOfOrigin,
    ModelQuantizationType Quantization,
    long ParameterCount,
    string Description)
{
    
}