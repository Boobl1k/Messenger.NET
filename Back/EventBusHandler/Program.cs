using EventBusHandler.Context;
using EventBusHandler.Options;
using EventBusHandler.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services
    .Configure<BucketsOptions>(
        builder.Configuration.GetSection(BucketsOptions.Buckets))
    .Configure<S3Options>(
        builder.Configuration.GetSection(S3Options.S3))
    .Configure<MongoOptions>(
        builder.Configuration.GetSection(MongoOptions.MongoConfiguration))
    .Configure<RabbitOptions>(
        builder.Configuration.GetSection(RabbitOptions.RabbitConfiguration));

services.AddDbContext<AppDbContext>();
services.AddHostedService<BusHandlerService>();

var app = builder.Build();

app.MapGet("/", () => "EventBusHandler Service");

app.Run();