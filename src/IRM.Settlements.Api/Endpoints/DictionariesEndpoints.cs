using IRM.Settlements.Api.Contracts.Dictionaries;
using IRM.Settlements.Api.Contracts.Validation;
using IRM.Settlements.Application.Common;
using IRM.Settlements.Application.Common.Querying;
using IRM.Settlements.Application.UseCases.Permissions;
using IRM.Settlements.Application.UseCases.ServiceCenters;
using IRM.Settlements.Application.UseCases.ServiceCompanies;
using IRM.Settlements.Application.UseCases.Mvz;
using Wolverine;

namespace IRM.Settlements.Api.Endpoints;

public static class DictionariesEndpoints
{
    private const int DefaultPageSize = 20;
    private const string JwtOrApiKey = "JwtOrApiKey";

    public static void MapDictionariesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/permissions",
            async (IMessageBus bus) =>
            {
                var query = new GetPermissionsQuery();
                var result = await bus.InvokeAsync<IEnumerable<string>>(query);
                return Results.Ok(result);
            });

        app.MapGet("/api/service-companies",
                async ([AsParameters] GetServiceCompaniesRequest request, IMessageBus bus) =>
                {
                    var query = new GetServiceCompaniesQuery(
                        request.Search,
                        request.PageSize ?? DefaultPageSize);

                    var result = await bus.InvokeAsync<GetServiceCompaniesResult>(query);
                    return Results.Ok(result);
                })
            .RequireAuthorization(JwtOrApiKey)
            .AddEndpointFilter<ValidationFilter<GetServiceCompaniesRequest>>();

        app.MapGet("/api/service-centers",
                async ([AsParameters] GetServiceCentersRequest request, IMessageBus bus) =>
                {
                    var query = new GetServiceCentersQuery(
                        request.Search,
                        request.ServiceCompanySapId!,
                        request.PageSize ?? DefaultPageSize);

                    var result = await bus.InvokeAsync<GetServiceCentersResult>(query);
                    return Results.Ok(result);
                })
            .RequireAuthorization(JwtOrApiKey)
            .AddEndpointFilter<ValidationFilter<GetServiceCentersRequest>>();

        app.MapGet("/api/mvz",
                async ([AsParameters] GetMvzListRequest request, IMessageBus bus) =>
                {
                    var query = new GetMvzListQuery(
                        request.Search,
                        new Paging(request.PageIndex ?? 0, request.PageSize ?? DefaultPageSize));

                    var result = await bus.InvokeAsync<GridResult<MvzItemResult>>(query);
                    return Results.Ok(result);
                })
            .RequireAuthorization(JwtOrApiKey)
            .AddEndpointFilter<ValidationFilter<GetMvzListRequest>>();

        app.MapPut("/api/mvz/{shopName}",
                async (UpdateMvzItemRequest request, string shopName, IMessageBus bus) =>
                {
                    var command = new UpdateMvzItemCommand(
                        shopName,
                        request.MvzCode,
                        request.MvzName);

                    var result = await bus.InvokeAsync<MvzItemResult>(command);
                    return Results.Ok(result);
                })
            .RequireAuthorization(JwtOrApiKey)
            .AddEndpointFilter<ValidationFilter<UpdateMvzItemRequest>>();
    }
}
