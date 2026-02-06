using System;
using _Main.Scripts.Dice;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public class CombinationsController : IBaseController, IActivatable
{
    private readonly ModifiersModel modifiers;
    private readonly CombinationsView combinationsView;

    private int[] multipliers = new int[Enum.GetValues(typeof(DiceCombination)).Length];

    public CombinationsController(ModifiersModel modifiers, CombinationsView combinationsView)
    {
        this.modifiers = modifiers;
        this.combinationsView = combinationsView;
    }

    public void Activate()
    {
        modifiers.ModifierAdded += OnModifierAdded;
        modifiers.ModifierRemoved += OnModifierRemoved;

        multipliers = new int[29];
        Array.Fill(multipliers, 1);

        foreach (var item in modifiers.AllModifiers)
        {
            OnModifierAdded(item);
        }
    }

    public void Deactivate()
    {
        modifiers.ModifierRemoved -= OnModifierRemoved;
        modifiers.ModifierAdded -= OnModifierAdded;
    }

    private void OnModifierAdded(IModifier modifier)
    {
        if (modifier is MultiplyComboModifier multiplyComboModifier)
        {
            var comboIndex = (int)multiplyComboModifier.combination;
            if (comboIndex > 3 && comboIndex < 8)
            {
                for (int i = 0; i < 6; i++)
                {
                    multipliers[comboIndex + i]++;
                }
            }
        }
        else if (modifier is MultiplyKindOfModifiers multiplyKindOfModifiers)
        {
            var comboIndex = (int)multiplyKindOfModifiers.combination;
            var faceIndex = comboIndex + multiplyKindOfModifiers.face;
            multipliers[faceIndex]++;
        }

        UpdateText();
    }

    private void OnModifierRemoved(IModifier modifier)
    {
        if (modifier is MultiplyComboModifier multiplyComboModifier)
        {
            var comboIndex = (int)multiplyComboModifier.combination;
            if (comboIndex > 3 && comboIndex < 8)
            {
                for (int i = 0; i < 6; i++)
                {
                    multipliers[comboIndex + i]--;
                }
            }
        }
        else if (modifier is MultiplyKindOfModifiers multiplyKindOfModifiers)
        {
            var comboIndex = (int)multiplyKindOfModifiers.combination;
            var faceIndex = comboIndex + multiplyKindOfModifiers.face;
            multipliers[faceIndex]--;
        }

        UpdateText();
    }

    private void UpdateText()
    {
        combinationsView.single1.text = (DiceGameUtils.BaseScoreSetup(DiceCombination.SingleOnes, 1) * multipliers[(int)DiceCombination.SingleOnes]).ToString();
        combinationsView.single5.text = (DiceGameUtils.BaseScoreSetup(DiceCombination.SingleFives, 5) * multipliers[(int)DiceCombination.SingleFives]).ToString();
        combinationsView.straight1to5.text = (DiceGameUtils.BaseScoreSetup(DiceCombination.Straight_1_5, 0) * multipliers[(int)DiceCombination.Straight_1_5]).ToString();
        combinationsView.straight1to6.text = (DiceGameUtils.BaseScoreSetup(DiceCombination.Straight_1_6, 0) * multipliers[(int)DiceCombination.Straight_1_6]).ToString();
        combinationsView.straight2to6.text = (DiceGameUtils.BaseScoreSetup(DiceCombination.Straight_2_6, 0) * multipliers[(int)DiceCombination.Straight_2_6]).ToString();
        combinationsView.threeOfAKind1.text = (DiceGameUtils.BaseScoreSetup(DiceCombination.ThreeOfAKind, 1) * multipliers[(int)DiceCombination.ThreeOfAKind]).ToString();
        combinationsView.threeOfAKind2.text = (DiceGameUtils.BaseScoreSetup(DiceCombination.ThreeOfAKind, 2) * multipliers[(int)DiceCombination.ThreeOfAKind + 1]).ToString();
        combinationsView.threeOfAKind3.text = (DiceGameUtils.BaseScoreSetup(DiceCombination.ThreeOfAKind, 3) * multipliers[(int)DiceCombination.ThreeOfAKind + 2]).ToString();
        combinationsView.threeOfAKind4.text = (DiceGameUtils.BaseScoreSetup(DiceCombination.ThreeOfAKind, 4) * multipliers[(int)DiceCombination.ThreeOfAKind + 3]).ToString();
        combinationsView.threeOfAKind5.text = (DiceGameUtils.BaseScoreSetup(DiceCombination.ThreeOfAKind, 5) * multipliers[(int)DiceCombination.ThreeOfAKind + 4]).ToString();
        combinationsView.threeOfAKind6.text = (DiceGameUtils.BaseScoreSetup(DiceCombination.ThreeOfAKind, 6) * multipliers[(int)DiceCombination.ThreeOfAKind + 5]).ToString();
    }
}
