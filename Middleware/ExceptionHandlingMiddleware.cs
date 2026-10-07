using System.Net;
using System.Text.Json;
using ClinicApi.Common;
using ClinicApi.Services;

namespace ClinicApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            PatientAlreadyExistsException => (HttpStatusCode.Conflict, ex.Message),
            DoctorAlreadyExistsException => (HttpStatusCode.Conflict, ex.Message),
            UserAlreadyExistsException => (HttpStatusCode.Conflict, ex.Message),
            AppointmentConflictException => (HttpStatusCode.Conflict, ex.Message),
            EntityNotFoundException => (HttpStatusCode.NotFound, ex.Message),
            InvalidAppointmentDateException => (HttpStatusCode.BadRequest, ex.Message),
            InvalidAppointmentStatusException => (HttpStatusCode.BadRequest, ex.Message),
            InvalidCredentialsException => (HttpStatusCode.Unauthorized, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
        }
        else
        {
            _logger.LogWarning(ex, "Handled exception: {Message}", ex.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ApiErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = message
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
