using AspNetCore.Proxy;
using Ekr.Api.DataMaster.Middleware;
using Ekr.Core.Configuration;
using Ekr.Core.Constant;
using Ekr.Dependency;
using Ekr.Repository.Contracts.Logging;
using Elastic.Apm.NetCoreAll;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace Ekr.Api.DataMaster
{
    public class Startup
    {
        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            var ioc = new IoCConfiguration(services);
            ioc.LoadConfiguration(Configuration);
            ioc.RegisterForWebApi();

            // Get credential from appsetting.json file
            var appSettingSection = Configuration.GetSection("AppSettings");

            // Extract appsetting values
            var appSettings = appSettingSection.Get<CredConfig>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            var issuer = appSettings.Issuer;
            // Set authentication settings
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    // Set validation to be challanged by our credential
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = false
                };
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v2.0.0", new OpenApiInfo { Title = "Ekr.Api.DataMaster v2.0.0", Version = "v2.0.0" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (bool.Parse(Configuration.GetSection("IsSwaggerVisible").Value))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("v2.0.0/swagger.json", "Ekr.Api.DataMaster v2.0.0"));
                app.UseSwaggerUI(c => c.SwaggerEndpoint("EktpDataMaster/swagger/v2.0.0/swagger.json", "Ekr.Api.DataMaster SubDomain v2.0.0"));
            }

            app.UseExceptionHandlingMiddleware(app.ApplicationServices.GetService<IErrorLogRepository>());

            //app.UseEncryptionHandlingMiddleware();

            app.UseAllElasticApm(Configuration);

            app.UseAuthentication();

            app.UseCors("AllowAll");

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
