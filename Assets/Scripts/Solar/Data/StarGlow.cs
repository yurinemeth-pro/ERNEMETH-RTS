using UnityEngine;

public class StarGlow : MonoBehaviour
{
    private StarData data;
    private Renderer rend;
    private MaterialPropertyBlock propBlock;

    public void Initialize(StarData starData, Renderer targetRenderer)
    {
        data = starData;
        rend = targetRenderer;
        propBlock = new MaterialPropertyBlock();

        rend.material.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        if (data == null || rend == null) return;

        float pulse = 1f + Mathf.Sin(Time.time * data.starPulseSpeed) * data.starPulseAmount;
        float intensity = data.starEmissionIntensity * pulse;
        Color hdrColor = data.starColor * intensity;

        rend.GetPropertyBlock(propBlock);
        propBlock.SetColor("_EmissionColor", hdrColor);
        rend.SetPropertyBlock(propBlock);
    }
}