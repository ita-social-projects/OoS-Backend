using System;
using System.Collections.Generic;

using Bogus;

using OutOfSchool.WebApi.Models.ChatWorkshop;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    public static class ChatMessagesDtoGenerator
    {
        private static readonly Faker<ChatMessageWorkshopDto> faker = new Faker<ChatMessageWorkshopDto>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.Text, f => f.Random.Words())
            .RuleFor(x => x.CreatedDateTime, f => f.Date.RecentOffset());

        /// <summary>
        /// Creates new instance of the <see cref="ChatMessage"/> class with random data.
        /// </summary>
        /// <returns><see cref="ChatMessage"/> object.</returns>
        public static ChatMessageWorkshopDto Generate() => faker.Generate();

        /// <summary>
        /// Generates a list of the <see cref="ChatMessage"/> objects.
        /// </summary>
        /// <param name="count">count of instances to generate.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ChatMessage"/> objects.</returns>
        public static List<ChatMessageWorkshopDto> Generate(int count) => faker.Generate(count);

        public static ChatMessageWorkshopDto WithSenderRoleIsProvider(this ChatMessageWorkshopDto chatMessage, bool isProvider)
        {
            _ = chatMessage ?? throw new ArgumentNullException(nameof(chatMessage));

            chatMessage.SenderRoleIsProvider = isProvider;

            return chatMessage;
        }

        public static List<ChatMessageWorkshopDto> WithSenderRoleIsProvider(this List<ChatMessageWorkshopDto> chatMessages, bool isProvider)
        {
            _ = chatMessages ?? throw new ArgumentNullException(nameof(chatMessages));

            chatMessages.ForEach(x => x.WithSenderRoleIsProvider(isProvider));

            return chatMessages;
        }
    }
}
