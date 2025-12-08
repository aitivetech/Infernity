using Infernity.Framework.Core.Functional;

using Semver;

namespace Infernity.Framework.Core.Versioning;

public static class SemVersionExtensions
{
    extension(SemVersion version)
    {
        public bool IsCompatibleWith(SemVersion other)
        {
            return version.Major == other.Major && version.IsRelease ==  other.IsRelease;
        }
    }

    extension(SemVersion)
    {
        public static Optional<SemVersion> ParseOptional(string value)
        {
            if (SemVersion.TryParse(value,
                    SemVersionStyles.Any,
                    out var version))
            {
                return version;
            }
            
            return Optional<SemVersion>.None;
        }
    }
}