// https://opentelemetry.io/docs/languages/net/getting-started/

using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Globalization;

namespace opentelemetry_test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            const string serviceName = "roll-dice";

            builder.Logging.AddOpenTelemetry(options =>
            {
                options
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName))
                    .AddConsoleExporter();
            });
            builder.Services.AddOpenTelemetry()
                  .ConfigureResource(resource => resource.AddService(serviceName))
                  .WithTracing(tracing => tracing
                      .AddAspNetCoreInstrumentation()
                      .AddConsoleExporter())
                  .WithMetrics(metrics => metrics
                      .AddAspNetCoreInstrumentation()
                      .AddConsoleExporter());

            // Add services to the container.
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            string HandleRollDice([FromServices] ILogger<Program> logger, string? player)
            {
                var result = RollDice();

                if (string.IsNullOrEmpty(player))
                {
                    logger.LogInformation("Anonymous player is rolling the dice: {result}", result);
                }
                else
                {
                    logger.LogInformation("{player} is rolling the dice: {result}", player, result);
                }

                return result.ToString(CultureInfo.InvariantCulture);
            }

            int RollDice()
            {
                return Random.Shared.Next(1, 7);
            }

            app.MapGet("/rolldice/{player?}", HandleRollDice);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
