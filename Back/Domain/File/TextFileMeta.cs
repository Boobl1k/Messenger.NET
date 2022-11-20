namespace Domain.File;

public class TextFileMeta : IFileMeta
{
    public Guid Id { get; }
    public string Name { get; }
    public string Extension => "txt";

    public TextFileMeta(Guid id, string name)
    {
        this.Id = id;
        this.Name = name;
    }
}