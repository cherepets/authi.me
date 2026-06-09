using Authi.Server.ApiVersions;
using Microsoft.AspNetCore.Http;
using System;
using OtpNet;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Authi.Server.Web
{
    public abstract class WebPageBase : ApiVersionBase
    {
        protected const string ContentType = "text/html; charset=utf-8";

        protected bool TryAuthorize(HttpContext context, out string? error)
        {
            var authCookie = context.Request.Cookies["auth"];
            if (authCookie == null)
            {
                error = "Missing cookie.";
                return false;
            }

            try
            {
                var parts = authCookie.Split(':');
                if (parts.Length != 3)
                {
                    error = $"Invalid cookie.";
                    return false;
                }
                if (parts[0] != Services.Configuration.CredentialHash)
                {
                    error = $"Invalid user or password.";
                    return false;
                }
                if (!long.TryParse(parts[1], out var timestamp))
                {
                    error = $"Invalid timestamp.";
                    return false;
                }

                var serverTimestamp = Services.Clock.Timestamp;
                var skew = timestamp - serverTimestamp;
                if (Math.Abs(skew) > TimeSpan.FromHours(1).TotalMilliseconds)
                {
                    error = $"Expired cookie.";
                    return false;
                }

                var secret = Base32Encoding.ToBytes(Services.Configuration.TotpSecret);
                var expectedTotp = new Totp(secret).ComputeTotp(
                    DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime);
                if (parts[2] != expectedTotp)
                {
                    error = $"Invalid TOTP code.";
                    return false;
                }

                error = null;
                return true;
            }
            catch (Exception exception)
            {
                error = $"Authorization error: {exception}";
                return false;
            }
        }

        protected IResult AuthorizedPage(HttpContext context, Func<IResult> content)
        {
            return TryAuthorize(context, out _)
                ? content()
                : Results.Content(Html("login"), ContentType);
        }

        protected async Task<IResult> AuthorizedPage(HttpContext context, Func<Task<IResult>> content)
        {
            return TryAuthorize(context, out _)
                ? await content()
                : Results.Content(Html("login"), ContentType);
        }

        protected static string Html(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream($"Authi.Server.Web.html.{fileName}.html");
            using var reader = new StreamReader(stream!);
            return reader.ReadToEnd();
        }
    }
}
