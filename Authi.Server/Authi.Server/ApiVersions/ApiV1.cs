using Authi.Common.Dto;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Authi.Server.ApiVersions
{
    public partial class ApiV1 : ApiVersionBase
    {
        private readonly TimeSpan RequestValidFor = TimeSpan.FromSeconds(30);

        public override void ConfigureRoutes(IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("/api/v1");
            api.MapPost("/init", OnInit);
            api.MapPost("/read", OnRead);
            api.MapPost("/write", OnWrite);
            api.MapPost("/delete", OnDelete);
            api.MapPost("/publish", OnPublish);
            api.MapPost("/consume", OnConsume);
        }

        private ErrorResponse<T>? VerifyPayload<T>([NotNull] PayloadBase? payload) where T : class
        {
            if (payload == null)
            {
                payload = new PayloadBase { Timestamp = 0 };
                return new ErrorResponse<T>(ErrorMessages.CantParsePayload);
            }

            if (!Services.Clock.IsRecent(payload.Timestamp, RequestValidFor))
            {
                return new ErrorResponse<T>(ErrorMessages.CantVerifyClock);
            }

            return null;
        }
    }
}
