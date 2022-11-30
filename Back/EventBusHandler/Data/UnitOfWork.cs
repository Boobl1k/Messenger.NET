using EventBusHandler.Data.Context;
using EventBusHandler.Data.Repository;
using EventBusHandler.Options;
using Microsoft.Extensions.Options;

namespace EventBusHandler.Data;

public class UnitOfWork
{
    private readonly MongoDbContext _context;
    private readonly IOptions<MongoOptions> _configuration;

    private SoundFileMetaRepository? _soundFileMetaRepository;
    private TextFileMetaRepository? _textFileMetaRepository;
    private VideoFileMetaRepository? _videoFileMetaRepository;

    public UnitOfWork(MongoDbContext context, IOptions<MongoOptions> configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public SoundFileMetaRepository SoundFileMetas =>
        _soundFileMetaRepository ??= new SoundFileMetaRepository(_context, _configuration);

    public TextFileMetaRepository TextFileMetas =>
        _textFileMetaRepository ??= new TextFileMetaRepository(_context, _configuration);

    public VideoFileMetaRepository VideoFileMetas =>
        _videoFileMetaRepository ??= new VideoFileMetaRepository(_context, _configuration);
}

public static class UnitOfWorkExtensions
{
    public static void AddUnitOfWork(this IServiceCollection services) =>
        services.AddScoped<UnitOfWork>();
}