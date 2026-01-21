using System.Collections.Generic;
using UnityEngine;

public class InformationPanelView : MonoBehaviour
{
    [SerializeField]
    private Transform[] stations;

    [SerializeField]
    private Transform anchor;

    public Transform Anchor => anchor;
    public IReadOnlyList<Transform> Stations => stations;
}