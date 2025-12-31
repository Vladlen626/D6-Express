using System;
using PlatformCore.Services.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _Main.Scripts.UI
{
	public class UIMainMenu : UIBaseElement
	{
		public event Action OnStartClicked;
		public event Action OnSettingsClicked;

		public void StartBtn()
		{
			OnStartClicked?.Invoke();
		}

		public void SettingsBtn()
		{
			OnSettingsClicked?.Invoke();
		}
	}
}