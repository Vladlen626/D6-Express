using TMPro;
using UnityEngine;

public class InformationPanelStationView : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro stationName;

    [SerializeField]
    private Color activeColor;

    [SerializeField]
    private Color inactiveColor;

    public void SetStationName(string text)
    {
        stationName.text = text;
    }

    public void SetActiveStation(bool active)
    {
        stationName.color = active ? activeColor : inactiveColor;
    }

    public Vector3 Position => transform.position;
}