#pragma warning disable CA1050 // Declare types in namespaces
#pragma warning disable IDE1006 // Naming Styles

using Authi.Common.Client;
using Authi.Common.Extensions;
using Authi.Common.Services;
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Client = Authi.Common.Client.ApiClient;

public partial class apiClient
{
    private static readonly Clock clock = new();
    private static readonly Crypto crypto = new();

    [JSExport]
    public static async Task<string> consume(string syncCodeBase64)
    {
        var bytes = syncCodeBase64.ToBase64Bytes();
        var syncCode = SyncCode.Deserialize(bytes);
        using var client = new Client(syncCode.ServerUrl, clock, crypto);
        var result = await client.ConsumeAsync(syncCode);
        return result.ToJson();
    }

    [JSExport]
    public static async Task<string> read(string? serverUrl, string clientIdString, string versionString, string dataKeyBase64, string syncPrivateKeyBase64, string syncPublicKeyBase64)
    {
        using var client = new Client(serverUrl, clock, crypto);
        var clientId = Guid.Parse(clientIdString);
        var version = Guid.Parse(versionString);
        var dataKey = new AesKey(dataKeyBase64.ToBase64Bytes());
        var syncKeyPair = new X25519KeyPair(
            new X25519PrivateKey(syncPrivateKeyBase64.ToBase64Bytes()),
            new X25519PublicKey(syncPublicKeyBase64.ToBase64Bytes()));
        var result = await client.ReadAsync(clientId, version, dataKey, syncKeyPair);
        return result.ToJson();
    }
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA1050 // Declare types in namespaces
