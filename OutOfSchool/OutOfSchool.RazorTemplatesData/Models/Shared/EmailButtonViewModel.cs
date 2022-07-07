namespace OutOfSchool.RazorTemplatesData.Models.Shared;

public class EmailButtonViewModel
{
    public EmailButtonViewModel(string Text, string Url)
    {
        this.Text = Text;
        this.Url = Url;
    }

    public string Text { get; set; }
    public string Url { get; set; }
}