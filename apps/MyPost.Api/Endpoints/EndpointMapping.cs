namespace MyPost.Api.Endpoints;

public static class EndpointMapping
{
    public static IEndpointRouteBuilder MapMyPostEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api/v1");
        api.MapAuthEndpoints();
        api.MapCustomerEndpoints();
        api.MapPublicEndpoints();
        api.MapCourierEndpoints();
        api.MapAdminEndpoints();
        return endpoints;
    }
}
