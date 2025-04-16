using OutOfSchool.WebApi.Filters;

namespace OutOfSchool.WebApi.Extensions;

public static class EndpointFilterExtensions
{
    public static RouteGroupBuilder RequireTechAdmin(this RouteGroupBuilder group)
    {
        return group.AddEndpointFilter<TechAdminAccessFilter>();
    }
}