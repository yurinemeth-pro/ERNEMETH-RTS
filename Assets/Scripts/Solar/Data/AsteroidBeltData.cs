using UnityEngine;

[CreateAssetMenu(fileName = "NewAsteroidBelt", menuName = "Solar System/Asteroid Belt Data")]
public class AsteroidBeltData : ScriptableObject
{
    public string beltName;
    public float innerRadiusAU = 2.2f;
    public float outerRadiusAU = 3.2f;
    public int asteroidCount = 300;
    public float minVisualSize = 0.02f;
    public float maxVisualSize = 0.08f;
    public Color color = new Color(0.6f, 0.55f, 0.5f);
    public float orbitalPeriodDaysAtInnerEdge = 1400f; // referência de velocidade angular
}