using TMPro;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] 
    private Transform[] spawnPositions;
    
    [SerializeField] 
    private TradeItem[] tradeItemsPrefabs;

    [SerializeField] 
    private TextMeshPro[] prices;

    private TradeItem[] tradeItems;

    private void Awake()
    {
        tradeItems = new TradeItem[spawnPositions.Length];
    }

    private void OnEnable()
    {
        UpdateShop();
    }

    public void UpdateShop()
    {
        for (int i = 0; i < tradeItems.Length; i++)
        {
            var item = tradeItems[i];
            if (item == null) 
                continue;

            item.Buyed -= OnBuyed;
            Destroy(item.gameObject);
            tradeItems[i] = null;
        }

        for (int i = 0; i < spawnPositions.Length; i++)
        {
            var spawnPosition = spawnPositions[i];

            var prefab = tradeItemsPrefabs[Random.Range(0, tradeItemsPrefabs.Length)];
            var item = Instantiate(prefab, spawnPosition.position, Quaternion.identity, transform);

            prices[i].text = item.Price.ToString();
            item.Buyed += OnBuyed;

            tradeItems[i] = item;
        }
    }

    public void OnBuyed(TradeItem item, GameObject buyer)
    {
        int index = System.Array.IndexOf(tradeItems, item);
        if (index < 0)
            return;

        item.Buyed -= OnBuyed;
        tradeItems[index] = null;
        prices[index].text = "x";
        Destroy(item.gameObject);
    }
}
