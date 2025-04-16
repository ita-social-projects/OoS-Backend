namespace OutOfSchool.WebApi.Filters;

public class TechAdminAccessFilter : IEndpointFilter
{
    private readonly ICurrentUserService currentUserService;

    public TechAdminAccessFilter(ICurrentUserService currentUserService)
    {
        this.currentUserService = currentUserService;
    }
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {

        if (!currentUserService.IsTechAdmin())
        {
            return Results.Forbid();
        }

        return await next(context);
    }
}
