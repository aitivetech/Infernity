using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Exceptions;

public interface IExceptionHandler
{
    Optional<Exception> Handle(Exception ex);
}