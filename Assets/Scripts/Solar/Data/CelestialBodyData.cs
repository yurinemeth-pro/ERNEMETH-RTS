using UnityEngine;

[System.Serializable]
public class RingData
{
    public float innerRadiusAU = 1.2f; // distância do centro do planeta até a borda interna do anel
    public float outerRadiusAU = 1.5f;
    public Color color = Color.white;
    [Range(0f, 1f)] public float opacity = 0.6f;
}

public abstract class CelestialBodyData : ScriptableObject
{
    public string bodyName;
    public float rotationPeriodHours = 24f;

    [Header("Visual")]
    public float visualDiameter = 1f;
    public float realDiameterAU = 0.0001f;
    public int sphereResolution = 32;

    [Header("Destaque (hover/seleção)")]
    public Color highlightColor = Color.white;

    [Header("Anéis (opcional)")]
    public RingData[] rings;
}