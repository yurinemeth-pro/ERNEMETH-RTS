using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class StarfieldLayer : MonoBehaviour
{
    [Header("Geração de estrelas (aleatória, gerada 1 única vez)")]
    public int starCount = 1500;
    public float minStarSize = 0.0015f; // tamanho NORMALIZADO (fração do campo, não pixels)
    public float maxStarSize = 0.004f;
    public float minBrightness = 0.4f;
    public float maxBrightness = 1f;
    public Color starTint = Color.white;
    public int randomSeed = 12345;

    [Header("Cobertura (recalculada TODO frame — sempre cobre a tela, sem depender de valor fixo)")]
    public float coverageSafety = 1.3f;

    [Header("Paralaxe (0 = travado na câmera, cresce com o zoom out)")]
    [Range(0f, 0.3f)] public float maxParallaxAtFullZoomOut = 0.08f;
    public float referenceOrthoSizeForParallax = 4000f; // só calibra a INTENSIDADE do paralaxe, não a cobertura

    private Camera mainCam;
    private Transform camTransform;

    void Start()
    {
        mainCam = Camera.main;
        camTransform = mainCam.transform;
        BuildStarMesh();
    }

    Texture2D GenerateStarTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float maxDist = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x, y) - center;
                float dist = p.magnitude / maxDist;

                float core = Mathf.Clamp01(1f - dist * 3f);
                core = core * core;

                float angle = Mathf.Atan2(p.y, p.x);
                float rayPattern = Mathf.Abs(Mathf.Sin(angle * 2f));
                float rayShape = Mathf.Pow(rayPattern, 24f);
                float rayFalloff = Mathf.Clamp01(1f - dist);
                float rays = rayShape * rayFalloff * rayFalloff;

                float alpha = Mathf.Clamp01(core + rays * 0.8f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply();
        return tex;
    }

    void BuildStarMesh()
    {
        Random.State previousState = Random.state;
        Random.InitState(randomSeed);

        // Campo NORMALIZADO: metade do tamanho = 1 (unidade abstrata).
        // A cobertura de tela de verdade é resolvida no LateUpdate, escalando o objeto inteiro.
        const float NORMALIZED_HALF = 1f;

        Vector3[] vertices = new Vector3[starCount * 4];
        Vector2[] uv = new Vector2[starCount * 4];
        int[] triangles = new int[starCount * 6];
        Color[] colors = new Color[starCount * 4];

        for (int i = 0; i < starCount; i++)
        {
            Vector2 pos = new Vector2(
                Random.Range(-NORMALIZED_HALF, NORMALIZED_HALF),
                Random.Range(-NORMALIZED_HALF, NORMALIZED_HALF)
            );

            float size = Random.Range(minStarSize, maxStarSize);
            float brightness = Random.Range(minBrightness, maxBrightness);
            float rot = Random.Range(0f, Mathf.PI * 2f);

            Color c = starTint * brightness;
            c.a = 1f;

            Vector2 right = new Vector2(Mathf.Cos(rot), Mathf.Sin(rot)) * size;
            Vector2 up = new Vector2(-Mathf.Sin(rot), Mathf.Cos(rot)) * size;

            int vi = i * 4;
            vertices[vi + 0] = pos - right - up;
            vertices[vi + 1] = pos + right - up;
            vertices[vi + 2] = pos + right + up;
            vertices[vi + 3] = pos - right + up;

            uv[vi + 0] = new Vector2(0f, 0f);
            uv[vi + 1] = new Vector2(1f, 0f);
            uv[vi + 2] = new Vector2(1f, 1f);
            uv[vi + 3] = new Vector2(0f, 1f);

            colors[vi + 0] = c; colors[vi + 1] = c; colors[vi + 2] = c; colors[vi + 3] = c;

            int ti = i * 6;
            triangles[ti + 0] = vi + 0; triangles[ti + 1] = vi + 2; triangles[ti + 2] = vi + 1;
            triangles[ti + 3] = vi + 0; triangles[ti + 4] = vi + 3; triangles[ti + 5] = vi + 2;
        }

        Mesh mesh = new Mesh();
        if (starCount * 4 > 60000)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.mainTexture = GenerateStarTexture(64);
        GetComponent<MeshRenderer>().material = mat;

        Random.state = previousState;
    }

    void LateUpdate()
    {
        if (camTransform == null || mainCam == null) return;

        // Cobertura recalculada com o zoom e a proporção de tela ATUAIS — nunca fica desatualizada
        float requiredHalfSize = mainCam.orthographicSize * Mathf.Max(mainCam.aspect, 1f) * coverageSafety;
        transform.localScale = Vector3.one * requiredHalfSize;

        float zoomRatio = Mathf.Clamp01(mainCam.orthographicSize / referenceOrthoSizeForParallax);
        float parallaxAmount = zoomRatio * maxParallaxAtFullZoomOut;

        Vector3 camPos = camTransform.position;
        transform.position = new Vector3(
            camPos.x * (1f - parallaxAmount),
            camPos.y * (1f - parallaxAmount),
            transform.position.z
        );
    }
}