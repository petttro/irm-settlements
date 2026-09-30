using ClosedXML.Excel;
using IRM.Settlements.Application.Abstractions.Excel;
using IRM.Settlements.Domain.Entities;
using IRM.Settlements.Domain.ValueObjects;
using IRM.Settlements.Infrastructure.Common.Extensions;
using Microsoft.Extensions.Options;

namespace IRM.Settlements.Infrastructure.Excel;

public class ExcelExporter : IExcelExporter
{
    private const string ReportListToXlsName = "Выгрузка по взаиморасчетам";
    private const string ReportTemplateFilePath = "Excel/Templates";
    private const string ReportTemplateFileName = "ReportXLSTemplate.xlsx";

    private readonly string _protectionPassword;

    public ExcelExporter(IOptions<ExcelExportSettings> options)
    {
        _protectionPassword = options.Value.ProtectionPassword;
    }

    public byte[] ReportListToXls(List<Report> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(ReportListToXlsName);

        var headers = new[]
        {
            "Название",
            "Контрагент",
            "Дата создания отчета",
            "Дата отправки на оплату",
            "Дата оплаты",
            "Статус отчета",
            "Количество позиций",
            "Сумма"
        };

        var columnCount = headers.Length;

        sheet.Cell(1, 1).Value = ReportListToXlsName;
        sheet.Range(1, 1, 1, columnCount).Merge();

        for (var i = 0; i < columnCount; i++)
        {
            var cell = sheet.Cell(3, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        var rowIndex = 4;

        foreach (var rep in rows)
        {
            // "Название"
            sheet.Cell(rowIndex, 1).Value = rep.Name;
            // "Контрагент"
            sheet.Cell(rowIndex, 2).Value = rep.ServiceCompanyName;
            // "Дата создания отчета"
            if (rep.CreatedAt != default)
            {
                sheet.Cell(rowIndex, 3).Style.DateFormat.Format = "dd.MM.yyyy";
                sheet.Cell(rowIndex, 3).Value = rep.CreatedAt;
            }
            // "Дата отправки на оплату"
            if (rep.SentToPaymentDate.HasValue)
            {
                sheet.Cell(rowIndex, 4).Style.DateFormat.Format = "dd.MM.yyyy";
                sheet.Cell(rowIndex, 4).Value = rep.SentToPaymentDate.Value;
            }
            // "Дата оплаты"
            if (rep.PaymentDate.HasValue)
            {
                sheet.Cell(rowIndex, 5).Style.DateFormat.Format = "dd.MM.yyyy";
                sheet.Cell(rowIndex, 5).Value = rep.PaymentDate.Value;
            }
            // "Статус отчета"
            sheet.Cell(rowIndex, 6).Value = rep.Status.ToString();
            // "Количество позиций"
            sheet.Cell(rowIndex, 7).Value = rep.ItemsCount;
            // "Сумма"
            sheet.Cell(rowIndex, 8).Value = rep.TotalCost;

            rowIndex++;
        }

        sheet.Columns(1, columnCount).Width = 20;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return stream.ToArray();
    }

    public byte[] ReportToXls(Report report, ServiceCompanyDetails serviceCompany)
    {
        var path = Path.Combine(AppContext.BaseDirectory, ReportTemplateFilePath, ReportTemplateFileName);
        using var workbook = new XLWorkbook(path);

        GenerateReportSheet_Report(workbook, report, serviceCompany);
        GenerateReportSheet_Act(workbook, report, serviceCompany);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return stream.ToArray();
    }

    private void GenerateReportSheet_Report(XLWorkbook workbook, Report report, ServiceCompanyDetails serviceCompany)
    {
        var sheet = workbook.Worksheet("Отчет");
        sheet.Protect(_protectionPassword);

        Unlock(sheet.Row(1).Cells(10, 13));
        Unlock(sheet.Row(2).Cell(10));
        Unlock(sheet.Row(3).Cells(10, 13));

        sheet.Row(2).Cell(10).SetValue($"№ договора {serviceCompany.ExternalContractNumber} от {serviceCompany.ContractDate:dd.MM.yyyy}");
        sheet.Row(3).Cell(9).SetValue($"с {report.ServiceDateFrom:dd.MM.yyyy} по {report.ServiceDateTo:dd.MM.yyyy}");

        var startRow = 6;

        var totalRows = report.Items.Sum(i => 1 + i.AdditionalServices.Count(s => s.Cost > 0));
        if (totalRows > 1)
            sheet.Row(startRow).InsertRowsBelow(totalRows);

        var dataRowIndex = startRow;
        var number = 1;

        IXLRange? tableRange;

        foreach (var item in report.Items)
        {
            var firstRowIndex = dataRowIndex;
            var row = sheet.Row(dataRowIndex);

            // Основная услуга
            row.Cell(2).SetValue(number++);
            row.Cell(3).SetValue(item.CouponNumber);
            row.Cell(4).SetValue(item.OrderNumber);

            row.Cell(5).Style.DateFormat.Format = "dd.MM.yyyy";
            row.Cell(5).SetValue(item.SaleDate);

            row.Cell(6).SetValue(item.ServiceDate.ToString("dd.MM.yyyy"));

            var serviceCell = row.Cell(7);
            serviceCell.Style.Alignment.WrapText = true;
            serviceCell.SetValue(item.ServiceName);

            row.Cell(8).SetValue(item.Cost);
            row.Cell(9).SetValue(item.ConfirmType?.ToString() ?? string.Empty);

            Unlock(row.Cell(10));

            row.Cell(11).SetValue(item.CheckNumber);
            row.Cell(12).SetValue(item.ShopName);
            row.Cell(13).SetValue(item.CityName);

            dataRowIndex++;

            // Дополнительные услуги
            if (item.AdditionalServices is { Count: > 0 })
            {
                foreach (var additionalService in item.AdditionalServices)
                {
                    if (additionalService.Cost == 0)
                        continue;

                    var addRow = sheet.Row(dataRowIndex);
                    addRow.Cell(7).Style.Alignment.WrapText = true;
                    addRow.Cell(7).SetValue(additionalService.Name);
                    addRow.Cell(8).SetValue(additionalService.Cost);

                    dataRowIndex++;
                }

                // объединить колонки для основной части
                sheet.Range(firstRowIndex, 2, dataRowIndex - 1, 2).Merge()
                    .Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; // №
                sheet.Range(firstRowIndex, 3, dataRowIndex - 1, 3).Merge()
                    .Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; // Coupon
                sheet.Range(firstRowIndex, 4, dataRowIndex - 1, 4).Merge()
                    .Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                sheet.Range(firstRowIndex, 5, dataRowIndex - 1, 5).Merge()
                    .Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                sheet.Range(firstRowIndex, 6, dataRowIndex - 1, 6).Merge()
                    .Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                sheet.Range(firstRowIndex, 9, dataRowIndex - 1, 9).Merge()
                    .Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                sheet.Range(firstRowIndex, 10, dataRowIndex - 1, 10).Merge()
                    .Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                sheet.Range(firstRowIndex, 11, dataRowIndex - 1, 11).Merge()
                    .Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                sheet.Range(firstRowIndex, 12, dataRowIndex - 1, 12).Merge()
                    .Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                sheet.Range(firstRowIndex, 13, dataRowIndex - 1, 13).Merge()
                    .Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
        }

        // собираем диапазон таблицы
        tableRange = sheet.Range(startRow, 1, dataRowIndex - 1, 13);
        // Обводка таблицы
        ApplyThinBorder(tableRange);

        sheet.Row(dataRowIndex + 3).Cell(4).SetValue(report.ServiceCompanyName);

        Unlock(sheet.Row(dataRowIndex + 5).Cell(4));
        Unlock(sheet.Row(dataRowIndex + 5).Cell(14));
    }

    private void GenerateReportSheet_Act(XLWorkbook workbook, Report report, ServiceCompanyDetails serviceCompany)
    {
        var sheet = workbook.Worksheet("Акт");
        sheet.Protect(_protectionPassword);

        var hasContract = !string.IsNullOrWhiteSpace(serviceCompany.ExternalContractNumber);
        var contractNumber = serviceCompany.ExternalContractNumber;
        var contractDate = serviceCompany.ContractDate;

        // Header
        Unlock(sheet.Row(1).Cell(5));
        Unlock(sheet.Row(2).Cell(5));

        sheet.Row(2).Cell(5).SetValue(hasContract ? contractNumber : string.Empty);

        if (contractDate.HasValue)
        {
            Unlock(sheet.Row(3).Cell(5));
            sheet.Row(3).Cell(5)
                .SetValue($"\"{contractDate:dd}\" {contractDate.GetMonthGenitiveName()} {contractDate:yyyy} г.");
        }

        Unlock(sheet.Row(4).Cells(3, 4));

        // Placeholder block (row 7)
        var range = sheet.Row(7).Cells(1, 5);

        var value = sheet.Row(7).Cell(1).GetString()
            .Replace("{ServiceCompany}", report.ServiceCompanyName)
            .Replace("{ContractNumber}", hasContract ? contractNumber : "___________")
            .Replace("{ContractDate}", contractDate.HasValue ? contractDate.Value.ToString("dd.MM.yyyy") : "______________");

        Unlock(range);
        range.Value = value;

        // Aggregates table
        var aggregates = GroupByWareCode(report.Items).ToList();

        // Пункт 1.1
        sheet.Row(13).Cell(3).SetValue(aggregates.Sum(a => a.Count));
        sheet.Row(13).Cell(4).SetValue(report.TotalCost);

        var startRow = 14;
        var rowsCount = aggregates.Count;

        if (rowsCount > 1)
            sheet.Row(startRow).InsertRowsBelow(rowsCount - 1);

        var rowIndex = startRow;
        var count = 1;

        foreach (var aggregate in aggregates)
        {
            var row = sheet.Row(rowIndex);

            row.Cell(1).SetValue($"1.1.{count++}");
            row.Cell(2).SetValue(aggregate.ServiceName);
            row.Cell(2).Style.Alignment.WrapText = true;
            row.Cell(3).SetValue(aggregate.Count);
            row.Cell(4).SetValue(aggregate.Cost);

            Unlock(row.Cell(5));

            rowIndex++;
        }

        var endRow = startRow + rowsCount;

        // Footer / totals block
        for (var i = 0; i <= 4; i++)
            Unlock(sheet.Row(endRow + i).Cells(2, 5));

        Unlock(sheet.Row(endRow + 5).Cell(5));
        Unlock(sheet.Row(endRow + 6).Cells(3, 5));
        Unlock(sheet.Row(endRow + 7).Cells(3, 5));

        var nds = report.Items
            .Where(ri => ri.SapNds != "UN")
            .Select(ri => ri.SapNds)
            .FirstOrDefault();

        // В том числе НДС
        if (decimal.TryParse(nds, out var parsedNds))
            sheet.Row(endRow + 8).Cell(4).SetValue(parsedNds);
        else
            sheet.Row(endRow + 8).Cell(4).SetValue(string.Empty);

        // Исполнитель
        sheet.Row(endRow + 13).Cell(2).SetValue(report.ServiceCompanyName);

        Unlock(sheet.Row(endRow + 16).Cell(2));
        Unlock(sheet.Row(endRow + 16).Cell(4));

        sheet.Row(endRow + 19).Cell(2).Style.DateFormat.Format = "dd.MM.yyyy";
        sheet.Row(endRow + 19).Cell(2).SetValue(DateTime.UtcNow);
        Unlock(sheet.Row(endRow + 19).Cell(2));
    }

    public byte[] PaymentOrderToXls(List<PaymentOrderItem> paymentOrderItems)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add();

        var headers = new[]
        {
            "Поставщик",
            "Завод",
            "Тип контировки",
            "Номер материала",
            "Кол-во",
            "БЕИ",
            "Цена с НДС",
            "Дата поставки",
            "Код контировки",
            "Контракт", "Название материала",
            "Сумма НДС",
            "Затребовал",
            "ПФМ",
            "Сумма с НДС",
            "Код НДС",
            "Тип банка-партнёра",
            "Альтернативный получатель платежа",
            "Затраты на получателя платежа",
            "Счет на оплату",
            "Название карточки ЗнЗ",
            "Номер клиента-партнера"
        };

        var columnCount = headers.Length;
        for (var i = 0; i < columnCount; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = headers[i];
        }

        var rowIndex = 2;

        foreach (var rep in paymentOrderItems)
        {
            // "Поставщик"
            sheet.Cell(rowIndex, 1).Value = rep.ServiceCompanySapId;
            // "Завод"
            sheet.Cell(rowIndex, 2).Value = rep.ShopName;
            // "Тип контировки"
            sheet.Cell(rowIndex, 3).Value = PaymentOrderItem.MvzType;
            // "Номер материала",
            sheet.Cell(rowIndex, 4).Value = rep.MaterialNumber;
            // "Кол-во",
            sheet.Cell(rowIndex, 5).Value = PaymentOrderItem.Amount;
            // "БЕИ",
            sheet.Cell(rowIndex, 6).Value = PaymentOrderItem.Bei;
            // "Цена с НДС",
            sheet.Cell(rowIndex, 7).Value = rep.TotalCost;
            // "Дата поставки",
            sheet.Cell(rowIndex, 8).Value = rep.InvoiceDate.ToString("dd.MM.yyyy");
            // "Код контировки",
            sheet.Cell(rowIndex, 9).Value = rep.Mvz;
            // "Контракт",
            sheet.Cell(rowIndex, 10).Value = rep.SapContractNumber;
            // "Счет на оплату",
            sheet.Cell(rowIndex, 20).Value = rep.ReportNumber;

            rowIndex++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return stream.ToArray();
    }

    private static List<WareCodeAggregate> GroupByWareCode(IReadOnlyCollection<ReportItem> reportItems)
    {
        var allServices = reportItems
            .SelectMany(item =>
            {
                var services = new List<(string WareCode, string Name, decimal Cost)>();

                if (item.Cost != 0)
                    services.Add((item.WareCode, item.ServiceName, item.Cost));

                if (item.AdditionalServices is { Count: > 0 })
                    services.AddRange(item.AdditionalServices
                        .Where(s => s.Cost != 0)
                        .Select(x => (x.WareCode, x.Name, x.Cost)));

                return services;
            });

        return allServices
            .GroupBy(x => x.WareCode)
            .Select(g => new WareCodeAggregate
            {
                WareCode = g.Key,
                ServiceName = g.First().Name,
                Count = g.Count(),
                Cost = g.Sum(x => x.Cost)
            })
            .ToList();
    }

    private static void ApplyThinBorder(IXLRange range)
    {
        range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
    }

    private static void Unlock(IXLCell cell)
    {
        cell.Style.Protection.SetLocked(false);
    }

    private static void Unlock(IXLCells cells)
    {
        cells.Style.Protection.SetLocked(false);
    }
}
