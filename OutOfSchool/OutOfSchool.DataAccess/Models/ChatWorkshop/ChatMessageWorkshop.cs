using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models.ChatWorkshop;

public class ChatMessageWorkshop : IKeyedEntity<Guid>, ISoftDeleted
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    [Required]
    public Guid ChatRoomId { get; set; }

    [Required]
    [MaxLength(Constants.ChatMessageTextMaxLength)]
    public string Text { get; set; }

    [Required]
    public bool SenderRoleIsProvider { get; set; }

    [Required]
    public DateTimeOffset CreatedDateTime { get; set; }

    public DateTimeOffset? ReadDateTime { get; set; }

    public virtual ChatRoomWorkshop ChatRoom { get; set; }
}