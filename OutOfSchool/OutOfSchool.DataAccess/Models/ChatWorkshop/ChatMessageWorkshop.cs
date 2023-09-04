using OutOfSchool.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace OutOfSchool.Services.Models.ChatWorkshop;

public class ChatMessageWorkshop : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    [Required]
    public Guid ChatRoomId { get; set; }

    [Required]
    [MaxLength(Constants.TextMaxLength)]
    public string Text { get; set; }

    [Required]
    public bool SenderRoleIsProvider { get; set; }

    [Required]
    public DateTimeOffset CreatedDateTime { get; set; }

    public DateTimeOffset? ReadDateTime { get; set; }

    public virtual ChatRoomWorkshop ChatRoom { get; set; }
}