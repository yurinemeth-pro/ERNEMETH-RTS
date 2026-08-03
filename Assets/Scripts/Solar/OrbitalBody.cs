using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OrbitalBody : MonoBehaviour
{   
    
    public PlanetData data;
    public GameClock clock;

    public SolarSystemSettings settings;

    void Update()
    {
        if (data == null || clock == null) return;

        // Dias decorridos desde o início da simulação
        double daysElapsed = (clock.CurrentDate - new System.DateTime(clock.startYear, clock.startMonth, clock.startDay)).TotalDays;

        // Quantos graus o planeta girou desde o início
        float degreesPerDay = 360f / data.orbitalPeriodDays;
        float currentAngle = data.startingAngleDegrees + (float)(degreesPerDay * daysElapsed);
        float angleRad = currentAngle * Mathf.Deg2Rad;

        // Posição no plano orbital
        float x = Mathf.Cos(angleRad) * data.orbitalRadiusAU * settings.unitsPerAU;
        float y = Mathf.Sin(angleRad) * data.orbitalRadiusAU * settings.unitsPerAU;

        transform.position = new Vector3(x, y, 0f);
    }

    void Start()
    {
        LineRenderer line = GetComponent<LineRenderer>();
        line.loop = true;
        line.useWorldSpace = true;
        line.positionCount = 100;
        float lineWidth = 0.05f / data.visualDiameter;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        float radius = data.orbitalRadiusAU * settings.unitsPerAU;
        transform.localScale = new Vector3(data.visualDiameter, data.visualDiameter, 1f);
        for (int i = 0; i < 100; i++)
        {
            float angle = ((float)i / 100) * 360f * Mathf.Deg2Rad;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }
}