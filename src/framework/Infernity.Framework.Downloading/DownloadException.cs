using Infernity.Framework.Core.Exceptions;

namespace Infernity.Framework.Downloading;

public class DownloadException : InfernityException
{
    public DownloadException()
    {
    }

    public DownloadException(string message) : base(message)
    {
    }

    public DownloadException(string message, Exception inner) : base(message, inner)
    {
    }
}