using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Exceptions;

public sealed class ExceptionContext
{
    internal ExceptionContext(Exception exception)
    {
        Exception = exception;
    }

    public Exception Exception { get; set; }
    
    public bool WasHandled { get; set; }
}