using System;

namespace OutOfSchool.Services.Models.ChatWorkshop
{
    public class ParentInfoForChatList
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public string FirstName { get; set; }
    }
}
