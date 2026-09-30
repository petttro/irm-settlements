namespace IRM.Settlements.Application.Extensions;

public static class StringExtensions
{
    public static string ToValidFileName(this string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return fileName;

        // Заменяем пробелы на подчеркивание
        fileName = fileName.Replace(' ', '_');

        // Удаляем все остальные невалидные символы
        var invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());
        return new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
    }
}
