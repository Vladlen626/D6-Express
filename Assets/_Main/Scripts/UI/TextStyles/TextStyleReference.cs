using System;
using UnityEngine;

namespace _Main.Scripts.UI
{
	[Serializable]
	public struct TextStyleReference
	{
		[SerializeField]
		private string id;

		public string Id
		{
			get => id;
			set => id = value;
		}

		public TextStyleReference(string id)
		{
			this.id = id ?? string.Empty;
		}
	}
}
