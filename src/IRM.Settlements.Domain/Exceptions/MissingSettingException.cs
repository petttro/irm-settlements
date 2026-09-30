namespace IRM.Settlements.Domain.Exceptions;

/// <summary>
/// Исключение, возникающее при отсутствии обязательного параметра конфигурации приложения.
/// </summary>
public class MissingSettingException : DomainException
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MissingSettingException"/> с указанным сообщением об ошибке.
    /// </summary>
    /// <param name="message">Сообщение, описывающее ошибку.</param>
    public MissingSettingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MissingSettingException"/> с сообщением об ошибке,
    /// указывающим на отсутствие обязательного параметра конфигурации.
    /// </summary>
    /// <param name="settingsType">Тип конфигурации, в котором отсутствует параметр.</param>
    /// <param name="settingName">Имя параметра конфигурации, который не был установлен.</param>
    public MissingSettingException(Type settingsType, string settingName)
        : base($"Параметр конфигурации '{settingsType}.{settingName}' не установлен.")
    {
    }

    /// <summary>
    /// Проверка значения на null с выбросом <see cref="MissingSettingException"/>.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <param name="settingsType">Тип настроек.</param>
    /// <param name="settingName">Имя настройки.</param>
    /// <exception cref="MissingSettingException">Выбрасывается, если значение <paramref name="value"/> равно null.</exception>
    public static void ThrowIfNull(object? value, Type settingsType, string settingName)
    {
        if (value is null)
        {
            throw new MissingSettingException(settingsType, string.IsNullOrWhiteSpace(settingName) ? "UnknownSetting" : settingName);
        }
    }
}
