using Infernity.Framework.Core.Exceptions;

namespace Infernity.Framework.Configuration;

public class ConfigurationException : InfernityException
{
    public ConfigurationException()
    {
    }

    public ConfigurationException(string message) : base(message)
    {
    }

    public ConfigurationException(string message, Exception inner) : base(message, inner)
    {
    }
}