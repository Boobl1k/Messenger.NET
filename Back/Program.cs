using Back;
using Back.Repositories;
using Back.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddDbContext<AppDbContext>();

services.AddScoped<MessagesRepository>();
services.AddScoped<MessagesService>();

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