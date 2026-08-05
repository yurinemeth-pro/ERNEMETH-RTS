using UnityEngine;

[CreateAssetMenu(fileName = "NewPlanet", menuName = "Solar System/Planet Data")]
public class PlanetData : ScriptableObject
{
    public string planetName;
    public float orbitalRadiusAU = 1f;
    public float orbitalPeriodDays = 365f;
    public float startingAngleDegrees = 0f;
    public float rotationPeriodHours = 24f;

    [Header("Visual")]
    public float visualDiameter = 1f;
    public float realDiameterAU = 0.0001f;
    public int sphereResolution = 32;

    [Header("Destaque (hover)")]
    public Color highlightColor = Color.white; // cor predominante do planeta, usada no brilho ao passar o mouse
}