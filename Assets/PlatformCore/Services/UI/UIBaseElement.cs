using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlatformCore.Services.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class UIBaseElement : MonoBehaviour
	{
		[SerializeField] protected CanvasGroup _group;

		[Header("Canvas Performance Settings")] [SerializeField]
		private UICanvasType _canvasType = UICanvasType.Static;
		public UICanvasType CanvasType => _canvasType;

		private void Awake()
		{
			if (!_group)
			{
				Debug.LogError($"[{GetType().Name}] CanvasGroup is missing on GameObject '{gameObject.name}'!");
			}

			OnAwake();
		}

		public void Show()
		{
			if (_group)
			{
				_group.alpha = 1;
				_group.interactable = true;
				_group.blocksRaycasts = true;
			}

			OnShow();
		}

		public void Hide()
		{
			if (_group)
			{
				_group.alpha = 0;
				_group.interactable = false;
				_group.blocksRaycasts = false;
			}

			OnHide();
		}

		public bool isShown()
		{
			return _group.alpha > 0;
		}

		protected virtual void OnAwake()
		{
		}

		protected virtual void OnShow()
		{
		}

		protected virtual void OnHide()
		{
		}
	}
}