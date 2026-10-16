using IBS.Services;

namespace IBSWeb.Api
{
    public static class DailyApi
    {
        public static void MapDailyApi(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/api/daily-service",
                async (DailyService service, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        await service.ExecuteAsync(cancellationToken);
                        return Results.Ok("DailyService processing completed.");
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        return Results.Problem("Daily processing failed.", statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
                .AllowAnonymous();
        }
    }
}
