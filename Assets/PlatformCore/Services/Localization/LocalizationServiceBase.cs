using System.Threading;
using Cysharp.Threading.Tasks;
using PlatformCore.Services.Factory;
using PlatformCore.Services;

public class LocalizationServiceBase : BaseAsyncService, ILocalizationService
{
	private readonly ConfigService configService;

	private TextsConfig textsConfig;

	public LocalizationServiceBase(ConfigService configService)
	{
		this.configService = configService;
	}

	protected override async UniTask OnPreInitializeAsync(CancellationToken ct)
	{
		textsConfig = await configService.GetFirstOrDefaultAsync<TextsConfig>(ResourcePaths.Json.texts_eng);
	}

	public string GetLocalized(string id)
	{
		return textsConfig.texts[id];
	}
}