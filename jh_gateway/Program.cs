using jh_gateway.Middlewares;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Ocelot services
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();

const string policyName = "JS_POC_CORS";

builder.Services.AddCors(delegate (CorsOptions options) {
    options.AddPolicy(policyName, delegate (CorsPolicyBuilder builder) {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("ContentDisposition")
            .WithExposedHeaders("Authorization")
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10.0));
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(policyName);
app.UseJwtMiddleware(); // Use your custom middleware here

// Use Ocelot middleware
await app.UseOcelot();

app.UseAuthorization();


app.MapControllers();

app.Run();