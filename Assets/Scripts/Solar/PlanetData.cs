using UnityEngine;

[CreateAssetMenu(fileName = "NewPlanet", menuName = "Solar System/Planet Data")]
public class PlanetData : ScriptableObject
{
    public string planetName;
    public float orbitalRadiusAU = 1f;
    public float orbitalPeriodDays = 365f;
    public float startingAngleDegrees = 0f;

    [Header("Visual")]
    public float visualDiameter = 1f; // diâmetro do planeta na cena, em unidades Unity (não é escala real)

    [Header("Visual - Modo Realista")]
    public float realDiameterAU = 0.0001f; // diâmetro real do planeta, em UA

    [Header("Qualidade visual")]
    public int sphereResolution = 32;

    [Header("Rotação")]
    public float rotationPeriodHours = 24f; // tempo pra dar 1 volta no próprio eixo
}