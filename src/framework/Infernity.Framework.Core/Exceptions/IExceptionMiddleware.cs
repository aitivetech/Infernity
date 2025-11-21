using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Pipelines;

namespace Infernity.Framework.Core.Exceptions;

public interface IExceptionMiddleware : IPipelineStep<ExceptionContext>
{
   
}