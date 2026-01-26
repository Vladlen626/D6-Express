public interface ILocalizationService
{
	//TODO: Предлагаю сделать статическим классом. Убрать интерфейс
	// Хочется в любой части кода писать LocalizationService.GetCurrentLocale ( получаешь EN-RU и т.п.)
	// Добавить Action OnLocaleChanged, чтоб подписываться на это и автоматически переводить все LocalizedText.
	// В одной московской компании это примерно так))

	// todo: 
	// SetLocalization()
	public string GetLocalized(string id);
}