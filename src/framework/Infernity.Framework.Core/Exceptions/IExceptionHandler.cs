namespace Infernity.Framework.Core.Exceptions;

public interface IExceptionHandler
{
   bool Handle(Exception ex);
}