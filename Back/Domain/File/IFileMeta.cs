namespace Domain.File;

public interface IFileMeta
{
    public Guid Id { get; }
    public string Name { get; }
    public string Extension { get; }
}