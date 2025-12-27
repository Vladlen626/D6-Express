using System.Collections.Generic;
using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;

[ExecuteInEditMode]
public class TrainGenerator : MonoBehaviour
{
    [SerializeField]
    private bool shouldGenerate = false;

    [SerializeField]
    private Transform trainRoot;

    [SerializeField]
    [Header("Floor")]
    private GameObject floorPrefab;

    [SerializeField]
    [Header("Floor")]
    private float floorWidth = 1;

    [SerializeField]
    [Header("Ceiling")]
    private GameObject ceilingPrefab;

    [SerializeField]
    [Header("Ceiling")]
    private float ceilingHeightPosition;

    [SerializeField]
    [Header("Start Hallway")]
    private GameObject startHallwayPrefab;

    [SerializeField]
    [Header("Start Hallway")]
    private float startHallwayZStep;

    [SerializeField]
    [Header("Start Section Bio")]
    private GameObject startSectionBioPrefab;

    [SerializeField]
    [Header("Start Section Bio")]
    private float startSectionBioZStep;

    [SerializeField]
    [Header("Sections")]
    private GameObject sectionPrefab;

    [SerializeField]
    [Header("Sections")]
    private int sectionsCount;

    [SerializeField]
    [Header("Sections")]
    private float sectionZStep;

    [SerializeField]
    [Header("End Section Bio")]
    private GameObject endSectionBioPrefab;

    [SerializeField]
    [Header("End Section Bio")]
    private float endSectionBioZStep;

    [SerializeField]
    [Header("End Hallway")]
    private GameObject endHallwayPrefab;

    [SerializeField]
    [Header("End Hallway")]
    private float endHallwayZStep;

    private bool generated;

    private int hack = 50;

    [ContextMenu("Generate Train")]
    private void GenerateTrain()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
            hack--;
            if (hack == 0)
            {
                break;
            }
        }

        var currentPos = new Vector3(0, 0, -CalculateTrainLength() / 2);

        Instantiate(startHallwayPrefab, currentPos, Quaternion.identity, trainRoot);
        currentPos.z += startHallwayZStep;

        Instantiate(startSectionBioPrefab, currentPos, Quaternion.identity, trainRoot);
        currentPos.z += startSectionBioZStep;

        var lastSectionPos = currentPos;
        GameObject[] sections = new GameObject[sectionsCount];
        for (int i = 0; i < sectionsCount; i++)
        {
            sections[i] = Instantiate(sectionPrefab, lastSectionPos, Quaternion.identity, trainRoot);
            lastSectionPos += new Vector3(0, 0, sectionZStep);
        }
        currentPos = lastSectionPos;

        Instantiate(endSectionBioPrefab, currentPos, Quaternion.identity, trainRoot);
        currentPos.z += endSectionBioZStep;

        Instantiate(endHallwayPrefab, currentPos, Quaternion.identity, trainRoot);
        currentPos.z += endHallwayZStep;

        // GenerateCeiling(CalculateTrainLength());
        GenerateFloor(CalculateTrainLength());

        generated = true;
    }

    private void GenerateFloor(float length)
    {
        GameObject floor = Instantiate(floorPrefab, trainRoot.position, Quaternion.identity, trainRoot);
        Renderer floorRend = floor.GetComponent<Renderer>();
        float originalLength = floorRend.bounds.size.z;

        floor.transform.localScale = new Vector3(floorWidth, 1f, length / originalLength);
        floor.transform.localPosition = new Vector3(0, 0, 0);
    }

    private void GenerateCeiling(float length)
    {
        GameObject ceiling = Instantiate(ceilingPrefab, trainRoot.position, Quaternion.identity, trainRoot);
        Renderer ceilingRend = ceiling.GetComponent<Renderer>();
        float originalLength = ceilingRend.bounds.size.z;

        ceiling.transform.localScale = new Vector3(floorWidth, 1f, length / originalLength);
        ceiling.transform.localPosition = new Vector3(0, ceilingHeightPosition, 0);
    }

    float CalculateTrainLength()
    {
        return startHallwayZStep + startSectionBioZStep +
               (sectionsCount * sectionZStep) +
               endSectionBioZStep + endHallwayZStep;
    }

    private void OnValidate()
    {
        if (!shouldGenerate || generated)
        {
            return;
        }

        GenerateTrain();
    }
}