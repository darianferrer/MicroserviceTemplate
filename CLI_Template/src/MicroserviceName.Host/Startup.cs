using FluentValidation.AspNetCore;
using MicroserviceName.Host.AppSettings;
using MicroserviceName.Host.Middlewares;
using MicroserviceName.Host.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace MicroserviceName.Host
{
    public class Startup
    {
        private readonly SwaggerSettings _swaggerSettings = new SwaggerSettings();
        private readonly AppMetadataSettings _appMetadata = new AppMetadataSettings();

        public Startup(IConfiguration configuration)
        {
            configuration.GetSection("Swagger").Bind(_swaggerSettings);
            configuration.GetSection("Meta").Bind(_appMetadata);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers()
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddFluentValidation(fv =>
                {
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc(_appMetadata.Version, new OpenApiInfo
                {
                    Title = _appMetadata.Name,
                    Description = _swaggerSettings.Description,
                    Contact = _swaggerSettings.Contact,
                    Version = _appMetadata.Version,
                });
                config.OperationFilter<RequiredHeadersOperationFilter>();
            });

            services.AddSingleton(_appMetadata);

            // TODO: Add Dapper/FluentMigrator or EntityFramework Core setup
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware<LoggingMiddleware>();
            app.UseMiddleware<DiagnosticsMiddleware>();
            app.UseMiddleware<ErrorLoggingMiddleware>();
            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger(action => action.RouteTemplate = "docs/{documentName}/swagger.json");
            app.UseSwaggerUI(action =>
            {
                action.SwaggerEndpoint
                (
                    $"/docs/{_appMetadata.Version}/swagger.json",
                    $"{_appMetadata.Name} v{_appMetadata.Version}"
                );
                action.RoutePrefix = "docs";
            });

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
