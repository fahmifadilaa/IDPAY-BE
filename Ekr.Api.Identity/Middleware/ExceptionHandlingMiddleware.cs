using Ekr.Core.Entities;
using Ekr.Core.Entities.Logging;
using Ekr.Core.Helper;
using Ekr.Repository.Contracts.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Ekr.Api.Identity.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IErrorLogRepository _errorLogRepository;

        public ExceptionHandlingMiddleware(RequestDelegate next, IErrorLogRepository errorLogRepository)
        {
            _next = next;
            _errorLogRepository = errorLogRepository;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                httpContext.Request.EnableBuffering();
                await _next(httpContext);
            }
            catch (Exception e)
            {
                var err = new Tbl_LogError
                {
                    InnerException = e.InnerException?.Message,
                    CreatedAt = DateTime.Now,
                    Message = e.Message,
                    Payload = JsonConvert.SerializeObject(httpContext.Request.Body.ToString()),
                    Source = e.Source,
                    StackTrace = e.StackTrace,
                    SystemName = "Identity"
                };

                var numb = _errorLogRepository.CreateErrorLog(err);

                var res = new ExceptionDto
                {
                    ExceptionMessage = "91501 || Something wrong happen, please contact our administrator!",
                    TicketNumber = numb.ToString(),
                    ExceptionTrace = e
                };

                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(res));
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder, IErrorLogRepository errorLogRepository)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>(errorLogRepository);
        }
    }
}
