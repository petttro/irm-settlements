namespace IRM.Settlements.Api.Extensions;

/// <summary>
/// Методы расширения для работы с переменными сред.
/// </summary>
public static class EnvironmentExtensions
{
    /// <summary>
    /// Окружение CI\CD.
    /// </summary>
    private const string PipeEnvironmentName = "Pipe";

    /// <summary>
    /// Окружение IntegrationTests.
    /// </summary>
    private const string IntegrationTestsEnvironmentName = "IntegrationTests";

    /// <summary>
    /// Определяет в какой среде запускается приложение: Если в CI\CD - true.
    /// </summary>
    /// <param name="hostEnvironment">Окружение хоста.</param>
    /// <returns>Если в CI\CD - true, иначе false.</returns>
    public static bool IsPipeEnvironment(this IHostEnvironment hostEnvironment)
    {
        return hostEnvironment.EnvironmentName == PipeEnvironmentName;
    }

    /// <summary>
    /// Определяет в какой среде запускается приложение: Если Интеграционные тесты - true.
    /// </summary>
    /// <param name="hostEnvironment">Окружение хоста.</param>
    /// <returns>Если в IntegrationTests - true, иначе false.</returns>
    public static bool IsIntegrationTests(this IHostEnvironment hostEnvironment)
    {
        return hostEnvironment.EnvironmentName == IntegrationTestsEnvironmentName;
    }
}
