using UnityEngine;

[CreateAssetMenu(fileName = "NewStar", menuName = "Solar System/Star Data")]
public class StarData : CelestialBodyData
{
    [Header("Brilho")]
    public Color starColor = Color.white;
    public float starEmissionIntensity = 3f;
    public float starPulseSpeed = 0.5f;
    public float starPulseAmount = 0.15f;
}