using Infernity.Framework.Core.Collections;
using Infernity.GeneratedCode;

using Microsoft.Extensions.DependencyInjection;

namespace Infernity.Framework.Core.Startup;

public static partial class ServiceCollectionExtensions
{
    extension(IServiceProvider serviceProvider)
    {
        public async Task ExecuteStartupTasks()
        {
            foreach (var startupTask in serviceProvider.GetServices<IStartupTask>().OrderOrderableOptionally())
            {
                await startupTask.Execute();
            }
        }
    }
}