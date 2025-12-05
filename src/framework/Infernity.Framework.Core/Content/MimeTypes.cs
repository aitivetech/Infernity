using Infernity.Framework.Core.Collections;
using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Core.Content;

/// <summary>
///     Generated from: https://github.com/patrickmccallum/mimetype-io
/// </summary>
public static partial class MimeTypes
{
    public const string DefaultExtension = ".bin";
    private static readonly Dictionary<string, MimeType> _mimeTypesById = new();
    private static readonly Dictionary<MimeType, List<string>> _extensionsByMimeType = new();
    private static readonly Dictionary<string, List<MimeType>> _mimeTypesByExtension = new();

    public static readonly MimeType InfernityModelPackage = Declare("application/infernity+modelpackage",
        [".nupkg"],
        MimeTypeEncoding.Binary,
        MimeTypeCategory.Archive);

    public static readonly MimeType InfernityModelDataPackage = Declare("application/infernity+modeldatapackage",
        [".imdp"],
        MimeTypeEncoding.Binary,
        MimeTypeCategory.Archive);

    public static readonly MimeType InfernityModelSetPackage = Declare("application/infernity+modelsetpackage",
        [".nupkg"],
        MimeTypeEncoding.Binary,
        MimeTypeCategory.Archive);
    
    public static bool IsCategory(string id,
        MimeTypeCategory category)
    {
        return GetById(id).Select(c => c.Category == category).Or(false);
    }

    public static bool IsEncoding(string id,
        MimeTypeEncoding encoding)
    {
        return GetById(id).Select(c => c.Encoding == encoding).Or(false);
    }

    public static Optional<MimeType> Detect(string? contentType,
        string? path)
    {
        if (contentType != null)
        {
            var mimeType = GetById(contentType);

            if (mimeType)
            {
                return mimeType.Value;
            }
        }

        if (path != null)
        {
            var extension = Path.GetExtension(path);
            var mimeTypes = GetByExtension(extension);

            return mimeTypes.FirstOrNone();
        }

        return Optional<MimeType>.None;
    }

    public static MimeType DetectWithDefault(string? contentType,
        string? path)
    {
        return Detect(contentType,
            path).Or(ApplicationOctetStream);
    }

    public static Optional<MimeType> GetById(string id)
    {
        return _mimeTypesById.GetOptional(id);
    }

    public static string GetExtensionById(string id)
    {
        var mimeType = GetById(id);

        if (mimeType)
        {
            return mimeType.Value.Extensions.FirstOrDefault() ?? DefaultExtension;
        }

        return DefaultExtension;
    }

    public static string GetIdByExtension(string extension)
    {
        var mimeTypes = GetByExtension(extension);

        if (mimeTypes.Any())
        {
            return mimeTypes.First().Id;
        }

        var actualExtension = Path.GetExtension(extension);

        mimeTypes = GetByExtension(actualExtension);

        return mimeTypes.Any() ? mimeTypes.First().Id : ApplicationOctetStream.Id;
    }

    public static IReadOnlyList<MimeType> GetByExtension(string extension)
    {
        var actualExtension = extension.StartsWith(".") ? extension : "." + extension;

        if (_mimeTypesByExtension.TryGetValue(actualExtension.ToLower(),
                out var mimeTypes))
        {
            return mimeTypes;
        }

        return Array.Empty<MimeType>();
    }

    private static MimeType Declare(
        string id,
        IReadOnlyList<string> extensions,
        MimeTypeEncoding encoding = MimeTypeEncoding.Unknown,
        MimeTypeCategory category = MimeTypeCategory.Unknown)
    {
        var result = new MimeType(id,
            extensions,
            encoding,
            category);

        if (_mimeTypesById.TryAdd(result.Id,
                result))
        {
            // We have a new one sop add to the other collections
            var extensionsForMimeType =
                _extensionsByMimeType.GetOrAdd(result,
                    r => new List<string>());

            extensionsForMimeType.AddRange(extensions);
            _extensionsByMimeType[result] = extensionsForMimeType.Distinct().ToList();

            // Add to _mimeTypesByExtension
            foreach (var extension in extensions)
            {
                var mimeTypesForExtension = _mimeTypesByExtension.GetOrAdd(extension,
                    e => new List<MimeType>());
                mimeTypesForExtension.Add(result);
                _mimeTypesByExtension[extension] = mimeTypesForExtension.Distinct().ToList();
            }

            return result;
        }

        return _mimeTypesById[id];
    }

    #region Our own

    #endregion

    #region Additional common ones that seem not to be tracked upstream

    public static readonly MimeType ModelGlb = Declare("model/gltf-binary",
        [".glb"],
        MimeTypeEncoding.Binary,
        MimeTypeCategory.Model3d);

    public static readonly MimeType ModelGltf = Declare("model/gltf+json",
        [".gltf"],
        MimeTypeEncoding.Text,
        MimeTypeCategory.Model3d);

    public static readonly MimeType ModelObj =
        Declare("model/obj",
            [".obj"],
            MimeTypeEncoding.Text,
            MimeTypeCategory.Model3d);

    public static readonly MimeType ModelUsdz =
        Declare("model/vnd.usd+zip",
            [".usdz"],
            MimeTypeEncoding.Binary,
            MimeTypeCategory.Model3d);

    public static readonly MimeType ModelUsda =
        Declare("model/vnd.usda",
            [".usda"],
            MimeTypeEncoding.Text,
            MimeTypeCategory.Model3d);

    public static readonly MimeType ModelStl =
        Declare("model/stl",
            [".stl"],
            MimeTypeEncoding.Text,
            MimeTypeCategory.Model3d);

    public static readonly MimeType ModelRhino =
        Declare("model/vnd.3dm",
            [".3dm"],
            MimeTypeEncoding.Text,
            MimeTypeCategory.Model3d);

    public static readonly MimeType ModelPly =
        Declare("text/plain+ply",
            [".ply"],
            MimeTypeEncoding.Text,
            MimeTypeCategory.Model3d);

    public static readonly MimeType ModelFbx =
        Declare("application/fbx ",
            [".fbx"],
            MimeTypeEncoding.Binary,
            MimeTypeCategory.Model3d);

    #endregion
}