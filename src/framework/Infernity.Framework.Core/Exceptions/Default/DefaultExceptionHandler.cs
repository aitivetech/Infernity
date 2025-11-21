using Infernity.Framework.Core.Collections;
using Infernity.Framework.Core.Pipelines;

namespace Infernity.Framework.Core.Exceptions.Default;

public sealed class DefaultExceptionHandler : IExceptionHandler
{
    private readonly Action<ExceptionContext> _pipeline;

    public DefaultExceptionHandler(IEnumerable<IExceptionMiddleware> exceptionProcessors)
    {
        _pipeline = exceptionProcessors.OrderOrderableOptionally().Compile();
    }
    
    public bool Handle(Exception ex)
    {
        var context = new ExceptionContext(ex);
        _pipeline(context);
        return context.WasHandled;
    }
}