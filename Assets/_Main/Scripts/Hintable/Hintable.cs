using UnityEngine;

public class Hintable : MonoBehaviour
{
    [SerializeField]
    private string hintText;

    public string HintText => hintText;
}
