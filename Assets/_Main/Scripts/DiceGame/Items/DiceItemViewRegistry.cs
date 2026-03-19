using System;
using System.Collections.Generic;

namespace _Main.Scripts.Dice
{
	public sealed class DiceItemViewRegistry
	{
		private readonly Dictionary<IModifierItem, ItemView> itemViews = new();

		public void Register(IModifierItem item, ItemView view)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			if (!view)
			{
				throw new ArgumentNullException(nameof(view));
			}

			itemViews[item] = view;
		}

		public void Unregister(IModifierItem item, ItemView view = null)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			if (!itemViews.TryGetValue(item, out var registeredView))
			{
				return;
			}

			if (view != null && !ReferenceEquals(registeredView, view))
			{
				return;
			}

			itemViews.Remove(item);
		}

		public bool TryGetItemView(IModifierItem item, out ItemView view)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			if (!itemViews.TryGetValue(item, out view))
			{
				return false;
			}

			if (!view)
			{
				itemViews.Remove(item);
				view = null;
				return false;
			}

			return true;
		}

		public void Clear()
		{
			itemViews.Clear();
		}
	}
}
