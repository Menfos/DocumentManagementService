using DocumentManagementService.BlobStorageService;
using DocumentManagementService.BlobStorageService.ClientFactories;
using DocumentManagementService.Data;
using DocumentManagementService.Data.CosmosDb.ClientFactories;
using DocumentManagementService.Extensions;
using DocumentManagementService.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DocumentManagementService
{
    public class Startup
    {
        private const string CosmosDbEndpointKey = "CosmosDB:Endpoint";
        private const string CosmosDbAuthenticationKey = "CosmosDB:AuthenticationKey";
        private const string AzureStorageConnectionStringKey = "AzureStorage:ConnectionString";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<ICosmosDocumentClientFactory, CosmosDocumentClientFactory>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                return new CosmosDocumentClientFactory(
                    configuration[CosmosDbEndpointKey],
                    configuration[CosmosDbAuthenticationKey]);
            });
            services.AddSingleton<IBlobClientFactory, BlobClientFactory>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                return new BlobClientFactory(configuration[AzureStorageConnectionStringKey]);
            });
            services.AddScoped<IBlobStorageService, BlobStorageService.BlobStorageService>();
            services.AddScoped<IPdfDocumentsRepository, PdfDocumentsRepository>();
            services.AddScoped<IPdfDocumentHandler, PdfDocumentHandler>();
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

            app.InitializeBlobStorage();
            app.InitializeCosmosDbStorage();
        }
    }
}
