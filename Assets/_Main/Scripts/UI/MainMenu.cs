using System;
using PlatformCore.Services.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _Main.Scripts.UI
{
	public class MainMenu : UIBaseElement
	{
		[SerializeField] private CanvasGroup _group;
		[SerializeField] private Button _startBtn;
		[SerializeField] private Button _settingsBtn;

		public event Action OnStartClicked;
		public event Action OnSettingsClicked;

		private void Awake()
		{
			if (_startBtn)
			{
				_startBtn.onClick.AddListener(() => OnStartClicked?.Invoke());
			}

			if (_settingsBtn)
			{
				_settingsBtn.onClick.AddListener(() => OnSettingsClicked?.Invoke());
			}
		}

		protected override void OnShow()
		{
			if (!_group) return;
			_group.alpha = 1f;
			_group.interactable = true;
			_group.blocksRaycasts = true;
		}

		protected override void OnHide()
		{
			if (!_group) return;
			_group.alpha = 0f;
			_group.interactable = false;
			_group.blocksRaycasts = false;
		}
	}
}