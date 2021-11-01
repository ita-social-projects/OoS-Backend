using System;
using System.Collections.Generic;

using Bogus;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    public static class ChatMessagesGenerator
    {
        private static readonly Faker<ChatMessage> faker = new Faker<ChatMessage>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.Text, f => f.Random.Words())
            .RuleFor(x => x.CreatedTime, f => f.Date.RecentOffset());

        /// <summary>
        /// Creates new instance of the <see cref="ChatMessage"/> class with random data.
        /// </summary>
        /// <returns><see cref="ChatMessage"/> object.</returns>
        public static ChatMessage Generate() => faker.Generate();

        /// <summary>
        /// Generates a list of the <see cref="ChatMessage"/> objects.
        /// </summary>
        /// <param name="count">count of instances to generate.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ChatMessage"/> objects.</returns>
        public static List<ChatMessage> Generate(int count) => faker.Generate(count);

        public static ChatMessage WithUser(this ChatMessage chatMessage, User user)
        {
            _ = chatMessage ?? throw new ArgumentNullException(nameof(chatMessage));

            chatMessage.User = user;
            chatMessage.UserId = user?.Id ?? default;

            return chatMessage;
        }

        public static List<ChatMessage> WithUser(this List<ChatMessage> chatMesages, User user)
        {
            _ = chatMesages ?? throw new ArgumentNullException(nameof(chatMesages));
            chatMesages.ForEach(chatMesage =>
            {
                chatMesage.User = user;
                chatMesage.UserId = user?.Id ?? default;
            });

            return chatMesages;
        }

        public static ChatMessage WithChatRoom(this ChatMessage chatMessage, ChatRoom chatRoom)
        {
            _ = chatMessage ?? throw new ArgumentNullException(nameof(chatMessage));

            chatMessage.ChatRoom = chatRoom;
            chatMessage.ChatRoomId = chatRoom?.Id ?? default;

            return chatMessage;
        }

        public static List<ChatMessage> WithChatRoom(this List<ChatMessage> chatMessages, ChatRoom chatRoom)
        {
            _ = chatMessages ?? throw new ArgumentNullException(nameof(chatMessages));
            chatMessages.ForEach(x => x.WithChatRoom(chatRoom));

            return chatMessages;
        }
    }
}
