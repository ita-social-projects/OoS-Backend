using System;
using System.Collections.Generic;

using Bogus;

using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class ChatMessagesGenerator
{
    private static readonly Faker<ChatMessageWorkshop> faker = new Faker<ChatMessageWorkshop>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.Text, f => f.Random.Words())
        .RuleFor(x => x.CreatedDateTime, f => f.Date.RecentOffset());

    /// <summary>
    /// Creates new instance of the <see cref="ChatMessageWorkshop"/> class with random data.
    /// </summary>
    /// <returns><see cref="ChatMessageWorkshop"/> object.</returns>
    public static ChatMessageWorkshop Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="ChatMessage"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="ChatMessage"/> objects.</returns>
    public static List<ChatMessageWorkshop> Generate(int count) => faker.Generate(count);

    public static ChatMessageWorkshop WithSenderRoleIsProvider(this ChatMessageWorkshop chatMessage, bool isProvider)
    {
        _ = chatMessage ?? throw new ArgumentNullException(nameof(chatMessage));

        chatMessage.SenderRoleIsProvider = isProvider;

        return chatMessage;
    }

    public static List<ChatMessageWorkshop> WithSenderRoleIsProvider(this List<ChatMessageWorkshop> chatMessages, bool isProvider)
    {
        _ = chatMessages ?? throw new ArgumentNullException(nameof(chatMessages));

        chatMessages.ForEach(x => x.WithSenderRoleIsProvider(isProvider));

        return chatMessages;
    }

    public static List<ChatMessageWorkshop> WithUser(this List<ChatMessageWorkshop> chatMessages, bool isProvider)
    {
        _ = chatMessages ?? throw new ArgumentNullException(nameof(chatMessages));

        chatMessages.ForEach(x => x.WithSenderRoleIsProvider(isProvider));

        return chatMessages;
    }

    public static ChatMessageWorkshop WithChatRoom(this ChatMessageWorkshop chatMessage, ChatRoomWorkshop chatRoom)
    {
        _ = chatMessage ?? throw new ArgumentNullException(nameof(chatMessage));

        chatMessage.ChatRoom = chatRoom;
        chatMessage.ChatRoomId = chatRoom?.Id ?? default;

        return chatMessage;
    }

    public static List<ChatMessageWorkshop> WithChatRoom(this List<ChatMessageWorkshop> chatMessages, ChatRoomWorkshop chatRoom)
    {
        _ = chatMessages ?? throw new ArgumentNullException(nameof(chatMessages));
        chatMessages.ForEach(x => x.WithChatRoom(chatRoom));

        return chatMessages;
    }

    public static ChatMessageWorkshop WithReadDateTime(this ChatMessageWorkshop chatMessage, DateTimeOffset? readDateTime)
    {
        _ = chatMessage ?? throw new ArgumentNullException(nameof(chatMessage));

        chatMessage.ReadDateTime = readDateTime;

        return chatMessage;
    }

    public static List<ChatMessageWorkshop> WithReadDateTime(this List<ChatMessageWorkshop> chatMessages, DateTimeOffset? readDateTime)
    {
        _ = chatMessages ?? throw new ArgumentNullException(nameof(chatMessages));

        chatMessages.ForEach(x => x.WithReadDateTime(readDateTime));

        return chatMessages;
    }
}