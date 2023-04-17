using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;

namespace Showtime.Api.Application.Exceptions;

public class HttpGlobalExceptionFilter : IExceptionFilter
{
    private readonly IWebHostEnvironment env;
    private readonly ILogger<HttpGlobalExceptionFilter> logger;

    public HttpGlobalExceptionFilter(IWebHostEnvironment env, ILogger<HttpGlobalExceptionFilter> logger)
    {
        this.env = env;
        this.logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        logger.LogError(new EventId(context.Exception.HResult),
            context.Exception,
            context.Exception.Message);

        if (context.Exception is ShowtimeException showtimeException)
        {
            var problemDetails = new ValidationProblemDetails()
            {
                Instance = context.HttpContext.Request.Path,
                Status = StatusCodes.Status400BadRequest,
                Detail = "Please refer to the errors property for additional details."
            };

            problemDetails.Errors.Add("DomainValidations", new string[] { context.Exception.Message.ToString() });

            if(showtimeException.Errors.Length > 0)
            {
                problemDetails.Errors.Add("Errors", showtimeException.Errors);
            }

            if (context.Exception.InnerException is FluentValidation.ValidationException validationException)
            {
                var validationErrors = GetErrors(validationException);
                // Add the validation errors to the details
                foreach (var  validationError in validationErrors)
                {
                    problemDetails.Errors.Add(validationError.Key, validationError.Value);
                }
            }
            context.Result = new BadRequestObjectResult(problemDetails);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
        else
        {
            var json = new JsonErrorResponse
            {
                Messages = new[] { "Oops! An error has occured. Please, try it again." }
            };

            if (env.IsDevelopment())
            {
                json.DeveloperMessage = new { 
                    context.Exception.Message,
                    StackTrace = context.Exception.StackTrace.ToString(),
                };
            }

            context.Result = new InternalServerErrorObjectResult(json);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        context.ExceptionHandled = true;
    }

    private static IReadOnlyDictionary<string, string[]> GetErrors(FluentValidation.ValidationException exception)
    {
        var errors = exception.Errors.Where(x => x != null).GroupBy(x => x.PropertyName,
                                            x => x.ErrorMessage, (propertyName, errorMessages) => new
                                            {
                                                Key = propertyName,
                                                Values = errorMessages.Distinct().ToArray()
                                            }).ToDictionary(x => x.Key, x => x.Values);    
        return errors;
    }

    private class JsonErrorResponse
    {
        public string[] Messages { get; set; }

        public object DeveloperMessage { get; set; }
    }

    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult(object error) : base(error)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
