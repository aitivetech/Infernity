using Infernity.Framework.Core.Collections;
using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Exceptions;

public sealed class RootExceptionHandler : IRootExceptionHandler
{
    private readonly IReadOnlyList<IExceptionHandler> _exceptionHandlers;

    public RootExceptionHandler(IEnumerable<IExceptionHandler> exceptionHandlers)
    {
        _exceptionHandlers = exceptionHandlers.OrderOrderableOptionally().ToList();
    }

    public bool Handle(Exception exception)
    {
        var currentException = Optional.Some(exception);

        foreach (var handler in _exceptionHandlers)
        {
            if (currentException)
            {
                currentException = handler.Handle(currentException.Value);
            }
        }

        return currentException.IsUndefined;
    }
}