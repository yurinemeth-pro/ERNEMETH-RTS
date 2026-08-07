using UnityEngine;

public class SunRays : MonoBehaviour
{
    [Header("Raios de luz")]
    public int rayCount = 3000;
    public float minBaseLength = 0.01f;
    public float maxBaseLength = 0.09f;
    public float minWidth = 0.05f;
    public float maxWidth = 1f;
    public float minPulseSpeed = 0.2f;
    public float maxPulseSpeed = 8f;
    [Range(0f, 1f)] public float minOpacity = 0.0001f;
    [Range(0f, 1f)] public float maxOpacity = 0.001f;
    [Range(0f, 0.5f)] public float lengthPulseAmount = 1f;

    private Mesh mesh;
    private int count;
    private float[] angle;
    private float[] baseLength;
    private float[] width;
    private float[] pulseSpeed;
    private float[] pulsePhase;
    private Color rayColor;

    public void Initialize(StarData data)
    {
        rayColor = data != null ? data.starColor : Color.white;
        Build();
    }

    void Build()
    {
        count = rayCount;
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();

        mesh = new Mesh();
        mf.mesh = mesh;
        mr.material = new Material(Shader.Find("Sprites/Default"));

        angle = new float[count];
        baseLength = new float[count];
        width = new float[count];
        pulseSpeed = new float[count];
        pulsePhase = new float[count];

        for (int i = 0; i < count; i++)
        {
            angle[i] = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            baseLength[i] = Random.Range(minBaseLength, maxBaseLength);
            width[i] = Random.Range(minWidth, maxWidth);
            pulseSpeed[i] = Random.Range(minPulseSpeed, maxPulseSpeed);
            pulsePhase[i] = Random.Range(0f, Mathf.PI * 2f);
        }

        RebuildMesh();
    }

    void RebuildMesh()
    {
        Vector3[] vertices = new Vector3[count * 3];
        Color[] colors = new Color[count * 3];
        int[] triangles = new int[count * 3];

        for (int i = 0; i < count; i++)
        {
            float wave = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed[i] + pulsePhase[i]);
            float currentLength = baseLength[i] * (1f - lengthPulseAmount + lengthPulseAmount * wave * 2f);
            float currentOpacity = Mathf.Lerp(minOpacity, maxOpacity, wave);

            Vector2 dir = new Vector2(Mathf.Cos(angle[i]), Mathf.Sin(angle[i]));
            Vector2 perp = new Vector2(-dir.y, dir.x) * width[i];

            int vi = i * 3;
            Color baseColor = rayColor; baseColor.a = currentOpacity;
            Color tipColor = rayColor; tipColor.a = 0f; // esvai até ficar transparente na ponta

            vertices[vi + 0] = new Vector3(-perp.x, -perp.y, 0f);
            vertices[vi + 1] = new Vector3(perp.x, perp.y, 0f);
            vertices[vi + 2] = new Vector3(dir.x * currentLength, dir.y * currentLength, 0f);

            colors[vi + 0] = baseColor;
            colors[vi + 1] = baseColor;
            colors[vi + 2] = tipColor;

            triangles[vi + 0] = vi + 0;
            triangles[vi + 1] = vi + 1;
            triangles[vi + 2] = vi + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    void LateUpdate()
    {
        // Os raios NÃO acompanham a autorrotação do Sol (senão pareceriam girar junto com a textura)
        transform.rotation = Quaternion.identity;
        RebuildMesh();
    }
}