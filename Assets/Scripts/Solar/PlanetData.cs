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
}