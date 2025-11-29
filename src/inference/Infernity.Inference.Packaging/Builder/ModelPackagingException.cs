using Infernity.Framework.Core.Exceptions;

namespace Infernity.Inference.Packaging.Builder;

public class ModelPackagingException : InfernityException
{
    public ModelPackagingException()
    {
    }

    public ModelPackagingException(string message) : base(message)
    {
    }

    public ModelPackagingException(string message, Exception inner) : base(message, inner)
    {
    }
}