using Infernity.Framework.Core.Functional;
using Infernity.GeneratedCode;

using LLama.Native;

namespace Infernity.Inference.Providers.Llama;

[AddLogger]
internal sealed partial class LlamaInferenceProviderLoader
{
    private static readonly Lock _lock = new();
    private static Optional<LlamaInferenceProviderLoader> _instance;

    private readonly Optional<LlamaInferenceProviderConfiguration> _configuration;

    internal LlamaInferenceProviderLoader(Optional<LlamaInferenceProviderConfiguration> configuration)
    {
        _configuration = configuration;

        var container = NativeLibraryConfig.All.WithVulkan(false).WithCuda(false).WithAvx(AvxLevel.None)
            .WithAutoFallback(false)
            .WithLogCallback(Logger);

        if (configuration)
        {
            switch (configuration.Value.Backend)
            {
                case LlamaBackendType.Cpu:
                    container.WithAvx(AvxLevel.Avx512);
                    break;
                case LlamaBackendType.Cuda:
                    container.WithCuda(true);
                    break;
                case LlamaBackendType.Vulkan:
                    container.WithVulkan(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            container.WithAvx(AvxLevel.Avx2);
        }
    }

    internal static LlamaInferenceProviderLoader GetOrCreate(
        Optional<LlamaInferenceProviderConfiguration> configuration)
    {
        lock (_lock)
        {
            // It has already been created.
            if (_instance)
            {
                // With a configuration before so we are ok.
                if (_instance.Value._configuration)
                {
                    return _instance.Value;
                }

                // We want to configure but is has been created without one.. Since we cannot unload we have to fail here
                if (configuration)
                {
                    throw new InvalidOperationException("LlamaInferenceProviderLoader already created");
                }

                return _instance.Value;
            }

            _instance = new LlamaInferenceProviderLoader(configuration);
            return _instance.Value;
        }
    }
}