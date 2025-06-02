using System;
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GtMotive.Estimate.Microservice.Api.Filters
{
    public sealed class BusinessExceptionFilter : IExceptionFilter
    {
        private readonly IAppLogger<BusinessExceptionFilter> _appLogger;

        public BusinessExceptionFilter(IAppLogger<BusinessExceptionFilter> appLogger)
        {
            _appLogger = appLogger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _appLogger.LogError(context.Exception, "Exception captured in BusinessExceptionFilter.");

            if (context.Exception is DomainException)
            {
                var problemDetails = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                    Detail = context.Exception.Message,
                    Instance = context.HttpContext.Request.Path,
                };

                _appLogger.LogWarning("Domain Exception: {status} - {detail}", problemDetails.Status, problemDetails.Detail);

                context.Result = new BadRequestObjectResult(problemDetails);
                context.Exception = null;
            }
            else
            {
                var problemDetails = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = context.Exception.Message,
                    Instance = context.HttpContext.Request.Path,
                };

                _appLogger.LogError(context.Exception, "Unhandled Exception");

                context.Result = new InternalServerErrorObjectResult(problemDetails);
                context.Exception = null;
            }
        }
    }
}
