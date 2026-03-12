using System.Collections.Generic;
using _Main.Scripts.Dice;
using UnityEngine;

public class ShopItemView : MonoBehaviour
{
    [SerializeField]
    private List<DiceVisualEntry> diceVisuals;

    private Dictionary<string, Transform> _diceVisualMap;
    private GameObject runtimeInstance;
    private string runtimeVisualId;

    public void Initialize(string diceConfigId)
    {
        BuildMap();
        SetupVisual(diceConfigId);
    }

    private void SetupVisual(string diceViewId)
    {
        if (_diceVisualMap != null)
        {
            foreach (var visual in _diceVisualMap.Values)
            {
                if (visual)
                {
                    visual.gameObject.SetActive(false);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(diceViewId) &&
            _diceVisualMap != null &&
            _diceVisualMap.TryGetValue(diceViewId, out var target))
        {
            target.gameObject.SetActive(true);
            ClearRuntimeInstance();
            return;
        }

        SetupRuntimeVisual(diceViewId);
    }

    private void BuildMap()
    {
        _diceVisualMap = new Dictionary<string, Transform>();
        if (diceVisuals == null)
        {
            return;
        }

        foreach (var entry in diceVisuals)
        {
            if (entry.visual && !_diceVisualMap.ContainsKey(entry.id))
            {
                _diceVisualMap.Add(entry.id, entry.visual);
            }
        }
    }

    private void SetupRuntimeVisual(string visualId)
    {
        if (string.IsNullOrWhiteSpace(visualId))
        {
            ClearRuntimeInstance();
            return;
        }

        if (runtimeInstance && runtimeVisualId == visualId)
        {
            runtimeInstance.SetActive(true);
            return;
        }

        ClearRuntimeInstance();

        var prefab = Resources.Load<GameObject>($"Items/{visualId}");
        if (!prefab)
        {
            return;
        }

        runtimeInstance = Instantiate(prefab, transform);
        runtimeInstance.transform.localPosition = Vector3.zero;
        runtimeInstance.transform.localRotation = Quaternion.identity;
        runtimeInstance.transform.localScale = Vector3.one;
        runtimeVisualId = visualId;

        DisableRuntimeBehaviours(runtimeInstance);
    }

    private void DisableRuntimeBehaviours(GameObject instance)
    {
        var itemViews = instance.GetComponentsInChildren<ItemView>(true);
        for (int i = 0; i < itemViews.Length; i++)
        {
            itemViews[i].enabled = false;
        }

        var colliders = instance.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
    }

    private void ClearRuntimeInstance()
    {
        if (runtimeInstance)
        {
            Destroy(runtimeInstance);
        }

        runtimeInstance = null;
        runtimeVisualId = null;
    }
}
