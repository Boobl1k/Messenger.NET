namespace Domain.File;

public class SoundFileMeta : IFileMeta
{
    public Guid Id { get; }
    public string Name { get; }
    public string Extension => "mp3";
    public string Author { get; }
    public string Album { get; }

    public SoundFileMeta(Guid id, string name, string author, string album)
    {
        this.Id = id;
        this.Name = name;
        this.Author = author;
        this.Album = album;
    }
}