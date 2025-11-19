namespace Infernity.Framework.Plugins.Isolation
{
    internal class RuntimeOptions
    {
        public string? Tfm { get; set; }

        public string[]? AdditionalProbingPaths { get; set; }
    }
    
    internal class RuntimeConfiguration
    {
        public RuntimeOptions? RuntimeOptions { get; set; }
    }
}
