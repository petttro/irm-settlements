using IRM.Settlements.Api.Contracts.Reports;
using IRM.Settlements.Api.Contracts.Validation;
using IRM.Settlements.Application.Common;
using IRM.Settlements.Application.Common.Querying;
using IRM.Settlements.Application.UseCases.PaymentOrders;
using IRM.Settlements.Application.UseCases.Reports.Commands;
using IRM.Settlements.Application.UseCases.Reports.Queries;
using IRM.Settlements.Application.UseCases.Reports.Results;
using IRM.Settlements.Domain.Enums;
using Wolverine;

namespace IRM.Settlements.Api.Endpoints;

public static class ReportsEndpoints
{
    private const int DefaultPageIndex = 0;
    private const int DefaultPageSize = 20;

    private static readonly HashSet<string> AllowedReportSort = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(ReportResult.ServiceCompanyName),
        nameof(ReportResult.CreatedAt),
        nameof(ReportResult.SentToPaymentDate),
        nameof(ReportResult.PaymentDate)
    };

    private static readonly HashSet<string> AllowedReportItemsSort = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(ReportItemResult.CouponNumber),
        nameof(ReportItemResult.SaleOrderNumber),
        nameof(ReportItemResult.OrderNumber),
        nameof(ReportItemResult.ServiceCenterName),
        nameof(ReportItemResult.ServiceDate),
        nameof(ReportItemResult.CityName)
    };

    public static void MapReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports")
            .RequireAuthorization("JwtOrApiKey");

        group.MapPost("/",
                async (CreateReportRequest request, IMessageBus bus) =>
                {
                    var command = new CreateReportCommand(
                        request.ServiceCompanySapId!,
                        request.ServiceCenters?.Distinct().ToList() ?? [],
                        request.ReportName,
                        new DateOnlyRange(request.ServiceDateFrom, request.ServiceDateTo));

                    var result = await bus.InvokeAsync<ReportResult>(command);
                    return Results.Ok(result);
                })
            .WithDescription("Создание нового отчета")
            .AddEndpointFilter<ValidationFilter<CreateReportRequest>>();

        group.MapGet("/",
                async ([AsParameters] GetReportsRequest request, IMessageBus bus) =>
                {
                    var hasStatusFilter = Enum.TryParse<ReportStatus>(request.Status, ignoreCase: true, out var status);

                    var query = new GetReportsQuery(
                        request.ServiceCompanyId,
                        request.Search,
                        hasStatusFilter ? status : null,
                        new DateTimeRange(request.CreatedDateFrom, request.CreatedDateTo),
                        new DateTimeRange(request.SentToPaymentDateFrom, request.SentToPaymentDateTo),
                        new DateTimeRange(request.PaymentDateFrom, request.PaymentDateTo),
                        new Paging(request.PageIndex ?? DefaultPageIndex, request.PageSize ?? DefaultPageSize),
                        SortParser.Parse(request.Sort, AllowedReportSort)
                    );

                    var result = await bus.InvokeAsync<GridResult<ReportResult>>(query);
                    return Results.Ok(result);
                })
            .WithDescription("Получение списка отчетов")
            .AddEndpointFilter<ValidationFilter<GetReportsRequest>>();

        group.MapGet("/export",
                async ([AsParameters] ExportReportsRequest request, IMessageBus bus) =>
                {
                    var hasStatusFilter = Enum.TryParse<ReportStatus>(request.Status, ignoreCase: true, out var status);

                    var query = new ExportReportsQuery(
                        request.ServiceCompanyId,
                        hasStatusFilter ? status : null,
                        new DateTimeRange(request.CreatedDateFrom, request.CreatedDateTo),
                        new DateTimeRange(request.SentToPaymentDateFrom, request.SentToPaymentDateTo),
                        new DateTimeRange(request.PaymentDateFrom, request.PaymentDateTo));

                    var result = await bus.InvokeAsync<FileResult>(query);

                    return Results.File(result.Data, result.ContentType, result.FileName);
                })
            .WithDescription("Экспорт списка отчетов в Excel")
            .AddEndpointFilter<ValidationFilter<ExportReportsRequest>>();

        group.MapGet("/{reportId:guid}",
                async (Guid reportId, IMessageBus bus) =>
                {
                    var query = new GetReportQuery(reportId);
                    var result = await bus.InvokeAsync<ReportResult>(query);
                    return Results.Ok(result);
                })
            .WithDescription("Получение одного отчета");

        group.MapGet("/{reportId:guid}/export",
                async (Guid reportId, IMessageBus bus) =>
                {
                    var query = new ExportReportQuery(reportId);
                    var result = await bus.InvokeAsync<FileResult>(query);

                    return Results.File(result.Data, result.ContentType, result.FileName);
                })
            .WithDescription("Получение одного отчета");

        group.MapGet("/{reportId:guid}/items",
                async ([AsParameters] GetReportItemsRequest request, Guid reportId, IMessageBus bus) =>
                {
                    var query = new GetReportItemsQuery(
                        reportId,
                        new Paging(request.PageIndex ?? DefaultPageIndex, request.PageSize ?? DefaultPageSize),
                        SortParser.Parse(request.Sort, AllowedReportItemsSort));

                    var result = await bus.InvokeAsync<ReportItemsResult>(query);
                    return Results.Ok(result);
                })
            .WithDescription("Получение позиций отчета");

        group.MapPut("/{reportId:guid}",
                async (Guid reportId, UpdateReportRequest request, IMessageBus bus) =>
                {
                    var command = new UpdateReportCommand(
                        reportId,
                        request.ReportName,
                        request.ServiceCenters?.Distinct().ToList() ?? [],
                        new DateOnlyRange(request.ServiceDateFrom, request.ServiceDateTo));

                    var result = await bus.InvokeAsync<ReportResult>(command);
                    return Results.Ok(result);
                })
            .WithDescription("Пересоздание отчета с новыми параметрами")
            .AddEndpointFilter<ValidationFilter<UpdateReportRequest>>();

        group.MapDelete("/{reportId:guid}/items/{reportItemId:guid}",
                async (Guid reportId, Guid reportItemId, IMessageBus bus) =>
                {
                    var command = new DeleteReportItemCommand(reportId, reportItemId);
                    var result = await bus.InvokeAsync<ReportResult>(command);

                    return Results.Ok(result);
                })
            .WithDescription("Удалить позицию отчета");

        group.MapPatch("/{reportId:guid}",
                async (Guid reportId, UpdateReportItemsRequest request, IMessageBus bus) =>
                {
                    var operations = request.Operations
                        .Select(requestOperation => new ReportItemOperation
                        {
                            CouponNumber = requestOperation.CouponNumber,
                            Type = requestOperation.Type
                        })
                        .ToList();

                    var command = new UpdateReportItemsCommand(reportId, operations);

                    var result = await bus.InvokeAsync<ReportResult>(command);
                    return Results.Ok(result);
                })
            .WithDescription("Изменить позиции отчета")
            .AddEndpointFilter<ValidationFilter<UpdateReportItemsRequest>>();

        group.MapDelete("/{reportId:guid}",
                async (Guid reportId, IMessageBus bus) =>
                {
                    var command = new DeleteReportCommand(reportId);
                    await bus.InvokeAsync<ReportResult>(command);

                    return Results.Ok();
                })
            .WithDescription("Удалить отчет");

        group.MapPost("/{reportId:guid}/send-to-payment",
                async (Guid reportId, IMessageBus bus) =>
                {
                    var command = new SendToPaymentCommand(reportId);
                    var result = await bus.InvokeAsync<ReportResult>(command);

                    return Results.Ok(result);
                })
            .WithDescription("Отправить отчет на оплату");

        group.MapPost("/{reportId:guid}/confirm-payment",
                async (Guid reportId, IMessageBus bus) =>
                {
                    var command = new ConfirmPaymentCommand(reportId);
                    var result = await bus.InvokeAsync<ReportResult>(command);

                    return Results.Ok(result);
                })
            .WithDescription("Перевести отчет в статус 'Оплачен'");

        group.MapPost("/{reportId:guid}/recalculate-cost",
                async (Guid reportId, IMessageBus bus) =>
                {
                    var command = new RecalculateCostCommand(reportId);
                    var result = await bus.InvokeAsync<ReportResult>(command);

                    return Results.Ok(result);
                })
            .WithDescription("Перерасчет цены");

        app.MapPost("/api/payment-orders/export",
                async (ExportPaymentOrderRequest request, IMessageBus bus) =>
                {
                    var query = new ExportPaymentOrderQuery(request.ReportIds);
                    var result = await bus.InvokeAsync<FileResult>(query);

                    return Results.File(result.Data, result.ContentType, result.FileName);
                })
            .RequireAuthorization("JwtOrApiKey")
            .AddEndpointFilter<ValidationFilter<ExportPaymentOrderRequest>>();
    }
}
