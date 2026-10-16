using IBS.Services;

namespace IBSWeb.Api
{
    public static class StartOfTheMonthApi
    {
        public static void MapStartOfTheMonthApi(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/api/start-of-the-month-service",
                async (StartOfTheMonthService service, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        await service.ExecuteAsync(cancellationToken);
                        return Results.Ok("StartOfTheMonthService processing completed.");
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        return Results.Problem("Start-of-month processing failed.", statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
                .AllowAnonymous();
        }
    }
}
