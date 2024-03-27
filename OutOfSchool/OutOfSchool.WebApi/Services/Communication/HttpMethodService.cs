namespace OutOfSchool.WebApi.Services.Communication;

public enum HttpMethodType
{
    Post,
    Put,
    Delete,
    Get,
}

public static class HttpMethodService
{
    public static HttpMethod GetHttpMethodType(Request request)
    {
        switch (request.HttpMethodType)
        {
            case HttpMethodType.Post:
                return HttpMethod.Post;
            case HttpMethodType.Put:
                return HttpMethod.Put;
            case HttpMethodType.Delete:
                return HttpMethod.Delete;
            default:
                return HttpMethod.Get;
        }
    }
}