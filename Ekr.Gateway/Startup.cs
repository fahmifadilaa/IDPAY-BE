using Ekr.Core.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System;
using System.IO;
using System.Text;

namespace Ekr.Gateway
{
    public class Startup
    {
        public Startup()
        {
			//if (bool.Parse(Configuration.GetSection("IsDocker").Value))
			//{
			//    var builder = new ConfigurationBuilder()
			//    .SetBasePath(Directory.GetCurrentDirectory())
			//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			//    .AddJsonFile("OcelotConfigDocker.json")
			//    .AddEnvironmentVariables();

			//    Configuration = builder.Build();
			//}
			//else
			//{
			//    var builder = new ConfigurationBuilder()
			//    .SetBasePath(Directory.GetCurrentDirectory())
			//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			//    .AddJsonFile("OcelotConfig.json")
			//    .AddEnvironmentVariables();

			//    Configuration = builder.Build();
			//}

			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile("OcelotConfig.json")
				.AddEnvironmentVariables();

			Configuration = builder.Build();
		}

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOcelot(Configuration);

            //add cors
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            services.AddControllers();

            var appSettingSection = Configuration.GetSection("AppSettings");
            services.Configure<CredConfig>(appSettingSection);

            var appSettings = appSettingSection.Get<CredConfig>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            var issuer = appSettings.Issuer;

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
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                };
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            await app.UseOcelot().ConfigureAwait(false);

            app.UseCors("AllowAll");

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
