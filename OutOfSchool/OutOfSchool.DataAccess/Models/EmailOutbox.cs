using System;

namespace OutOfSchool.Services.Models;
public class EmailOutbox : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    public string Email { get; set; }

    public string Subject { get; set; }

    public string HtmlContent { get; set; }

    public string PlainContent { get; set; }

    public DateTime CreationTime { get; set; }
}
