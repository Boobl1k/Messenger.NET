namespace Domain.File;

public class VideoFileMeta : IFileMeta
{
    public Guid Id { get; }
    public string Name { get; }
    public string Extension { get; }
    public string Studio { get; }
    public string Producer { get; }

    public VideoFileMeta(Guid id, string name, string extension, string studio, string producer)
    {
        extension = extension.ToLower();
        if (extension is not ("mp4" or "mov" or "avi")) throw new Exception("broken extension");
        this.Id = id;
        this.Name = name;
        this.Extension = extension;
        this.Studio = studio;
        this.Producer = producer;
    }
}