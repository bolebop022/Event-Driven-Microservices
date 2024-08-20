

using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Play.Common.MongoDB;
using Play.Inventory.Serivce.Clients;
using Play.Inventory.Serivce.Entities;
using Polly;
using Polly.Timeout;

namespace Play.Inventory.Service
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

            services.AddMongo()
                    .AddMongoRepository<InventoryItem>("inventoryitems");

    #if DEBUG
            var certificatePath = "/home/bonganelebopo/dev/PLAY.INVENTORY/src/https.pfx";
            var certificatePassword = "GrushiMaclaude24!";
            
            var certificate = new X509Certificate2(certificatePath, certificatePassword);
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(certificate);
            
            // Console.WriteLine("Certificate Subject: " + certificate.Subject);
            // Console.WriteLine("Certificate Issuer: " + certificate.Issuer);
            // Console.WriteLine("Certificate Valid From: " + certificate.NotBefore);
            // Console.WriteLine("Certificate Valid Until: " + certificate.NotAfter);

            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            services.AddHttpClient<CatalogClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001");
                client.Timeout = TimeSpan.FromSeconds(30); // Set other HttpClient options if needed
            })
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
            ))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1))
            .ConfigurePrimaryHttpMessageHandler(() => handler);
    #endif

            // services.AddHttpClient<CatalogClient>(client =>
            // {
            //     client.BaseAddress = new Uri("https://localhost:5001");
            // });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Inventory.Service", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Inventory.Service v1"));
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
