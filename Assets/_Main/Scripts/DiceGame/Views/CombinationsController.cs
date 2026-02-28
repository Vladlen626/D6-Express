using System.Collections.Generic;
using _Main.Scripts.Dice;
using PlatformCore.Core;
using PlatformCore.Infrastructure.Lifecycle;

public sealed class CombinationsController : IBaseController, IActivatable
{
    private readonly ModifiersModel modifiers;
    private readonly CombinationsView combinationsView;
    private readonly Dictionary<(DiceCombination combo, int face), int> multipliers = new();
    public CombinationsController(ModifiersModel modifiers, CombinationsView combinationsView)
    {
        this.modifiers = modifiers;
        this.combinationsView = combinationsView;
    }
    public void Activate()
    {
        modifiers.ModifierAdded += OnModifierAdded;
        modifiers.ModifierRemoved += OnModifierRemoved;

        ResetMultipliersToDefault();

        foreach (var item in modifiers.AllModifiers)
        {
            ApplyModifier(item, delta: +1);
        }

        UpdateText();
    }

    public void Deactivate()
    {
        modifiers.ModifierRemoved -= OnModifierRemoved;
        modifiers.ModifierAdded -= OnModifierAdded;
    }

    private void OnModifierAdded(IModifier modifier)
    {
        ApplyModifier(modifier, delta: +1);
        UpdateText();
    }

    private void OnModifierRemoved(IModifier modifier)
    {
        ApplyModifier(modifier, delta: -1);
        UpdateText();
    }

    private void ResetMultipliersToDefault()
    {
        multipliers.Clear();

        SetDefault(DiceCombination.SingleOnes);
        SetDefault(DiceCombination.SingleFives);
        SetDefault(DiceCombination.Straight_1_5);
        SetDefault(DiceCombination.Straight_1_6);
        SetDefault(DiceCombination.Straight_2_6);

        for (int face = 1; face <= 6; face++)
            SetDefault(DiceCombination.ThreeOfAKind, face);
    }

    private void SetDefault(DiceCombination combo, int face = 0)
    {
        multipliers[(combo, face)] = 1;
    }

    private void ApplyModifier(IModifier modifier, int delta)
    {
        switch (modifier)
        {
            case MultiplyKindOfModifiers m:
                ApplyToKey(m.combination, m.face, delta);
                break;

            case MultiplyComboModifier m:
                ApplyToCombo(m.combination, delta);
                break;
        }
    }

    private void ApplyToCombo(DiceCombination combo, int delta)
    {
        if (combo == DiceCombination.ThreeOfAKind)
        {
            for (int face = 1; face <= 6; face++)
                ApplyToKey(combo, face, delta);

            return;
        }

        ApplyToKey(combo, face: 0, delta);
    }

    private void ApplyToKey(DiceCombination combo, int face, int delta)
    {
        var key = (combo, face);

        if (!multipliers.TryGetValue(key, out var current))
            current = 1;

        var next = current + delta;

        if (next < 1)
            next = 1;

        multipliers[key] = next;
    }

    private int Mult(DiceCombination combo, int face = 0)
    {
        if (multipliers.TryGetValue((combo, face), out var m))
        {
            return m;
        }
        return 1;
    }

    private void UpdateText()
    {
        combinationsView.single1.text =
            (DiceGameUtils.BaseScoreSetup(DiceCombination.SingleOnes, 1) * Mult(DiceCombination.SingleOnes)).ToString();

        combinationsView.single5.text =
            (DiceGameUtils.BaseScoreSetup(DiceCombination.SingleFives, 5) * Mult(DiceCombination.SingleFives)).ToString();

        combinationsView.straight1to5.text =
            (DiceGameUtils.BaseScoreSetup(DiceCombination.Straight_1_5, 0) * Mult(DiceCombination.Straight_1_5)).ToString();

        combinationsView.straight1to6.text =
            (DiceGameUtils.BaseScoreSetup(DiceCombination.Straight_1_6, 0) * Mult(DiceCombination.Straight_1_6)).ToString();

        combinationsView.straight2to6.text =
            (DiceGameUtils.BaseScoreSetup(DiceCombination.Straight_2_6, 0) * Mult(DiceCombination.Straight_2_6)).ToString();

        combinationsView.threeOfAKind1.text =
            (DiceGameUtils.BaseScoreSetup(DiceCombination.ThreeOfAKind, 1) * Mult(DiceCombination.ThreeOfAKind, 1)).ToString();

        combinationsView.threeOfAKind2.text =
            (DiceGameUtils.BaseScoreSetup(DiceCombination.ThreeOfAKind, 2) * Mult(DiceCombination.ThreeOfAKind, 2)).ToString();

        combinationsView.threeOfAKind3.text =
            (DiceGameUtils.BaseScoreSetup(DiceCombination.ThreeOfAKind, 3) * Mult(DiceCombination.ThreeOfAKind, 3)).ToString();

        combinationsView.threeOfAKind4.text =
            (DiceGameUtils.BaseScoreSetup(DiceCombination.ThreeOfAKind, 4) * Mult(DiceCombination.ThreeOfAKind, 4)).ToString();

        combinationsView.threeOfAKind5.text =
            (DiceGameUtils.BaseScoreSetup(DiceCombination.ThreeOfAKind, 5) * Mult(DiceCombination.ThreeOfAKind, 5)).ToString();

        combinationsView.threeOfAKind6.text =
            (DiceGameUtils.BaseScoreSetup(DiceCombination.ThreeOfAKind, 6) * Mult(DiceCombination.ThreeOfAKind, 6)).ToString();
    }
}
