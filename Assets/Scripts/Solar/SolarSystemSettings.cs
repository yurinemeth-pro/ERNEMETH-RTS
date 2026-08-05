using UnityEngine;

[CreateAssetMenu(fileName = "SolarSystemSettings", menuName = "Solar System/Global Settings")]
public class SolarSystemSettings : ScriptableObject
{
    [Header("Escala de Distância (compressão)")]
    public float unitsPerAU = 100f;

    [Header("Modo de Escala")]
    public bool useRealisticScale = false;

    [Header("Linhas de Órbita")]
    [Range(0f, 1f)]
    public float orbitLineOpacity = 0.5f;
}