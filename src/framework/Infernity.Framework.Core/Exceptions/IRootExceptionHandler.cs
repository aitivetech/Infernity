namespace Infernity.Framework.Core.Exceptions;

public interface IRootExceptionHandler
{
    bool Handle(Exception exception);
}