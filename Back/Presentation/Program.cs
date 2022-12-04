using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Presentation;
using Presentation.Hubs;
using Presentation.Options;
using Presentation.RabbitMQ.Producer;
using Presentation.Repositories;
using Presentation.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers().AddNewtonsoftJson();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.Configure<DbOptions>(builder.Configuration.GetSection(DbOptions.OptionsPath));

services.AddDbContext<AppDbContext>()
    .AddScoped<MessagesRepository>()
    .AddScoped<MessagesService>()
    .AddSingleton<MessagesProducer>()
    .AddSingleton<FileSaveCommandsProducer>()
    .AddSingleton<FilesService>()
    .AddSingleton<CacheService>();

var s3Options = builder.Configuration.GetSection(S3Options.OptionsPath).Get<S3Options>();
services.AddSingleton(new AmazonS3Client(s3Options.AccessKey, s3Options.SecretKey,
    new AmazonS3Config { ServiceURL = s3Options.Url, ForcePathStyle = true }));

var redisOptions = builder.Configuration.GetSection(RedisOptions.OptionsPath).Get<RedisOptions>();
services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisOptions.Configuration));

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