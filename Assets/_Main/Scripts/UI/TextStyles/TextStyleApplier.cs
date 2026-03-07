using TMPro;
using UnityEngine;

namespace _Main.Scripts.UI
{
	[ExecuteAlways]
	public class TextStyleApplier : MonoBehaviour, ISerializationCallbackReceiver
	{
		[SerializeField]
		private TextStyleReference style;

		// Backward compatibility for old serialized field.
		[SerializeField, HideInInspector]
		private string styleId;

		private TextMeshProUGUI target;

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

			var styleEntry = library.GetStyle(style.Id);
			if (styleEntry == null)
			{
				return;
			}

			target.color = styleEntry.Color;

			if (styleEntry.UseAdvanced)
			{
				if (styleEntry.FontSize > 0f)
				{
					target.fontSize = styleEntry.FontSize;
				}

				target.fontStyle = styleEntry.FontStyle;
			}
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			if (string.IsNullOrWhiteSpace(style.Id) && !string.IsNullOrWhiteSpace(styleId))
			{
				style = new TextStyleReference(styleId);
				styleId = string.Empty;
			}
		}
	}
}
