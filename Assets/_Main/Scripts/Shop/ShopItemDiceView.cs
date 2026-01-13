using System.Collections.Generic;
using _Main.Scripts.Dice;
using UnityEngine;

public class ShopItemDiceView : MonoBehaviour
{
    [SerializeField]
    private List<DiceVisualEntry> diceVisuals;

    private Dictionary<string, Transform> _diceVisualMap;

    public void Initialize(string diceConfigId)
    {
        _diceVisualMap = new Dictionary<string, Transform>();
        foreach (var entry in diceVisuals)
        {
            if (!_diceVisualMap.ContainsKey(entry.id))
            {
                _diceVisualMap.Add(entry.id, entry.visual);
            }
        }

        SetupVisual(diceConfigId);
    }

    private void SetupVisual(string diceViewId)
    {
        foreach (var visual in _diceVisualMap.Values)
        {
            visual.gameObject.SetActive(false);
        }

        if (_diceVisualMap.TryGetValue(diceViewId, out var target))
        {
            target.gameObject.SetActive(true);
        }
    }
}