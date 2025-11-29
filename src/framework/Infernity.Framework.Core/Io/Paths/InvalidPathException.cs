namespace Infernity.Framework.Core.Io.Paths
{
    /// <summary>
    /// An invalid path was specified.
    /// </summary>
    public class InvalidPathException : ArgumentException
    {
        /// <summary>
        /// An invalid path was specified.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="message"></param>
        public InvalidPathException(string path, string message)
            : base(message + " (" + path + ")") { }

        public InvalidPathException() : base()
        {
        }

        public InvalidPathException(string? message) : base(message)
        {
        }

        public InvalidPathException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public InvalidPathException(string? message, string? paramName, Exception? innerException) : base(message, paramName, innerException)
        {
        }
    }
}
