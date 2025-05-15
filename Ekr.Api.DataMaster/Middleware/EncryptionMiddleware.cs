using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.Logging;
using Ekr.Core.Helper;
using Ekr.Core.Securities.Symmetric;
using Ekr.Repository.Contracts.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Api.DataMaster.Middleware
{
    public class EncryptionMiddleware
    {
        private readonly RequestDelegate _next;

        public EncryptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.Body = Aes256Encryption.EncryptStream(httpContext.Response.Body);
            //httpContext.Request.Body = DecryptStream(httpContext.Request.Body);
            //if (httpContext.Request.QueryString.HasValue)
            //{
            //    string decryptedString = DecryptString(httpContext.Request.QueryString.Value.Substring(1));
            //    httpContext.Request.QueryString = new QueryString($"?{decryptedString}");
            //}
            await _next(httpContext);
            //await httpContext.Request.Body.DisposeAsync();
            await httpContext.Response.Body.DisposeAsync();
        }
        
    }
    

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class EncryptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseEncryptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EncryptionMiddleware>();
        }
    }
}
