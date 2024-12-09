using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Serilog;

namespace Neoron.API;

public class Program
{
    public static void Main(string[] args)
    {
        // Use bootstrap logger during startup
        Log.Logger = LoggingExtensions.CreateBootstrapLogger();

        try
        {
            Log.Information("Starting up Neoron API");
            
            var builder = WebApplication.CreateBuilder(args);
            
            // Add enhanced Serilog configuration
            builder.Host.AddCustomLogging();
            
            Log.Information("Configuring application services...");
            
            builder.AddServiceDefaults();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.MapDefaultEndpoints();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            // Add diagnostic middleware
            app.Use(async (context, next) =>
            {
                Log.Debug("Processing request: {Method} {Path}", 
                    context.Request.Method, 
                    context.Request.Path);
                await next();
            });

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
