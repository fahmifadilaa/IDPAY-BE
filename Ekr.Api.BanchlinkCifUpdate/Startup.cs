using Ekr.Business.BancslinkCifUpdate;
using Ekr.Business.Contracts.BancslinkCifUpdate;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Dapper.Connection.Sql;
using Ekr.Dapper.Connection;
using Ekr.Repository.BancslinkCifUpdate;
using Ekr.Repository.Contracts.BancslinkCifUpdate;
using Ekr.Dapper.Connection.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Ekr.Core.Configuration;
using System.IO;

namespace Ekr.Api.BancslinkCifUpdate
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
                    Title = "Ekr.Api.BancslinkCifUpdate",
                    Version = "v1"
                });
            });

            services.Configure<ConnectionStringConfig>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<ErrorMessageConfig>(Configuration.GetSection("ErrorMessageConfig"));

            services.AddScoped<IEKtpReaderBackendDb, EKtpReaderBackendDb>();
            services.AddScoped<IBancslinkCifUpdateRepository, BancslinkCifUpdateRepository>();
            services.AddScoped<IBancslinkCifUpdateService, BancslinkCifUpdateService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // ALWAYS ENABLE SWAGGER (include production)
            app.UseSwagger();
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ekr.Api.BancslinkCifUpdate v1");
            //    c.RoutePrefix = "swagger"; // akses: /swagger/index.html
            //});
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Ekr.Api.BancslinkCifUpdate v1");
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
