#if DEBUG

using Authi.Server.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace Authi.Server.ApiVersions
{
    public class DebugApi : ApiVersionBase
    {
        public override void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("/debug");
            api.MapGet("/client/{id}", async (Guid id) =>
            {
                try
                {
                    return await Services.ClientRepository.ReadAsync(id);
                }
                catch
                {
                    return (new { Error = "Can't find client!" }) as object;
                }
            });
            api.MapGet("/data/{id}", async (Guid id) =>
            {
                try
                {
                    return await Services.DataRepository.ReadAsync(id);
                }
                catch
                {
                    return (new { Error = "Can't find data!" }) as object;
                }
            });
            api.MapGet("/sync/{id}", async (Guid id) =>
            {
                try
                {
                    return await Services.SyncRepository.ReadAsync(id);
                }
                catch
                {
                    return (new { Error = "Can't find sync!" }) as object;
                }
            });
            api.MapGet("/clock", () => Services.Clock.Timestamp);
            api.MapGet("/newKeys", () =>
            {
                var keys = Services.Crypto.GenerateX25519KeyPair();
                return new KeyPair
                {
                    Private = keys.Private.ToString(),
                    Public = keys.Public.ToString()
                };
            });
        }
    }
}

#endif
