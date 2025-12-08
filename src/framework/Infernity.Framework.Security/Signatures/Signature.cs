using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

using Infernity.Framework.Security.Hashing;
using Infernity.GeneratedCode;

namespace Infernity.Framework.Security.Signatures;

[TypedId]
public readonly partial record struct Signature(
    string Value)
{
    public async Task<bool> Validate(string path,
        IHashProvider<Sha256Value> hashProvider,
        string publicKey,
        CancellationToken cancellationToken = default)
    {
        await using var fileStream = new FileStream(path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        
        return await Validate(fileStream,hashProvider,publicKey,cancellationToken);
    }
    
    public async Task<bool> Validate(Stream source,
        IHashProvider<Sha256Value> hashProvider,
        string publicKey,
        CancellationToken cancellationToken = default)
    {
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);

        var hash = await hashProvider.Algorithm.ComputeAsync(source,
            cancellationToken);
        
        var signature = Convert.FromBase64String(Value);
        
        return ecdsa.VerifyHash(hash.Data.Span, signature);
    }
    
    public static async Task<Signature> Generate(
        string path,
        IHashProvider<Sha256Value> hashProvider,
        SignatureKeys keys,
        CancellationToken cancellationToken = default)
    {
        await using var fileStream = new FileStream(path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        return await Generate(fileStream,
            hashProvider,
            keys,
            cancellationToken);
    }

    public static async Task<Signature> Generate(
        Stream source,
        IHashProvider<Sha256Value> hashProvider,
        SignatureKeys keys,
        CancellationToken cancellationToken = default)
    {
        var hash = await hashProvider.Algorithm.ComputeAsync(source,
            cancellationToken);

        using var ecdsa = ECDsa.Create();
        ecdsa.ImportECPrivateKey(Convert.FromBase64String(keys.Private),
            out _);

        var signature = ecdsa.SignHash(hash.Data.Span);

        return new(Convert.ToBase64String(signature));
    }
}