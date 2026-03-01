using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlatformCore.Services.UI
{
	/// <summary>
	/// Resizes a UI element based on TMP text size.
	/// Intended for simple roots that contain a background and a text, both stretched to full size.
	/// </summary>
	public sealed class UIBackgroundSizer : MonoBehaviour
	{
		/// <summary>
		/// Background RectTransform that will be stretched to the root size.
		/// If null, tries to find "ElementBackground" child or first child.
		/// </summary>
		[SerializeField] private RectTransform _background;
		/// <summary>
		/// TMP text component used for size calculations.
		/// If null, resolves from children.
		/// </summary>
		[SerializeField] private TMP_Text _text;
		/// <summary>
		/// Optional LayoutElement used by layout groups to read preferred sizes.
		/// </summary>
		[SerializeField] private LayoutElement _layoutElement;
		/// <summary>
		/// Maximum width of the text. 0 disables max width.
		/// </summary>
		[SerializeField] private float _maxWidth;
		/// <summary>
		/// Enables TMP word wrapping when true.
		/// </summary>
		[SerializeField] private bool _wrap = true;
		/// <summary>
		/// When true, preferred sizes are written to LayoutElement (if present or auto-added).
		/// </summary>
		[SerializeField] private bool _useLayoutElement = true;
		/// <summary>
		/// When true, a LayoutElement will be added if missing.
		/// </summary>
		[SerializeField] private bool _autoAddLayoutElement = true;
		/// <summary>
		/// When true, applies the calculated size directly to the root RectTransform.
		/// </summary>
		[SerializeField] private bool _applyToRootRect = true;
		private bool _isRefreshing;

		private void OnEnable()
		{
			if (Application.isPlaying)
			{
				Refresh();
			}
		}

		private void OnValidate()
		{
			AutoResolve();
			if (Application.isPlaying)
			{
				Refresh();
			}
		}

		private void OnRectTransformDimensionsChange()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			Refresh();
		}

		[ContextMenu("Apply")]
		public void Refresh()
		{
			RefreshInternal(false);
		}

		/// <summary>
		/// Refreshes sizes in editor mode (for quick tuning in the Inspector).
		/// </summary>
		public void RefreshInEditor()
		{
			RefreshInternal(true);
		}

		private void RefreshInternal(bool allowEditor)
		{
			if (_isRefreshing)
			{
				return;
			}

			AutoResolve();
			if (!_text)
			{
				return;
			}

			if (!Application.isPlaying && !allowEditor)
			{
				return;
			}

			_isRefreshing = true;

			EnsureStretch();

			var textValue = _text.text ?? string.Empty;
			_text.enableWordWrapping = false;
			_text.ForceMeshUpdate();

			var noWrap = _text.GetPreferredValues(textValue);
			float width = noWrap.x;
			float height = noWrap.y;

			if (_wrap)
			{
				_text.enableWordWrapping = true;
				if (_maxWidth > 0f && noWrap.x > _maxWidth)
				{
					var wrapped = _text.GetPreferredValues(textValue, _maxWidth, 0f);
					width = _maxWidth;
					height = wrapped.y;
				}
			}
			else if (_maxWidth > 0f)
			{
				width = Mathf.Min(width, _maxWidth);
			}

			_text.enableWordWrapping = _wrap;

			var size = new Vector2(width, height);
			if (_useLayoutElement)
			{
				var layoutElement = GetOrAddLayoutElement();
				if (layoutElement)
				{
					layoutElement.preferredWidth = size.x;
					layoutElement.preferredHeight = size.y;
				}
			}

			if (_applyToRootRect)
			{
				var rootRect = transform as RectTransform;
				if (rootRect)
				{
					rootRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
					rootRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
				}
			}

			_isRefreshing = false;
		}

		private LayoutElement GetOrAddLayoutElement()
		{
			if (!_layoutElement)
			{
				_layoutElement = GetComponent<LayoutElement>();
			}

			if (!_layoutElement && _autoAddLayoutElement)
			{
				_layoutElement = gameObject.AddComponent<LayoutElement>();
			}

			return _layoutElement;
		}

		private void AutoResolve()
		{
			if (!_text)
			{
				_text = GetComponentInChildren<TMP_Text>(true);
			}

			if (!_background)
			{
				var bg = transform.Find("ElementBackground");
				if (!bg && transform.childCount > 0)
				{
					bg = transform.GetChild(0);
				}

				_background = bg as RectTransform;
			}

			if (!_layoutElement)
			{
				_layoutElement = GetComponent<LayoutElement>();
			}
		}

		private void EnsureStretch()
		{
			if (_background)
			{
				SetStretchPreserveOffsets(_background);
			}

			if (_text)
			{
				SetStretchPreserveOffsets(_text.rectTransform);
			}
		}

		private static void SetStretchPreserveOffsets(RectTransform rect)
		{
			if (!rect)
			{
				return;
			}

			var offsetMin = rect.offsetMin;
			var offsetMax = rect.offsetMax;
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = offsetMin;
			rect.offsetMax = offsetMax;
		}
	}
}
