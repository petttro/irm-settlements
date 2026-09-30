namespace IRM.Settlements.Application.UseCases.Reports.Results;

public class FileResult
{
    public required string FileName { get; set; }

    public required byte[] Data { get; set; }

    public required string ContentType { get; set; }
}
