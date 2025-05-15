using Ekr.Repository.Contracts.DataMaster.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using Ekr.Auth;
using Ekr.Core.Entities.DataMaster.Utility.Entity;
using Ekr.Core.Configuration;
using Ekr.Core.Constant;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Ekr.Core.Helper;

namespace Ekr.Api.MappingNik.Filters
{
    public class LogActivityAttribute : ActionFilterAttribute
    {
        public string Keterangan { get; set; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await Task.WhenAll(LogEkr(context), next.Invoke());
        }

        private Task LogEkr(ActionExecutingContext context)
        {
            var logRepo = context.HttpContext.RequestServices.GetService<IUtility1Repository>();

            var logConfig = context.HttpContext.RequestServices.GetService<IOptions<LogConfig>>().Value;

            string authorization = context.HttpContext.Request.Headers["Authorization"];

            var unitId = 1;
            var npp = "Admin";
            var userId = 20;

            var request = context.HttpContext.Request;

            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var token = authorization.Split(" ")[1];

                if (request.Headers["Type-Aplikasi"] == "ClientApps")
                {
                    var claims = TokenManager.GetPrincipal(token);

                    unitId = int.Parse(string.IsNullOrEmpty(claims?.UnitId) ? "0" : (claims?.UnitId ?? "0"));
                    npp = claims?.NIK ?? "";
                    userId = int.Parse(string.IsNullOrEmpty(claims?.UserId) ? "0" : (claims?.UserId ?? "0"));
                }

                else
                {
                    var claims = TokenManager.GetPrincipalAgent(token);

                    unitId = 0;
                    npp = claims.Name;
                    userId = 0;
                }
            }

            var log = new TblLogActivity
            {
                UnitId = unitId,
                ActionTime = DateTime.Now,
                Browser = request.Headers["User-Agent"].ToString() ?? "",
                DataBaru = request.Headers["DataBaru"].ToString() ?? "",
                DataLama = request.Headers["DataLama"].ToString() ?? "",
                Ip = request.Headers["Cf-Connecting-Ip"].ToString() ?? (request.Headers["X-Forwarded-For"].ToString() ?? ""),
                Keterangan = Keterangan,
                Npp = npp,
                UserId = userId,
                Url = string.Concat(request.Scheme,
                "://",
                request.Host.ToUriComponent(),
                request.PathBase.ToUriComponent(),
                request.Path.ToUriComponent(),
                request.QueryString.ToUriComponent()),
                ClientInfo = "Route = " + (request.Headers["UrlFront"].ToString() ?? "")
            };

            //if (logConfig.IsLogActive)
            //{
            //    var sb = new StringBuilder(DateTime.Now.ToString("G") + " [RequestInformation] :");
            //    sb.AppendLine(JsonConvert.SerializeObject(log));

            //    FileHelper.WriteOrReplaceFileContentOrCreateNewFile(sb.ToString(), logConfig.PathLogActivity, logConfig.FileNameActivity, "logs", TimingInterval.DAILY);
            //}

            return Task.FromResult(logRepo.InsertLogActivity(log));
        }
    }
}
