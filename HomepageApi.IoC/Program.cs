using HomepageApi.Api;
using HomepageApi.IoC;

using Microsoft.AspNetCore.Cors.Infrastructure;

public partial class Program
{
    private const string ProductionCorsPolicy = "fztgreen.dev";
    private const string DevelopmentCorsPolicy = "localhost";

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(AddCorsPolicies);
        builder.Services.AddApi();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        RegisterDependencies.Register(builder.Services);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors(DevelopmentCorsPolicy);
        }
        else
        {
            app.UseCors(ProductionCorsPolicy);
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    private static void AddCorsPolicies(CorsOptions options)
    {
        options.AddPolicy(ProductionCorsPolicy, policy => policy.WithOrigins("https://fztgreen.dev"));
        options.AddPolicy(DevelopmentCorsPolicy, policy => policy.WithOrigins("localhost"));
    }
}
