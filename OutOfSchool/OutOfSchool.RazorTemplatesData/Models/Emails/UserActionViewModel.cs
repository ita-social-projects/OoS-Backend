using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.RazorTemplatesData.Models.Emails
{
    public class UserActionViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ActionUrl { get; set; }
    }
}
