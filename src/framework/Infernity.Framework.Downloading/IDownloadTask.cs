using Infernity.Framework.Core.Functional;
using Infernity.Framework.Security.Hashing;

namespace Infernity.Framework.Downloading;

public enum DownloadTaskState
{
    Queued,
    Active,
    Succeeded,
    Failed,
    Cancelled
}

public sealed record DownloadTaskData(
    DownloadTaskId Id,
    DownloadTaskState State,
    DateTimeOffset CreatedAt,
    string Url,
    string TargetPath,
    long Length,
    long Position,
    Optional<Sha256Value> Hash,
    Optional<DateTimeOffset> CompletedAt);

public interface IDownloadTask
{
    DownloadTaskId Id { get; }
    DownloadTaskState State { get; }
    string Url { get; }
    string TargetPath { get; }
    long Length { get; }
    long Position { get; }
    Optional<Sha256Value> Hash { get; }
    Task Cancel();
    double CompletionRatio => (double)Position / (double)Length;
}