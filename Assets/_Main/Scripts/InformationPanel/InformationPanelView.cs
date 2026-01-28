using System.Collections.Generic;
using UnityEngine;

public class InformationPanelView : MonoBehaviour
{
    [SerializeField]
    private InformationPanelStationView[] stations;

    public IReadOnlyList<InformationPanelStationView> Stations => stations;
}