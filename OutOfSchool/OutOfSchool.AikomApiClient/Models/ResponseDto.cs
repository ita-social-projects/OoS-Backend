namespace OutOfSchool.AikomApiClient.Models;

public class ResponseDto
{
    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public int? ErrorCode { get; set; }

    public object? Result { get; set; }

}
