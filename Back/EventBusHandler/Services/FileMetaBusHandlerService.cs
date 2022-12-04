using EventBusHandler.Services.Consumer;

namespace EventBusHandler.Services;

internal class FileMetaBusHandlerService : BackgroundService
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(1));
    private readonly ILogger<FileMetaBusHandlerService> _logger;

    private readonly FileMetaConsumer _fileMetaConsumer;

    public FileMetaBusHandlerService(ILogger<FileMetaBusHandlerService> logger, FileMetaConsumer fileMetaConsumer)
    {
        _logger = logger;
        _fileMetaConsumer = fileMetaConsumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            try
            {
                _fileMetaConsumer.StartConsumingFileMetas();
            }
            catch (Exception e)
            {
                _logger.LogError("😭 ERROR: \'{ErrorMessage}\'", e.Message);
                _fileMetaConsumer.Dispose();
            }
        } while (await _timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
    }

    public override void Dispose()
    {
        _fileMetaConsumer.Dispose();
        base.Dispose();
    }
}