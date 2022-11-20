using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Presentation;
using Presentation.Hubs;
using Presentation.RabbitMQ.Producer;
using Presentation.Repositories;
using Presentation.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers().AddNewtonsoftJson();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddDbContext<AppDbContext>();

services.AddScoped<MessagesRepository>();
services.AddScoped<MessagesService>();
services.AddSingleton<MessagesProducer>();
services.AddSingleton<FileSaveCommandsProducer>();
services.AddSingleton<AmazonS3Client>(new AmazonS3Client("qweqweqwe", "qweqweqwe",
    new AmazonS3Config { ServiceURL = "http://minio:9000", ForcePathStyle = true }));
services.AddSingleton<FilesService>();
services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("cache"));
services.AddSingleton<CacheService>();

// SignalR
services.AddSignalR(opt => { opt.EnableDetailedErrors = true; });

services.AddCors(options =>
{
    options.AddPolicy("ClientPermission", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins("http://localhost", "http://192.168.76.216")
            .AllowCredentials();
    });
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetService<AppDbContext>();
    await context!.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
    app.UseSwagger().UseSwaggerUI();

app
    // .UseHttpsRedirection()
    .UseAuthentication()
    .UseAuthorization();

app.UseCors("ClientPermission");

app.MapControllers();

// SignalR
app.MapHub<ChatHub>("/chat");

app.Run();