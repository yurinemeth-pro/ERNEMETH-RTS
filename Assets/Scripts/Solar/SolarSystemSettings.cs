using UnityEngine;

[CreateAssetMenu(fileName = "SolarSystemSettings", menuName = "Solar System/Global Settings")]
public class SolarSystemSettings : ScriptableObject
{
    [Header("Escala de Distância (compressão)")]
    [Tooltip("Quantas unidades do Unity representam 1 UA de distância real")]
    public float unitsPerAU = 5f;
}