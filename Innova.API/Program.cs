using Innova.API.Extensions;
using Innova.Infrastructure.Extensions;
using Innova.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.ConfigureSerilog();

builder.Services.ConfigureInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureCors();

builder.Services.ConfigureServices();

var app = builder.Build();

// Global exception handling middleware
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
