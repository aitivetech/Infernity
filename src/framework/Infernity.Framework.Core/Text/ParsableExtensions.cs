using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Text;

public static class ParsableExtensions
{
    extension<T>(T) where T : IParsable<T>
    {
        public static Optional<T> TryParseOptional(string value,
            IFormatProvider? provider = null)
        {
            if (T.TryParse(value,
                    provider,
                    out var result))
            {
                return result;
            }
            
            return Optional<T>.None;
        }
    }
}