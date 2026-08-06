using UnityEngine;

public class PlanetRings : MonoBehaviour
{
    private CelestialBodyData data;
    private SolarSystemSettings settings;

    public void Initialize(CelestialBodyData bodyData, SolarSystemSettings globalSettings)
    {
        data = bodyData;
        settings = globalSettings;

        for (int i = 0; i < data.rings.Length; i++)
        {
            CreateRing(data.rings[i], i);
        }
    }

    void CreateRing(RingData ring, int index)
    {
        GameObject ringObj = new GameObject("Ring_" + index);
        ringObj.transform.SetParent(transform, false);

        LineRenderer line = ringObj.AddComponent<LineRenderer>();
        line.loop = true;
        line.useWorldSpace = false;
        line.positionCount = 100;
        line.material = new Material(Shader.Find("Sprites/Default"));

        Color c = ring.color;
        c.a = ring.opacity;
        line.startColor = c;
        line.endColor = c;

        // Raio médio entre borda interna e externa, em unidades LOCAIS
        // (o anel é filho do planeta, então a escala do planeta já multiplica isso automaticamente)
        float avgRadius = (ring.innerRadiusAU + ring.outerRadiusAU) * 0.5f;

        for (int i = 0; i < 100; i++)
        {
            float angle = ((float)i / 100) * 360f * Mathf.Deg2Rad;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * avgRadius, Mathf.Sin(angle) * avgRadius, 0f));
        }

        float width = ring.outerRadiusAU - ring.innerRadiusAU;
        line.startWidth = width;
        line.endWidth = width;
    }
}