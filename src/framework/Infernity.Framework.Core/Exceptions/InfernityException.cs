namespace Infernity.Framework.Core.Exceptions;

public abstract class InfernityException : Exception
{
    protected InfernityException() { }
    protected InfernityException(string message) : base(message) { }

    protected InfernityException(string message,
        Exception inner) : base(message,
        inner)
    {
    }
}
