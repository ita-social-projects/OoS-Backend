namespace OutOfSchool.RazorTemplatesData.Models.Emails
{    public class ConfirmEmailViewModel
    {

        public ConfirmEmailViewModel(string ConfirmEmailUrl)
        {
            this.ConfirmEmailUrl = ConfirmEmailUrl;
        }

        public string ConfirmEmailUrl { get; set; }
    }
}