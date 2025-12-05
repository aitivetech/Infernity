using Infernity.Framework.Core.Exceptions;

namespace Infernity.Inference.Packaging;

public class ModelPackageException : InfernityException
{
    public ModelPackageException()
    {
    }

    public ModelPackageException(string message) : base(message)
    {
    }

    public ModelPackageException(string message, Exception inner) : base(message, inner)
    {
    }
}