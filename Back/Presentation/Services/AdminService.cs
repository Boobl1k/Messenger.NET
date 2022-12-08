using Microsoft.AspNetCore.SignalR;
using Presentation.Hubs;
using Presentation.Hubs.Clients;
using Presentation.RabbitMQ.Consumers;
using Presentation.RabbitMQ.Producer;

namespace Presentation.Services;

public class AdminService
{
    private readonly AdminProducer _adminProducer;
    private readonly AdminConsumer _adminConsumer;
    private readonly ILogger<AdminService> _logger;
    private readonly IHubContext<ChatHub> _hubContext;

    public AdminService(AdminProducer adminProducer, AdminConsumer adminConsumer, ILogger<AdminService> logger, IHubContext<ChatHub> hubContext)
    {
        _adminProducer = adminProducer;
        _adminConsumer = adminConsumer;
        _logger = logger;
        _hubContext = hubContext;
    }

    public Task FreeAdmin(string adminName)
    {
        _adminProducer.ProduceAdminFreeCommand(adminName);
        _logger.LogInformation("admin \"{admin name}\" free", adminName);
        return Task.CompletedTask;
    }

    public async Task<string> GetAdmin(string userName)
    {
        var adminName = _adminConsumer.ConsumeAdmin();
        await _hubContext.Clients.All.SendAsync(nameof(IChatClient.GoChat), adminName, userName);
        _logger.LogInformation("admin \"{admin name}\" is given to user", adminName);
        return adminName;
    }
}