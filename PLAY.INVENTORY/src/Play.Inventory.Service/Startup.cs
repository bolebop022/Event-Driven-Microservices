

using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Play.Common.MongoDB;
using Play.Common.RabbitMQ;
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
                    .AddMongoRepository<InventoryItem>("inventoryitems")
                    .AddMongoRepository<CatalogItem>("catalogitems")
                    .AddMassTransitWithRabbitMQ();

            AddCatalogClient(services);

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
        private static void AddCatalogClient(IServiceCollection services)
        {
            Random jitterer = new Random();

            var handler = new HttpClientHandler();

            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            services.AddHttpClient<CatalogClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001");
                client.Timeout = TimeSpan.FromSeconds(35); // Set other HttpClient options if needed
            })
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
                onRetry: (outcome, timeSpan, retryAttempt) =>
                {
                    var serviceProvder = services.BuildServiceProvider();
                    serviceProvder.GetService<ILogger<CatalogClient>>()?
                      .LogWarning($"Delaying for {timeSpan.TotalSeconds} seconds, then making retry {retryAttempt}");
                }
            ))
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                3,
                TimeSpan.FromSeconds(15),
                onBreak: (outcome, timeSpan) =>
                {
                    var serviceProvder = services.BuildServiceProvider();
                    serviceProvder.GetService<ILogger<CatalogClient>>()?
                      .LogWarning($"Opening the circuit for {timeSpan} seconds...");
                },
                onReset: () =>
                {
                    var serviceProvder = services.BuildServiceProvider();
                    serviceProvder.GetService<ILogger<CatalogClient>>()?
                      .LogWarning($"Closing the circuit...");
                }
            ))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1))
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        }
    }
}
