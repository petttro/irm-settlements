using System.Text;

namespace IRM.Settlements.IntegrationTests.Utils.MvzCsvToSql;

public static class MvzCsvToSqlConverter
{
    public static void Convert()
    {
        var inputFile = "Utils/MvzCsvToSql/input.csv";
        var outputFile = "Utils/MvzCsvToSql/insert.sql";

        var lines = File.ReadAllLines(inputFile, Encoding.UTF8);

        var values = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line =>
            {
                var parts = line.Split(';');

                if (parts.Length != 3)
                    return null;

                var id = parts[0].Trim();
                var mvzName = parts[1].Trim();
                var mvzCode = parts[2].Trim();
                var updatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");

                return $"('{Escape(id)}', '{Escape(mvzName)}', '{Escape(mvzCode)}', '{updatedAt}')";
            })
            .Where(v => v != null)
            .ToList();

        if (!values.Any())
        {
            Console.WriteLine("Нет данных");
            return;
        }

        var sql = new StringBuilder();
        sql.AppendLine("INSERT INTO settlements.\"Mvz\" (\"Id\", \"MvzName\", \"MvzCode\", \"UpdatedAt\") VALUES");
        sql.AppendLine(string.Join(",\n", values) + ";");

        File.WriteAllText(outputFile, sql.ToString(), Encoding.UTF8);

        Console.WriteLine($"Готово: {outputFile}");
    }

    private static string Escape(string value)
    {
        return value.Replace("'", "''");
    }
}
