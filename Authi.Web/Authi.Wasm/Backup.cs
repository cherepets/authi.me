#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.

using Authi.Common.Client;
using Authi.Common.Extensions;
using System;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

public partial class backup
{
    [JSExport]
    public static async Task<string> parse(string content)
    {
        var result = content
            .Split('\n')
            .Select(item => item.Trim())
            .Select(item => OtpauthUri.TryParse(item, out var otpauth) ? otpauth : null)
            .Where(uri => uri != null)
            .Select(uri => uri!)
            .Select(uri => new CredentialDto
            {
                Title = uri.Issuer,
                Subtitle = uri.Account,
                Secret = uri.Secret,
                CloudId = Guid.Empty,
                Timestamp = 0
            });
        return result.ToJson();
    }
}

#pragma warning restore CS8981
