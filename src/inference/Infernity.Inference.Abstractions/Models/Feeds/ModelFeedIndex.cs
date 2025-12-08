using Infernity.Framework.Json.Dom;
using Infernity.Framework.Security.Signatures;

using Semver;

namespace Infernity.Inference.Abstractions.Models.Feeds;


public sealed class ModelFeedIndex : TypedJsonDocument<ModelFeedIndex>
{
    public sealed record SignedReference(
        SemVersion Version,
        Signature Signature);
    
    public required string PublicKey { get; set; }
    
    public required IReadOnlyList<string> Servers { get; set; }
    
    public required IReadOnlyDictionary<ModelId,IReadOnlyList<SignedReference>> Models { get; set; }
    
    public required IReadOnlyDictionary<ModelSetId,IReadOnlyList<SignedReference>> Sets { get; set; }
}