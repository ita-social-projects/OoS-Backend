using System;
namespace OutOfSchool.Services.Models;
public class EmailOutbox : IKeyedEntity<long>
{
    public long Id { get; set; }

    public string Email { get; set; }

    public string Subject { get; set; }

    public string HtmlContent { get; set; }

    public string PlainContent { get; set; }

    public DateTimeOffset CreationTime { get; set; }

    public DateTimeOffset ExpirationTime { get; set; }
}
