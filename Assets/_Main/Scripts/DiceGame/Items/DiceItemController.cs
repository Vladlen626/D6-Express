using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	/// <summary>
	/// Connects a DiceItemView with a modifier item and feeds state changes both ways.
	/// Register this controller in the lifecycle to hook/unhook automatically.
	/// </summary>
	public class ModifierItemController : IBaseController, IActivatable
	{
		private readonly IModifierItem item;
		private readonly DiceItemView view;

		public ModifierItemController(IModifierItem item, DiceItemView view)
		{
			this.item = item;
			this.view = view;
		}

		public void Activate()
		{
			item.AttachView(view);
			view.OnClicked.AddListener(OnViewClicked);
		}

		public void Deactivate()
		{
			view.OnClicked.RemoveListener(OnViewClicked);
			item.DetachView();
		}

		private void OnViewClicked()
		{
			item.TryHandleClick();
		}
	}
}
