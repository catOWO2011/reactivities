using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>(opt =>
{
    string pgHost = Environment.GetEnvironmentVariable("DATABASE_HOST");
    string pgPort = Environment.GetEnvironmentVariable("DATABASE_PORT");
    string pgUser = Environment.GetEnvironmentVariable("DATABASE_USER");
    string pgPass = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
    string pgDb = Environment.GetEnvironmentVariable("DATABASE_NAME");

    string connectionString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};";

    opt.UseNpgsql(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.(middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedData(context);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}

app.Run();