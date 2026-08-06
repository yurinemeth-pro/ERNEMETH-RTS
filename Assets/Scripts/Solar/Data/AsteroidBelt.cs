using UnityEngine;

public class AsteroidBelt : MonoBehaviour
{
    public AsteroidBeltData data;
    public SolarSystemSettings settings;
    public GameClock clock;

    private Transform[] asteroids;
    private float[] radii;
    private float[] startAngles;
    private float[] angularSpeeds; // graus/dia, varia por raio (mais perto do Sol = mais rápido)

    void Start()
    {
        asteroids = new Transform[data.asteroidCount];
        radii = new float[data.asteroidCount];
        startAngles = new float[data.asteroidCount];
        angularSpeeds = new float[data.asteroidCount];

        for (int i = 0; i < data.asteroidCount; i++)
        {
            GameObject a = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            a.transform.SetParent(transform, false);
            a.name = "Asteroid_" + i;

            float size = Random.Range(data.minVisualSize, data.maxVisualSize);
            a.transform.localScale = Vector3.one * size;

            Renderer r = a.GetComponent<Renderer>();
            r.material.color = data.color;

            radii[i] = Random.Range(data.innerRadiusAU, data.outerRadiusAU) * settings.unitsPerAU;
            startAngles[i] = Random.Range(0f, 360f);

            // Corpos mais próximos do Sol orbitam mais rápido (aproximação simplificada, não Kepler exato)
            float normalizedRadius = radii[i] / (data.outerRadiusAU * settings.unitsPerAU);
            angularSpeeds[i] = (360f / data.orbitalPeriodDaysAtInnerEdge) / Mathf.Max(normalizedRadius, 0.1f);

            asteroids[i] = a.transform;
        }
    }

    void Update()
    {
        if (clock == null) return;

        double daysElapsed = (clock.CurrentDate - new System.DateTime(clock.startYear, clock.startMonth, clock.startDay)).TotalDays;

        for (int i = 0; i < asteroids.Length; i++)
        {
            float angle = (startAngles[i] + angularSpeeds[i] * (float)daysElapsed) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radii[i];
            float y = Mathf.Sin(angle) * radii[i];
            asteroids[i].position = new Vector3(x, y, 0f);
        }
    }
}