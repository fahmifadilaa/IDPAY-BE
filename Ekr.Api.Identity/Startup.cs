using Ekr.Api.Identity.Middleware;
using Ekr.Dependency;
using Ekr.Repository.Contracts.Logging;
using Elastic.Apm.NetCoreAll;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Ekr.Api.Identity
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //add cors
            services.AddCors(options =>
            options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            var ioc = new IoCConfiguration(services);
            ioc.LoadConfiguration(Configuration);
            ioc.RegisterForWebIdentity();

            services.AddControllers();
            services.AddSwaggerGen(c => c.SwaggerDoc("v1.2.0", new OpenApiInfo { Title = "Ekr.Api.Identity", Version = "v1.2.0" }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandlingMiddleware(app.ApplicationServices.GetService<IErrorLogRepository>());
            app.UseAllElasticApm(Configuration);

            if (bool.Parse(Configuration.GetSection("IsSwaggerVisible").Value))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("v1.2.0/swagger.json", "Ekr.Api.Identity v1.2.0"));
            }

            app.UseCors("AllowAll");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
