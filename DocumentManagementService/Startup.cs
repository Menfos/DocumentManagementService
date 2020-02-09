using System;
using System.IO;
using System.Reflection;
using DocumentManagementService.Data;
using DocumentManagementService.Data.CosmosDb;
using DocumentManagementService.Data.CosmosDb.ClientFactories;
using DocumentManagementService.Data.CosmosDb.Initializers;
using DocumentManagementService.Domain;
using DocumentManagementService.Extensions;
using DocumentManagementService.FileStorage;
using DocumentManagementService.FileStorage.AzureBlobStorage;
using DocumentManagementService.FileStorage.AzureBlobStorage.ClientFactories;
using DocumentManagementService.FileStorage.AzureBlobStorage.Initializers;
using DocumentManagementService.Logger;
using DocumentManagementService.Logger.Serilog;
using DocumentManagementService.Logger.Serilog.Factories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;

namespace DocumentManagementService
{
    public class Startup
    {
        private const string CosmosDbEndpointKey = "CosmosDB:Endpoint";
        private const string CosmosDbAuthenticationKey = "CosmosDB:AuthenticationKey";
        private const string AzureStorageConnectionStringKey = "AzureStorage:ConnectionString";
        private const string LogFilePathKey = "ServiceLogging:LogFilePath";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "DocumentManagementService",
                        Version = "v1"
                    });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            services.AddScoped<IPdfDocumentHandler, PdfDocumentHandler>();

            services.AddScoped<IFileStorageHandler, AzureBlobStorageHandler>();
            services.AddSingleton<IAzureBlobClientFactory, AzureBlobClientFactory>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                return new AzureBlobClientFactory(configuration[AzureStorageConnectionStringKey]);
            });
            services.AddSingleton<IAzureBlobStorageInitializer, AzureBlobStorageInitializer>();

            services.AddScoped<IPdfDocumentsRepository, PdfDocumentsRepository>();
            services.AddSingleton<ICosmosDocumentClientFactory, CosmosDocumentClientFactory>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                return new CosmosDocumentClientFactory(configuration[CosmosDbEndpointKey], configuration[CosmosDbAuthenticationKey]);
            });
            services.AddSingleton<ICosmosDataInitializer, CosmosDataInitializer>();

            services.AddSingleton<IServiceLogger, SerilogServiceLogger>();
            services.AddSingleton<ISerilogServiceLoggerFactory, SerilogServiceLoggerFactory>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var logFilePath = configuration[LogFilePathKey];
                return string.IsNullOrEmpty(logFilePath)
                    ? new SerilogServiceLoggerFactory()
                    : new SerilogServiceLoggerFactory(logFilePath, RollingInterval.Day);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DocumentManagementService API v1");
            });

            app.UseBlobInitializer();
            app.UseCosmosDbInitializer();
        }
    }
}
