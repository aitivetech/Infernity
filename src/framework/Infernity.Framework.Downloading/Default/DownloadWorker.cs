using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Channels;

using Infernity.Framework.Security.Hashing;
using Infernity.GeneratedCode;

namespace Infernity.Framework.Downloading.Default;

[AddLogger]
internal sealed partial class DownloadWorker
{
    private readonly DownloadConfiguration _configuration;
    private readonly IHashProvider<Sha256Value> _hashProvider;
    private readonly ChannelReader<DownloadTask> _taskReader;

    internal DownloadWorker(
        DownloadConfiguration configuration,
        IHashProvider<Sha256Value> hashProvider,
        ChannelReader<DownloadTask> taskReader)
    {
        _configuration = configuration;
        _hashProvider = hashProvider;
        _taskReader = taskReader;
    }

    internal async Task Run(CancellationToken cancellationToken)
    {
        using var httpClient = _configuration.ClientFactory.Create();

        await foreach (var task in _taskReader.ReadAllAsync(cancellationToken)
                           .Where(t => t.State == DownloadTaskState.Queued)
                           .WithCancellation(cancellationToken))
        {
            await Process(task,
                httpClient,
                cancellationToken);
        }
    }

    private async Task Process(DownloadTask task,
        HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        try
        {
            await using (var writeStream = await _configuration.Storage.OpenWrite(task.TargetPath,
                             !task.ContinueExisting))
            {
                writeStream.Seek(0,
                    SeekOrigin.End);

                await task.Progress(writeStream.Position);
                await Download(task,
                    httpClient,
                    writeStream,
                    cancellationToken);
            }

            if (await Validate(task,
                    cancellationToken))
            {
                await task.Success();
            }
            else
            {
                await task.Failed(new ValidationException("Validation failed"),
                    0);
            }
        }
        catch (IOException ex)
        {
            await task.Failed(ex,
                0);
        }
        catch (HttpRequestException ex)
        {
            var statusCode = 0;
            if (ex.StatusCode != null)
            {
                statusCode = (int)ex.StatusCode.Value;
            }

            await task.Failed(ex,
                statusCode);
        }
    }

    private async Task Download(DownloadTask task,
        HttpClient httpClient,
        Stream writeStream,
        CancellationToken cancellationToken)
    {
        var bytesLeftToRead = task.Length - writeStream.Position;

        foreach (var chunkSize in GetChunkSizes(bytesLeftToRead))
        {
            using var request = new HttpRequestMessage(HttpMethod.Get,
                task.Url)
            {
                Headers =
                {
                    Range = new RangeHeaderValue(writeStream.Position,
                        chunkSize)
                }
            };
            
            using var response = await httpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                await using var readStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var oldWritePosition = writeStream.Position;
                await readStream.CopyToAsync(writeStream,cancellationToken);
                
                var bytesRead =  writeStream.Position - oldWritePosition;

                if (bytesRead != chunkSize)
                {
                    throw new HttpRequestException(HttpRequestError.InvalidResponse,
                        statusCode: HttpStatusCode.RequestedRangeNotSatisfiable);
                }
                
                await task.Progress(writeStream.Position);
            }
            else
            {
                throw new HttpRequestException(HttpRequestError.Unknown,statusCode:response.StatusCode);
            }
        }
    }

    private async Task<bool> Validate(DownloadTask task,
        CancellationToken cancellationToken)
    {
        if (task.Hash)
        {
            await using var readStream = await _configuration.Storage.OpenRead(task.TargetPath);
            var streamHash = await _hashProvider.Algorithm.ComputeAsync(readStream, cancellationToken);

            return streamHash == task.Hash.Value;
        }

        return true;
    }

    private IEnumerable<int> GetChunkSizes(long bytesLeftToRead)
    {
        var chunkCount = bytesLeftToRead % _configuration.ChunkSize;
        var lastChunkSize = bytesLeftToRead % _configuration.ChunkSize;

        for (var i = 0; i < chunkCount; ++i)
        {
            yield return _configuration.ChunkSize;
        }

        if (lastChunkSize != 0)
        {
            yield return (int)lastChunkSize;
        }
    }
}