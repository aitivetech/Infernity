using Infernity.Framework.Core;

using NuGet.Packaging.Core;

namespace Infernity.Inference.Packaging.Nuget;

public static class NugetModelPackageSchema
{
    public const string PackageFileName = "package.nupkg";

    public static readonly PackageType PackageType = new PackageType(ApplicationSuiteInfo.Id + "Model",
        new Version(1,
            0));
}