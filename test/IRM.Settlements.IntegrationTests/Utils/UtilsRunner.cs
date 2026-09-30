using IRM.Settlements.IntegrationTests.Utils.MvzCsvToSql;
using Xunit;

namespace IRM.Settlements.IntegrationTests.Utils;

public class UtilsRunner
{
    [Fact(Skip = "Стереть, чтобы запустить тулзу")]
    public void RunMvzToSql()
    {
        MvzCsvToSqlConverter.Convert();
    }
}
