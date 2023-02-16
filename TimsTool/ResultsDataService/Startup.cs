using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ResultsDataService.Middleware;
using Microsoft.Extensions.Azure;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using Azure.Core.Extensions;
using System;

namespace ResultsDataService
{
    /// <summary>
    /// Starup processing and initialisation
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Configuration property to access app settings
        /// </summary>
        public IConfiguration Configuration { get; }
 
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            var isTest = Configuration.GetValue<bool>("IsTest");
            if (isTest)
            {
                services.AddSingleton(x => new CoreBlobClient(Configuration.GetValue<string>("AzureBlobStorageTestConnectionString")));
            }
            else
            {
                services.AddSingleton(x => new CoreBlobClient(Configuration.GetValue<string>("AzureBlobStorageProdConnectionString")));
            }
            services.AddSingleton(x => new BackupBlobClient(Configuration.GetValue<string>("AzureBlobStorageBackupConnectionString")));
            services.AddSingleton<ICoreBlobService, CoreBlobService>();
            services.AddSingleton<IBackupBlobService, BackupBlobService>();

            services.AddSingleton(Configuration);
            services.AddControllers();  //might need services.AddControllers().AddNewtonsoftJson() from `Microsoft.AspNetCore.Mvc.Newtonsoft.json.

            services.AddMvcCore()
                    .AddApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ResultsDataService",
                    Description = "A service for retrieving and uploading results data with respect to Tim's Tool",
                    Contact = new OpenApiContact
                    {
                        Name = "Tim Cooke",
                        Email = "cooketim@hotmail.com"
                    }
                });


                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "XApiKey",
                    Description = "ApiKey must appear in header",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "ApiKeyScheme",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    }
                };

                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                 {
                     {securityScheme, new string[] { }}
                 });
            });
            services.AddAzureClients(builder =>
            {
                builder.AddBlobServiceClient(Configuration["AzureBlobStorageTestConnectionString:blob"], preferMsi: true);
                builder.AddQueueServiceClient(Configuration["AzureBlobStorageTestConnectionString:queue"], preferMsi: true);
            });
            //services.AddAzureClients(builder =>
            //{
            //    builder.AddBlobServiceClient(Configuration["AzureBlobStorageTestConnectionString:blob"], preferMsi: true);
            //    builder.AddQueueServiceClient(Configuration["AzureBlobStorageTestConnectionString:queue"], preferMsi: true);
            //});
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Showing API V1");
                //c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseMiddleware<ApiKeyMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    internal static class StartupExtensions
    {
        public static IAzureClientBuilder<BlobServiceClient, BlobClientOptions> AddBlobServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
        {
            if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
            {
                return builder.AddBlobServiceClient(serviceUri);
            }
            else
            {
                return builder.AddBlobServiceClient(serviceUriOrConnectionString);
            }
        }
        public static IAzureClientBuilder<QueueServiceClient, QueueClientOptions> AddQueueServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
        {
            if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
            {
                return builder.AddQueueServiceClient(serviceUri);
            }
            else
            {
                return builder.AddQueueServiceClient(serviceUriOrConnectionString);
            }
        }
    }
}
