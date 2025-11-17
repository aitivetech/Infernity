using System.ComponentModel;

using Infernity.Tools.SourceGenerators.Output;

using Microsoft.CodeAnalysis;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Infernity.Tools.SourceGenerators;

// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
// Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
public class MimeTypeFurtherReading
{
    [JsonProperty("title")] public string Title { get; set; } = null!;

    [JsonProperty("url")] public string Url { get; set; } = null!;
}

public class MimeTypeLinks
{
    [JsonProperty("deprecates")] public List<string> Deprecates { get; } = new List<string>();

    [JsonProperty("relatedTo")] public List<string> RelatedTo { get; } = new List<string>();

    [JsonProperty("parentOf")] public List<string> ParentOf { get; } = new List<string>();

    [JsonProperty("alternativeTo")] public List<string> AlternativeTo { get; } = new List<string>();
}

public class MimeTypeNotices
{
    [JsonProperty("hasNoOfficial")] public bool? HasNoOfficial { get; set; }

    [JsonProperty("communityContributed")] public bool? CommunityContributed { get; set; }

    [JsonProperty("popularUsage")] public string PopularUsage { get; set; } = null!;
}

public enum MimeTypeCategory
{
    Unknown,
    Text,
    Markup,
    Code,
    Document,
    Archive,
    Audio,
    Image,
    Model3d,
    Video,
}

public enum MimeTypeEncoding
{
    Unknown,
    Text,
    Binary,
}

public class MimeTypeRoot
{
    [JsonProperty("name")] public string Name { get; set; } = null!;

    [JsonProperty("description")] public string Description { get; set; } = null!;

    [JsonProperty("links")] public MimeTypeLinks Links { get; set; } = null!;

    [JsonProperty("fileTypes")] public List<string> FileTypes { get; } = new List<string>();

    [JsonProperty("furtherReading")]
    public List<MimeTypeFurtherReading> FurtherReading { get; } = new List<MimeTypeFurtherReading>();

    [JsonProperty("notices")] public MimeTypeNotices Notices { get; set; } = null!;

    [JsonProperty("deprecated")] public bool? Deprecated { get; set; }

    [JsonProperty("useInstead")] public string UseInstead { get; set; } = null!;

    [DefaultValue(MimeTypeEncoding.Unknown)]
    [JsonProperty("encoding", DefaultValueHandling = DefaultValueHandling.Populate)]
    public MimeTypeEncoding Encoding { get; set; }

    [DefaultValue(MimeTypeCategory.Unknown)]
    [JsonProperty("category", DefaultValueHandling = DefaultValueHandling.Populate)]
    public MimeTypeCategory Category { get; set; }
}

[Generator]
public class MimeTypesSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var additionalFiles = context.AdditionalTextsProvider.Where(a =>
                                     {
                                         var fileName = Path.GetFileName(a.Path);

                                         if (fileName != null)
                                         {
                                             return fileName.Equals("MimeTypes.json",
                                                                    StringComparison.InvariantCultureIgnoreCase);
                                         }

                                         return false;
                                     })
                                     .Select(Generate)
                                     .Where(s => s != null);

        context.RegisterSourceOutput(additionalFiles,
                                     (productionContext, sourceFile) =>
                                     {
                                         sourceFile?.AddToOutput(productionContext);
                                     });
    }

    private SourceFile? Generate(AdditionalText additionalText, CancellationToken cancellationToken)
    {
        var writer = new SourceWriter();

        SourceFile? Return()
        {
            return writer?.ToSourceFile("MimeTypes.g.cs");
        }

        var jsonText = additionalText?.GetText()?.ToString();

        if (string.IsNullOrWhiteSpace(jsonText))
        {
            writer.WriteLine("Mimetype json is empty");
            return Return();
        }

        IReadOnlyList<MimeTypeRoot> mimeTypeData = ReadMimeTypes(jsonText!);

        if (!mimeTypeData.Any())
        {
            writer.WriteLine("No mimetypes defined");
            return Return();
        }

        writer.WriteLine("namespace Infernity.Framework.Core.Content;");
        writer.WriteEmptyLines(1);

        var categoriesSet = mimeTypeData.Count(m => m.Category != MimeTypeCategory.Unknown);
        var encodingSet = mimeTypeData.Count(m => m.Encoding != MimeTypeEncoding.Unknown);

        writer.WriteLine($"// Generated {mimeTypeData.Count} mime types");
        writer.WriteLine($"// Encodings set: {encodingSet}/{mimeTypeData.Count()}");
        writer.WriteLine($"// Categories set: {categoriesSet}/{mimeTypeData.Count}");
        writer.WriteLine("public static partial class MimeTypes");
        writer.OpenBlock();

        foreach (var mimeType in mimeTypeData.OrderBy(o => o.Name))
        {
            var propertyName = CreatePropertyName(mimeType.Name);
            var extensions = string.Join(",", mimeType.FileTypes.Select(f => "\"" + f + "\""));

            writer.WriteLine(
                $"public static readonly MimeType {propertyName} = Declare(\"{mimeType.Name}\",[{extensions}], MimeTypeEncoding.{mimeType.Encoding}, MimeTypeCategory.{mimeType.Category});");
            writer.WriteEmptyLines(1);
        }

        writer.CloseBlock();

        return Return();
    }

    private static Dictionary<string, string?> _propertyReplacements = new()
    {
        { "/", null },
        { "+", "Plus" },
        { "-", null },
        { ".", null }
    };

    private string CreatePropertyName(string mimeType)
    {
        var result = mimeType;

        foreach (var entry in _propertyReplacements)
        {
            result = SplitAndCapitalize(result, entry.Key, entry.Value);
        }

        return result;
    }

    private string SplitAndCapitalize(string value, string split, string? replacement)
    {
        var parts = value.Split(new[] { split }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1)
        {
            return parts[0].Capitalize();
        }

        return string.Join(replacement ?? string.Empty, parts.Select(p => p.Capitalize()));
    }

    private IReadOnlyList<MimeTypeRoot> ReadMimeTypes(string jsonText)
    {
        var serializer = new JsonSerializer();
        serializer.Converters.Add(new StringEnumConverter());

        using var reader = new StringReader(jsonText);
        using var jsonReader = new JsonTextReader(reader)
        {
            CloseInput = false
        };

        return serializer.Deserialize<List<MimeTypeRoot>>(jsonReader) ?? new List<MimeTypeRoot>(1);
    }
}