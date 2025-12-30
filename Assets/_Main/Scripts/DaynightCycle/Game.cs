using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    private Level level;

    public int Money { get; set; }

    public Level Level { get; }
}