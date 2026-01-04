using UnityEngine;

public class Hintable : MonoBehaviour, IHintable
{
    [SerializeField]
    private string hintText;

    public string HintText => hintText;
}
