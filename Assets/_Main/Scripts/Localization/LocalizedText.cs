using PlatformCore.Core;
using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
	[SerializeField] private TMP_Text text;
	private ILocalizationService localizationService;

	public TMP_Text Tmp => text;

	public void SetText(string id)
	{
		text.text = GetLocalizationService().GetLocalized(id);
	}

	public void SetRawText(string text)
	{
		this.text.text = text;
	}

	public void SetText(string id, params string[] agrs)
	{
		var localized = GetLocalizationService().GetLocalized(id);
		text.text = string.Format(localized, agrs);
	}

	public void SetText(string id, int arg)
	{
		var localized = GetLocalizationService().GetLocalized(id);
		text.SetText(localized, arg);
	}

	private ILocalizationService GetLocalizationService()
	{
		if (localizationService == null)
		{
			localizationService = Locator.Resolve<ILocalizationService>();
		}

		return localizationService;
	}
}
