using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Abstractions.Models.Analysis;

public interface IModelAnalyzer
{
    bool AppliesTo(DirectoryInfo directoryInfo);
    
    ModelManifest Analyze(DirectoryInfo directoryInfo,ModelInfo modelInfo);
}