using System;
using UnityEngine;

namespace _Main.Scripts.UI
{
	[Serializable]
	public struct ColorStyleReference
	{
		[SerializeField]
		private string id;

		public string Id
		{
			get => id;
			set => id = value;
		}

		public ColorStyleReference(string id)
		{
			this.id = id ?? string.Empty;
		}
	}
}
