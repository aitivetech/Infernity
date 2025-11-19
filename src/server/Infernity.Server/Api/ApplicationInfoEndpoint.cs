using FastEndpoints;

namespace Infernity.Server.Api;

public sealed class ApplicationInfoEndpoint : Ep.NoReq.Res<IResult>
{
    public override void Configure()
    {
        Get("api/info");
        AllowAnonymous();
    }

    public override async Task<IResult> ExecuteAsync(CancellationToken ct)
    {
        return TypedResults.Ok("Hello world");
    }
}