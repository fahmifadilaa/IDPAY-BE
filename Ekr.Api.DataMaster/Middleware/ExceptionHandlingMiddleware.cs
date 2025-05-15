using Ekr.Core.Entities;
using Ekr.Core.Entities.Logging;
using Ekr.Core.Helper;
using Ekr.Repository.Contracts.Logging;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Ekr.Api.DataMaster.Middleware
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

                httpContext.Request.Body.Dispose();
            }
            catch (Exception e)
            {

                httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                string body = "";

                using (StreamReader stream = new StreamReader(httpContext.Request.Body))
                {
                    body = stream.ReadToEnd();
                }

                var err = new Tbl_LogError
                {
                    InnerException = e.InnerException?.Message,
                    CreatedAt = DateTime.Now,
                    Message = e.Message,
                    Payload = JsonConvert.SerializeObject(body.ToString()),
                    Source = e.Source,
                    StackTrace = e.StackTrace,
                    SystemName = "Data Master"
                };

                var numb = _errorLogRepository.CreateErrorLog(err);

                var res = new ExceptionDto
                {
                    ExceptionMessage = "Something wrong happen, please contact our administrator!",
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
