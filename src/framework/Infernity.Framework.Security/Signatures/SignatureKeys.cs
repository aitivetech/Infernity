using System.Security.Cryptography;

using Infernity.Framework.Json.Dom;

namespace Infernity.Framework.Security.Signatures;

public sealed class SignatureKeys : TypedJsonDocument<SignatureKeys>
{
    public required string Public { get; init; }
    public required string Private { get; init; }
    
    public static SignatureKeys Generate()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        
        var privateKey = Convert.ToBase64String(ecdsa.ExportECPrivateKey());
        var publicKey = Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo());

        return new() { Public = publicKey, Private = privateKey, };
    }
}