using Infernity.Framework.Configuration;
using Infernity.Framework.Core.Functional;

namespace Infernity.Inference.Packaging.Nuget;

[ConfigurationSection("PackageFeed")]
public sealed class NugetPackageFeedConfiguration
{
    public sealed class UserCredentials
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public bool IsPasswordClearText { get; set; }
    }
    
    public required string Uri { get; set; }
    
    public required string DataUri { get; set; }
    
    public bool AllowInsecureConnections { get; set; }
    
    public Optional<UserCredentials> Credentials { get; set; }
}