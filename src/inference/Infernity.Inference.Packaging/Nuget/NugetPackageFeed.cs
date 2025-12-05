using System.IO.Compression;

using Infernity.Framework.Core.Data;
using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Io.Paths;
using Infernity.Framework.Core.Patterns.Disposal;
using Infernity.Inference.Abstractions.Models;

using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

using PathLib;

using SemanticVersion = Infernity.Framework.Core.Versioning.SemanticVersion;

namespace Infernity.Inference.Packaging.Nuget;

public class NugetPackageFeed : Disposable, IModelFeed
{
    public const string FeedName = "default";

    private readonly SourceRepository _sourceRepository;
    private readonly SourceCacheContext _cacheContext;
    private readonly ILogger _logger = NullLogger.Instance;

    public NugetPackageFeed(NugetPackageFeedConfiguration configuration)
    {
        var credentials = configuration.Credentials.Select(c => new PackageSourceCredential(
            FeedName,
            c.Username,
            c.Password,
            c.IsPasswordClearText,
            null));

        var packageSource = new PackageSource(configuration.Uri,
            FeedName,
            true) { AllowInsecureConnections = configuration.AllowInsecureConnections, IsOfficial = true };

        if (credentials)
        {
            packageSource.Credentials = credentials.Value;
        }

        _sourceRepository = Repository.Factory.GetCoreV3(packageSource);
        _cacheContext = new SourceCacheContext();
    }

    public async Task<Optional<ModelDescription>> ReadById(VersionedModelId id,
        CancellationToken cancellationToken = default)
    {
        var resource = await _sourceRepository.GetResourceAsync<PackageMetadataResourceV3>(cancellationToken);

        var metadata = await resource.GetMetadataAsync(GetIdentity(id),
            _cacheContext,
            _logger,
            cancellationToken);

        if (metadata != null)
        {
            return ToModelDescription(
                metadata);
        }

        return Optional.None<ModelDescription>();
    }

    public async Task<IPagingResponse<long, ModelDescription>> Query(
        ModelFeedQuery query,
        IOffsetPagingRequest pagingRequest,
        CancellationToken cancellationToken = default)
    {
        var resource = await _sourceRepository.GetResourceAsync<PackageSearchResourceV3>(cancellationToken);

        var filter = new SearchFilter(false,
            SearchFilterType.IsLatestVersion)
        {
            PackageTypes = new List<string>() { NugetModelPackageSchema.PackageType.Name }
        };

        var searchTerms = string.Empty;

        var searchResult = await resource.SearchAsync(searchTerms,
            filter,
            (int)pagingRequest.Start,
            pagingRequest.Limit,
            _logger,
            cancellationToken);

        if (searchResult != null)
        {
            return pagingRequest.Respond<ModelDescription>(searchResult.Select(ToModelDescription).ToList(),
                0L); // 0L means autodetect base on page result size.    
        }

        return pagingRequest.RespondEmpty<ModelDescription>();
    }

    public async Task Install(
        IModelLibrary target,
        VersionedModelId modelId,
        CancellationToken cancellationToken = default)
    {
        using var writer = target.Write(modelId);

        var resource = await _sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
        var identity = GetIdentity(modelId);

        var packageFilePath = writer.TempDirectoryPath / NugetModelPackageSchema.PackageFileName;

        await DownloadModelPackage(
            packageFilePath,
            resource,
            identity,
            cancellationToken);

        var packageContentDirectoryPath = (PosixPath)(writer.TempDirectoryPath / "package_contents");
        packageContentDirectoryPath.EnsureDirectory();
        
        await ExtractModelPackage(
            packageFilePath,
            packageContentDirectoryPath,writer.ManifestFilePath,cancellationToken);
        
        
        
    }

    private async Task ExtractModelPackage(
        PurePosixPath packagePath,
        PosixPath targetPath,
        PosixPath manifestTargetPath,
        CancellationToken cancellationToken)
    {
        await using var packageStream = new FileStream(
            packagePath.ToPosix(),
            FileMode.Open,
            FileAccess.Read,
            FileShare.None);

        await ZipFile.ExtractToDirectoryAsync(packageStream,
            targetPath.ToPosix(),
            cancellationToken);

        var sourceManifestPath = targetPath / ModelFileLayout.ManifestFileName;
        
        File.Copy(sourceManifestPath.ToPosix(),manifestTargetPath.ToPosix());
    }

    private async Task DownloadModelPackage(PurePosixPath packageFilePath,
        FindPackageByIdResource resource,
        PackageIdentity identity,
        CancellationToken cancellationToken)
    {
        await using var packageStream = new FileStream(
            packageFilePath.ToPosix(),
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        await resource.CopyNupkgToStreamAsync(identity.Id,
            identity.Version,
            packageStream,
            _cacheContext,
            _logger,
            cancellationToken);
    }

    public Task Uninstall(IModelLibrary target,
        VersionedModelId modelId,
        CancellationToken token = default)
    {
        target.Delete(modelId);
        return Task.CompletedTask;
    }

    protected override void OnDispose()
    {
        _cacheContext.Dispose();
    }

    private PackageIdentity GetIdentity(VersionedModelId id)
    {
        return new PackageIdentity(id.Id,
            new NuGetVersion(id.Version.ToString()));
    }

    private VersionedModelId GetVersionedId(IPackageSearchMetadata metadata)
    {
        return new VersionedModelId(metadata.Identity.Id,
            SemanticVersion.Parse(metadata.Identity.Version.ToNormalizedString()));
    }

    private ModelDescription ToModelDescription(
        IPackageSearchMetadata metadata)
    {
        var id = GetVersionedId(metadata);
        return NugetPackageTags.Decode(id.Id,
            id.Version,
            metadata.Description,
            metadata.Tags);
    }
}