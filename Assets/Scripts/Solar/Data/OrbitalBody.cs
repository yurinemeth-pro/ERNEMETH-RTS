using UnityEngine;

public class OrbitalBody : MonoBehaviour
{
    public CelestialBodyData data;
    public GameClock clock;
    public SolarSystemSettings settings;

    [Header("Hierarquia orbital")]
    public Transform orbitCenter;

    [Header("Linha de órbita")]
    public float orbitLinePixelWidth = 2f;
    public int orbitSegments = 200;

    [Header("Seleção (destaque)")]
    public float orbitHighlightPixelWidth = 5f;
    public float highlightFadeSpeed = 4f;
    public float markerPixelSize = 15f;
    [Range(0f, 1f)] public float markerMaxOpacity = 0.5f;

    public float OrbitRadiusWorld { get; private set; }
    public Vector3 OrbitCenterPosition => orbitCenter != null ? orbitCenter.position : Vector3.zero;

    private LineRenderer orbitLine;
    private LineRenderer highlightLine;
    private MeshRenderer markerRenderer;
    private Material markerMaterial;
    private Camera mainCam;
    private Renderer rend;
    private bool isHighlighted;
    private float highlightAmount;
    private OrbitingBodyData orbitData;

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

    LineRenderer CreateLineObject(string objName)
    {
        GameObject obj = new GameObject(objName);
        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.loop = true;
        line.useWorldSpace = true;
        line.material = new Material(Shader.Find("Sprites/Default"));
        return line;
    }

    Mesh BuildFilledCircleMesh(int segments)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // centro

        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / segments) * 360f * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 0.5f; // raio 0.5 (diâmetro 1)
        }

        for (int i = 0; i < segments; i++)
        {
            int current = i + 1;
            int next = (i + 1) % segments + 1;

            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = current;
            triangles[i * 3 + 2] = next;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    void CreateMarker()
    {
        GameObject obj = new GameObject(data.bodyName + "_Marker");
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        markerRenderer = obj.AddComponent<MeshRenderer>();

        mf.mesh = BuildFilledCircleMesh(40);

        markerMaterial = new Material(Shader.Find("Sprites/Default"));
        markerRenderer.material = markerMaterial;

        markerRenderer.enabled = false;
    }

    void Start()
    {
        GenerateSphere(data.sphereResolution);
        rend = GetComponent<Renderer>();

        if (rend != null)
        {
            rend.material.EnableKeyword("_EMISSION");
        }

        if (data is StarData starData && rend != null)
        {
            gameObject.AddComponent<StarGlow>().Initialize(starData, rend);
            GameObject raysObj = new GameObject(data.bodyName + "_Rays");
            raysObj.transform.SetParent(transform, false);
            raysObj.AddComponent<SunRays>().Initialize(starData);
        }

        orbitData = data as OrbitingBodyData;
        OrbitRadiusWorld = orbitData != null ? orbitData.orbitalRadiusAU * settings.unitsPerAU : 0f;

        if (orbitData != null)
        {
            orbitLine = CreateLineObject(data.bodyName + "_OrbitLine");
            orbitLine.positionCount = orbitSegments;

            highlightLine = CreateLineObject(data.bodyName + "_OrbitHighlight");
            highlightLine.positionCount = orbitSegments;
        }
        else
        {
            transform.position = Vector3.zero;
        }

        CreateMarker();

        float diameter = settings.useRealisticScale
            ? data.realDiameterAU * settings.unitsPerAU
            : data.visualDiameter;
        transform.localScale = new Vector3(diameter, diameter, 1f);

        mainCam = Camera.main;
    }

    void Update()
    {
        if (data == null || clock == null) return;

        double daysElapsed = (clock.CurrentDate - new System.DateTime(clock.startYear, clock.startMonth, clock.startDay)).TotalDays;

        float rotationDegreesPerDay = data.rotationPeriodHours > 0f ? 360f / (data.rotationPeriodHours / 24f) : 0f;
        float currentRotation = (float)(rotationDegreesPerDay * daysElapsed) % 360f;
        transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);

        Vector3 centerPos = orbitCenter != null ? orbitCenter.position : Vector3.zero;

        if (orbitData != null)
        {
            float degreesPerDay = orbitData.orbitalPeriodDays > 0f ? 360f / orbitData.orbitalPeriodDays : 0f;
            float currentAngle = orbitData.startingAngleDegrees + (float)(degreesPerDay * daysElapsed);
            float angleRad = currentAngle * Mathf.Deg2Rad;

            float x = Mathf.Cos(angleRad) * OrbitRadiusWorld;
            float y = Mathf.Sin(angleRad) * OrbitRadiusWorld;

            transform.position = centerPos + new Vector3(x, y, 0f);
        }

        float target = isHighlighted ? 1f : 0f;
        highlightAmount = Mathf.MoveTowards(highlightAmount, target, Time.deltaTime * highlightFadeSpeed);

        if (rend != null && !(data is StarData))
        {
            Color emissive = Color.Lerp(Color.black, data.highlightColor * 0.6f, highlightAmount);
            rend.material.SetColor("_EmissionColor", emissive);
        }

        if (mainCam == null) return;

        if (orbitData != null && orbitLine != null)
        {
            UpdateCirclePoints(orbitLine, centerPos, OrbitRadiusWorld, orbitSegments);

            float width = orbitLinePixelWidth * mainCam.orthographicSize * 0.0015f;
            orbitLine.startWidth = width;
            orbitLine.endWidth = width;
            Color lineColor = new Color(1f, 1f, 1f, settings.orbitLineOpacity);
            orbitLine.startColor = lineColor;
            orbitLine.endColor = lineColor;
        }

        if (highlightLine != null)
        {
            bool visible = highlightAmount > 0.001f;
            highlightLine.enabled = visible;

            if (visible)
            {
                UpdateCirclePoints(highlightLine, centerPos, OrbitRadiusWorld, orbitSegments);

                float hWidth = orbitHighlightPixelWidth * mainCam.orthographicSize * 0.0015f;
                highlightLine.startWidth = hWidth;
                highlightLine.endWidth = hWidth;

                Color hColor = data.highlightColor;
                hColor.a = highlightAmount;
                highlightLine.startColor = hColor;
                highlightLine.endColor = hColor;
            }
        }

        // Marcador circular preenchido, tamanho constante na tela
        if (markerRenderer != null)
        {
            bool visible = highlightAmount > 0.001f;
            markerRenderer.enabled = visible;

            if (visible)
            {
                float markerDiameter = markerPixelSize * mainCam.orthographicSize * 0.002f;
                markerRenderer.transform.position = transform.position + new Vector3(0f, 0f, -1f); // ligeiramente à frente do planeta
                markerRenderer.transform.localScale = Vector3.one * markerDiameter;

                Color mColor = data.highlightColor;
                mColor.a = highlightAmount * markerMaxOpacity;
                markerMaterial.color = mColor;
            }
        }
    }

    void UpdateCirclePoints(LineRenderer line, Vector3 center, float radius, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / segments) * 360f * Mathf.Deg2Rad;
            Vector3 point = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            line.SetPosition(i, point);
        }
    }

    public void SetHighlighted(bool state)
    {
        isHighlighted = state;
    }
}