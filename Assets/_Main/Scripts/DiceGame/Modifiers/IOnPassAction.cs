using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public interface IOnPassAction
	{
		UniTask OnPass(DiceGameModel diceGameModel);
	}
}