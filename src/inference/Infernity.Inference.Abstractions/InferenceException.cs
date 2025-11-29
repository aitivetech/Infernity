using Infernity.Framework.Core.Exceptions;

namespace Infernity.Inference.Abstractions;

public class InferenceException : InfernityException
{
    public InferenceException()
    {
    }

    public InferenceException(string message) : base(message)
    {
    }

    public InferenceException(string message, Exception inner) : base(message, inner)
    {
    }
}