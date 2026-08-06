using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class StarfieldLayer : MonoBehaviour
{
    [Header("Geração de estrelas")]
    public int textureResolution = 512;
    public int starCount = 200;
    public Color starColor = Color.white;
    public float minStarSize = 1f;
    public float maxStarSize = 2.5f;
    [Range(0f, 0.3f)] public float dustOpacity = 0.05f;

    [Header("Paralaxe")]
    [Range(0f, 1f)] public float parallaxFactor = 0.5f; // 0 = parece infinitamente distante, 1 = acompanha o mundo
    public float quadWorldSize = 400f;
    public float tiling = 4f;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        GenerateTexture();
        BuildQuad();
    }

    void GenerateTexture()
    {
        Texture2D tex = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[textureResolution * textureResolution];
        Color transparent = new Color(0f, 0f, 0f, 0f);
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

        for (int i = 0; i < pixels.Length; i++)
        {
            if (Random.value < dustOpacity)
            {
                float shade = Random.Range(0.05f, 0.15f);
                pixels[i] = new Color(shade, shade, shade, shade);
            }
        }
        tex.SetPixels(pixels);

        for (int i = 0; i < starCount; i++)
        {
            int cx = Random.Range(0, textureResolution);
            int cy = Random.Range(0, textureResolution);
            float size = Random.Range(minStarSize, maxStarSize);
            float brightness = Random.Range(0.5f, 1f);
            int radius = Mathf.CeilToInt(size);

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int px = cx + dx;
                    int py = cy + dy;
                    if (px < 0 || px >= textureResolution || py < 0 || py >= textureResolution) continue;

                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > size) continue;

                    float falloff = 1f - (dist / size);
                    Color c = starColor * brightness;
                    c.a = falloff;
                    tex.SetPixel(px, py, c);
                }
            }
        }

        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.mainTexture = tex;
        mat.mainTextureScale = new Vector2(tiling, tiling);

        GetComponent<MeshRenderer>().material = mat;
    }

    void BuildQuad()
    {
        Mesh mesh = new Mesh();
        float h = quadWorldSize * 0.5f;

        mesh.vertices = new Vector3[]
        {
            new Vector3(-h, -h, 0), new Vector3(h, -h, 0),
            new Vector3(h, h, 0), new Vector3(-h, h, 0)
        };
        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0), new Vector2(tiling, 0),
            new Vector2(tiling, tiling), new Vector2(0, tiling)
        };
        mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    void LateUpdate()
    {
        if (mainCam == null) return;

        Vector3 camPos = mainCam.transform.position;
        Vector3 newPos = transform.position;
        newPos.x = camPos.x * (1f - parallaxFactor);
        newPos.y = camPos.y * (1f - parallaxFactor);
        transform.position = newPos;
    }
}