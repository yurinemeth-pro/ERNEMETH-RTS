using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("Zoom (multiplicativo)")]
    public float zoomSpeed = 0.15f; // 15% de mudança por "clique" de scroll
    public float minZoom = 2f;      // bem perto (nível planeta)
    public float maxZoom = 4000f;   // sistema solar inteiro cabe

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0f)
        {
            // Multiplicativo: cada clique muda uma PORCENTAGEM do valor atual, não um valor fixo
            float zoomFactor = 1f - (scrollInput * zoomSpeed * 10f);
            cam.orthographicSize *= zoomFactor;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }
}