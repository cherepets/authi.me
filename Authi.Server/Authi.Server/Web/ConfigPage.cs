using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Authi.Server.Web
{
    public class ConfigPage : WebPageBase
    {
        public override void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            if (!Services.Configuration.Exists)
            {
                app.MapGet("/", (HttpContext context) => Results.Content(
                    Html("config")
                        .Replace(
                            "{{CONNECTION_STRING}}",
                            string.Empty)
                        .Replace(
                            "{{TOTP_SECRET}}",
                            string.Empty),
                    ContentType));
            }
            else
            {
                app.MapGet("/config", (HttpContext context) =>
                    AuthorizedPage(
                        context,
                        () => Results.Content(
                            Html("config")
                                .Replace(
                                    "{{CONNECTION_STRING}}",
                                    HtmlEncoder.Default.Encode(Services.Configuration.ConnectionString))
                                .Replace(
                                    "{{TOTP_SECRET}}",
                                    HtmlEncoder.Default.Encode(Services.Configuration.TotpSecret)),
                            ContentType)));
            }

            app.MapPost("/config", async (HttpContext context, ConfigRequest request) =>
            {
                if (Services.Configuration.Exists && !TryAuthorize(context, out _))
                {
                    return Results.Unauthorized();
                }

                var (success, error) = await TrySaveConfigurationAsync(request);
                if (!success)
                {
                    return Results.BadRequest(error);
                }

                context.Response.OnCompleted(() =>
                {
                    app.ServiceProvider
                        .GetRequiredService<IHostApplicationLifetime>()
                        .StopApplication();
                    return Task.CompletedTask;
                });

                return Results.Ok();
            });
        }

        internal async Task<(bool Success, string? Error)> TrySaveConfigurationAsync(
            ConfigRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ConnectionString))
            {
                return (false, "Missing connection string.");
            }
            if (string.IsNullOrWhiteSpace(request.Login))
            {
                return (false, "Missing login.");
            }
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return (false, "Missing password.");
            }
            if (string.IsNullOrWhiteSpace(request.TotpSecret))
            {
                return (false, "Missing TOTP secret.");
            }

            try
            {
                if (!await Services.Database.CanConnectAsync(request.ConnectionString))
                {
                    return (false, "Invalid database connection.");
                }

                await Services.Configuration.SaveAsync(
                    request.ConnectionString,
                    request.Login,
                    request.Password,
                    request.TotpSecret);
                return (true, null);
            }
            catch (Exception exception)
            {
                return (false, $"Configuration error: {exception}");
            }
        }

    }

    public record ConfigRequest(
        string ConnectionString,
        string Login,
        string Password,
        string TotpSecret);
}
