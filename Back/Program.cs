using System.Reflection;
using Back;
using Back.Cucumbers;
using Back.Repositories;
using Back.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddDbContext<AppDbContext>();

services.AddScoped<MessagesRepository>();
services.AddScoped<MessagesService>();

services.AddMassTransit(config =>
{
    config.SetKebabCaseEndpointNameFormatter();
    config.SetInMemorySagaRepositoryProvider();

    config.AddConsumer<DefaultCucumber>();

    var assembly = Assembly.GetEntryAssembly();

    config.AddSagaStateMachines(assembly);
    config.AddSagas(assembly);
    config.AddActivities(assembly);

    config.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetService<AppDbContext>();
    await context!.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
    app.UseSwagger().UseSwaggerUI();

app.UseCors(b => b.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.Use(async (context, next) =>
{
    Console.WriteLine(context.Request.Path);

    if (context.Request.Path == "/")
    {
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync("Hello");
    }
    else
        await next();
});

app.UseHttpsRedirection()
    .UseAuthentication()
    .UseAuthorization();
app.MapControllers();
app.Run();