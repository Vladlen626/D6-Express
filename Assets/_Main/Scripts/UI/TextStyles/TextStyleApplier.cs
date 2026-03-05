using TMPro;
using UnityEngine;

namespace _Main.Scripts.UI
{
	[ExecuteAlways]
	public class TextStyleApplier : MonoBehaviour
	{
		[SerializeField]
		private string styleId;

		private TextMeshProUGUI target;

		public string StyleId
		{
			get => styleId;
			set => styleId = value;
		}

		private void OnEnable()
		{
			Apply();
		}

		private void OnValidate()
		{
			Apply();
		}

		public void Apply()
		{
			if (!target)
			{
				target = GetComponent<TextMeshProUGUI>();
			}

			if (!target)
			{
				return;
			}

			var library = TextStyleLibraryProvider.GetDefault();
			if (library == null)
			{
				return;
			}

			var style = library.GetStyle(styleId);
			if (style == null)
			{
				return;
			}

			target.color = style.Color;

			if (style.UseAdvanced)
			{
				if (style.FontSize > 0f)
				{
					target.fontSize = style.FontSize;
				}

				target.fontStyle = style.FontStyle;
			}
		}
	}
}
