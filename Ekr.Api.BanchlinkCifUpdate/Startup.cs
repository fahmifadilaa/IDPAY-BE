using Ekr.Business.BanchlinkCifUpdate;
using Ekr.Business.Contracts.BanchlinkCifUpdate;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Dapper.Connection.Sql;
using Ekr.Dapper.Connection;
using Ekr.Repository.BanchlinkCifUpdate;
using Ekr.Repository.Contracts.BanchlinkCifUpdate;
using Ekr.Dapper.Connection.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Ekr.Core.Configuration;
using System.IO;

namespace Ekr.Api.BanchlinkCifUpdate
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // ================================
        // 1. REGISTER SERVICES
        // ================================
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // ENABLE SWAGGER (Development + Production)
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Ekr.Api.BanchlinkCifUpdate",
                    Version = "v1"
                });
            });

            services.Configure<ConnectionStringConfig>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<ErrorMessageConfig>(Configuration.GetSection("ErrorMessageConfig"));

            services.AddScoped<IEKtpReaderBackendDb, EKtpReaderBackendDb>();
            services.AddScoped<IBanchlinkCifUpdateRepository, BanchlinkCifUpdateRepository>();
            services.AddScoped<IBanchlinkCifUpdateService, BanchlinkCifUpdateService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // ALWAYS ENABLE SWAGGER (include production)
            app.UseSwagger();
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ekr.Api.BanchlinkCifUpdate v1");
            //    c.RoutePrefix = "swagger"; // akses: /swagger/index.html
            //});
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Ekr.Api.BanchlinkCifUpdate v1");
                c.RoutePrefix = "swagger";
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
