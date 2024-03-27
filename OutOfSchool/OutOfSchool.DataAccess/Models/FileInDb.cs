namespace OutOfSchool.Services.Models;
public class FileInDb : IKeyedEntity<string>
{
    public string Id { get; set; }

    public string ContentType { get; set; }

    public byte[] Data { get; set; }
}
