using System;
using System.Collections.Generic;

using Bogus;

using OutOfSchool.WebApi.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    public static class ChatMessagesDtoGenerator
    {
        private static readonly Faker<ChatMessageDto> faker = new Faker<ChatMessageDto>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.Text, f => f.Random.Words())
            .RuleFor(x => x.CreatedTime, f => f.Date.RecentOffset());

        /// <summary>
        /// Creates new instance of the <see cref="ChatMessage"/> class with random data.
        /// </summary>
        /// <returns><see cref="ChatMessage"/> object.</returns>
        public static ChatMessageDto Generate() => faker.Generate();

        /// <summary>
        /// Generates a list of the <see cref="ChatMessage"/> objects.
        /// </summary>
        /// <param name="count">count of instances to generate.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ChatMessage"/> objects.</returns>
        public static List<ChatMessageDto> Generate(int count) => faker.Generate(count);

        public static ChatMessageDto WithUser(this ChatMessageDto chatMessage, UserDto user)
        {
            _ = chatMessage ?? throw new ArgumentNullException(nameof(chatMessage));

            chatMessage.UserId = user?.Id.ToString() ?? default;

            return chatMessage;
        }

        public static List<ChatMessageDto> WithUser(this List<ChatMessageDto> chatMessages, UserDto user)
        {
            _ = chatMessages ?? throw new ArgumentNullException(nameof(chatMessages));

            chatMessages.ForEach(x => x.WithUser(user));

            return chatMessages;
        }
    }
}
