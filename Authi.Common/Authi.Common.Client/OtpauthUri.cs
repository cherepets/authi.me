using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Web;

namespace Authi.Common.Client
{
    public class OtpauthUri(string issuer, string secret, string? account)
    {
        public string Issuer { get; } = issuer;
        public string Secret { get; } = secret;
        public string? Account { get; } = account;

        public static bool TryParse(string uriString, [NotNullWhen(true)] out OtpauthUri? otpauth)
        {
            Uri uri;
            NameValueCollection queryParams;
            try
            {
                uri = new Uri(uriString);
                queryParams = HttpUtility.ParseQueryString(uri.Query);
            }
            catch
            {
                otpauth = null;
                return false; 
            }

            string issuer;
            string secret;
            string? account = null;

            var label = uri.LocalPath.Trim('/', ':').Split(':');
            if (label.Length == 0)
            {
                otpauth = null;
                return false; 
            }
            if (label.Length == 2)
            {
                account = label[1];
            }

            issuer = queryParams["issuer"]?.ToString() ?? label[0];
            secret = queryParams["secret"]?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(secret))
            {
                otpauth = null;
                return false;
            }

            otpauth = new OtpauthUri(issuer, secret, account);
            return true;
        }

        public override string ToString()
        {
            var issuer = WebUtility.UrlEncode(Issuer);
            var secret = WebUtility.UrlEncode(Secret);
            var account = WebUtility.UrlEncode(Account);
            return $"otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}";
        }
    }
}
