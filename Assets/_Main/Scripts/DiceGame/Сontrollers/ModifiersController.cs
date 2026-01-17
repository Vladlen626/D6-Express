using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

namespace _Main.Scripts.Dice
{
	public class ModifiersController : IBaseController, IActivatable
	{
		private readonly DiceGameModel diceGameModel;
		private readonly TableModel tableModel;

		public void Activate()
		{
			throw new System.NotImplementedException();
		}

		public void Deactivate()
		{
			throw new System.NotImplementedException();
		}
	}
}