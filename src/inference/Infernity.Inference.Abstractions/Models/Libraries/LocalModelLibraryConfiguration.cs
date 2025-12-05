using Infernity.Framework.Configuration;
using Infernity.Framework.Core.Io.Paths;

namespace Infernity.Inference.Abstractions.Models.Libraries;

[ConfigurationSection("ModelLibrary")]
public class LocalModelLibraryConfiguration
{
    public required PurePosixPath Path { get; set; }
}