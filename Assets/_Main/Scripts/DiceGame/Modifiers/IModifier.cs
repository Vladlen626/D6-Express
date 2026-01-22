using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public interface IOnRoundStartModifier : IModifier
	{
	}

	public interface IOnRollModifier : IModifier
	{
	}

	public interface IOnPassModifier : IModifier
	{
	}

	public interface IOnRoundEndModifier : IModifier
	{
	}

	public interface IModifier
	{
		UniTask ModifyValues(DiceModifierContext modifierContext);
	}
}
