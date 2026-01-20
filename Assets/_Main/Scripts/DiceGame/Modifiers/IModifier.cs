using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public interface IOnRollModifier : IModifier
	{
	}
	public interface IOnPassModifier : IModifier
	{
	}

	public interface IModifier
	{
		UniTask ModifyValues(DiceCombinationResult diceCombinationResult);
	}
}