using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlatformCore.Services.UI
{
	public abstract class BaseUIElement : MonoBehaviour
	{
		[Header("Canvas Performance Settings")] [SerializeField]
		private UICanvasType _canvasType = UICanvasType.Static;
		public UICanvasType CanvasType => _canvasType;

		public void Show()
		{
			OnShow();
		}

		public void Hide()
		{
			OnHide();
		}

		protected virtual void OnShow()
		{
		}

		protected virtual void OnHide()
		{
		}
	}
}