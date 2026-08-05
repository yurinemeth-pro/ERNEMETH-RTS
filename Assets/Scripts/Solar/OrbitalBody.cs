using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OrbitalBody : MonoBehaviour
{
    public PlanetData data;
    public GameClock clock;
    public SolarSystemSettings settings;

    [Header("Linha de órbita")]
    public float orbitLinePixelWidth = 2f; // espessura desejada NA TELA, em pixels aproximados

    private LineRenderer orbitLine;
    private Camera mainCam;

    void GenerateSphere(int resolution)
    {
        Mesh mesh = new Mesh();
        int lat = resolution;
        int lon = resolution * 2;

        Vector3[] vertices = new Vector3[(lat + 1) * (lon + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        int index = 0;

        for (int y = 0; y <= lat; y++)
        {
            float theta = y * Mathf.PI / lat;
            for (int x = 0; x <= lon; x++)
            {
                float phi = x * 2 * Mathf.PI / lon;

                // Eixo dos polos alinhado com Z (o eixo que a câmera enxerga de frente)
                vertices[index] = new Vector3(
                    Mathf.Sin(theta) * Mathf.Cos(phi),
                    Mathf.Sin(theta) * Mathf.Sin(phi),
                    Mathf.Cos(theta)
                ) * 0.5f;

                uv[index] = new Vector2((float)x / lon, (float)y / lat);
                index++;
            }
        }

        int[] triangles = new int[lat * lon * 6];
        int t = 0;
        for (int y = 0; y < lat; y++)
        {
            for (int x = 0; x < lon; x++)
            {
                int curr = y * (lon + 1) + x;
                int next = curr + lon + 1;
                triangles[t++] = curr; triangles[t++] = next; triangles[t++] = curr + 1;
                triangles[t++] = curr + 1; triangles[t++] = next; triangles[t++] = next + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    void Start()
    {
        GenerateSphere(data.sphereResolution);

        LineRenderer line = GetComponent<LineRenderer>();
        line.loop = true;
        line.useWorldSpace = true;
        line.positionCount = 100;

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.white;
        line.endColor = Color.white;

        float radius = data.orbitalRadiusAU * settings.unitsPerAU;
        float diameter = settings.useRealisticScale
            ? data.realDiameterAU * settings.unitsPerAU
            : data.visualDiameter;
        transform.localScale = new Vector3(diameter, diameter, 1f);

        for (int i = 0; i < 100; i++)
        {
            float angle = ((float)i / 100) * 360f * Mathf.Deg2Rad;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }

        orbitLine = line;
        mainCam = Camera.main;
    }

    void Update()
    {
        if (data == null || clock == null) return;

        // Dias decorridos desde o início da simulação
        double daysElapsed = (clock.CurrentDate - new System.DateTime(clock.startYear, clock.startMonth, clock.startDay)).TotalDays;

        // Rotação em torno do próprio eixo
        float rotationDegreesPerDay = 360f / (data.rotationPeriodHours / 24f);
        float currentRotation = (float)(rotationDegreesPerDay * daysElapsed) % 360f;
        transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);

        // Quantos graus o planeta girou ao redor do Sol desde o início
        float degreesPerDay = 360f / data.orbitalPeriodDays;
        float currentAngle = data.startingAngleDegrees + (float)(degreesPerDay * daysElapsed);
        float angleRad = currentAngle * Mathf.Deg2Rad;

        // Posição no plano orbital
        float x = Mathf.Cos(angleRad) * data.orbitalRadiusAU * settings.unitsPerAU;
        float y = Mathf.Sin(angleRad) * data.orbitalRadiusAU * settings.unitsPerAU;

        transform.position = new Vector3(x, y, 0f);

        // Espessura da linha de órbita, compensando o zoom da câmera
        if (orbitLine != null && mainCam != null)
        {
            float width = orbitLinePixelWidth * mainCam.orthographicSize * 0.0015f;
            orbitLine.startWidth = width;
            orbitLine.endWidth = width;
        }
    }
}