using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public interface IOnRollAction
	{
		UniTask OnRoll(DiceGameModel diceGameModel);
	}
}