using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BloodLink.Api.Services;

public class ServiceExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not ServiceException se)
        {
            return false;
        }

        var problem = new ProblemDetails
        {
            Status = se.StatusCode,
            Title = se.Message,
        };
        if (se.Errors.Count > 0)
        {
            problem.Extensions["errors"] = se.Errors;
        }

        context.Response.StatusCode = se.StatusCode;
        await context.Response.WriteAsJsonAsync(problem, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json", ct);
        return true;
    }
}
