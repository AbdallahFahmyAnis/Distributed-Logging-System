// DistributedLoggingSystem.Web/Program.cs
using Distributed_Logging_System.Application.NewFolder;
using Distributed_Logging_System.Infrastructure.InfrastructureServiceCollectionExtensions;
using Distributed_Logging_System.WebServiceCollectionExtensions;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services
    .AddApplication() // Register application layer services
    .AddInfrastructure(builder.Configuration) // Register infrastructure layer services
    .AddWeb(builder.Configuration); // Register presentation layer services


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();