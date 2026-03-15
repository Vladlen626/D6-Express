using Cysharp.Threading.Tasks;

namespace _Main.Scripts.Dice
{
	public interface IOnRoundStartModifier : IModifier
	{
	}

	public interface IOnLevelStartModifier : IModifier
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

	public interface IModifierUiConfigProvider
	{
		string UiConfigId { get; }
	}

	public interface IModifierApplyResultProvider
	{
		bool LastApplyHadEffect { get; }
	}
}
