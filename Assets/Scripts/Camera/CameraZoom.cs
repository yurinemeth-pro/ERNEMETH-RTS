using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("Zoom (multiplicativo)")]
    public float zoomSpeed = 0.15f;
    public float minZoom = 2f;
    public float maxZoom = 4000f;

    [Header("Pan (arrastar com o botão do meio)")]
    public int panMouseButton = 2; // 0 = esquerdo, 1 = direito, 2 = meio

    private Camera cam;
    private Vector3 dragOrigin;
    private bool isDragging;
    private Transform followTarget;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        HandleZoom();
        HandleClickToFollow();
        HandlePan();
        HandleFollow();
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            float zoomFactor = 1f - (scrollInput * zoomSpeed * 10f);
            cam.orthographicSize *= zoomFactor;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(transform.position.z); // distância da câmera até o plano do jogo
        return cam.ScreenToWorldPoint(mouseScreenPos);
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(panMouseButton))
        {
            dragOrigin = GetMouseWorldPosition();
            isDragging = true;
            followTarget = null; // arrastar manualmente cancela o "seguir planeta"
        }

        if (Input.GetMouseButton(panMouseButton) && isDragging)
        {
            Vector3 difference = dragOrigin - GetMouseWorldPosition();
            transform.position += difference;
        }

        if (Input.GetMouseButtonUp(panMouseButton))
        {
            isDragging = false;
        }
    }

    void HandleClickToFollow()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 clickWorldPos = GetMouseWorldPosition();
            OrbitalBody[] allBodies = FindObjectsOfType<OrbitalBody>();

            OrbitalBody closest = null;
            float closestDistance = Mathf.Infinity;

            foreach (OrbitalBody body in allBodies)
            {
                float distance = Vector3.Distance(clickWorldPos, body.transform.position);

                // Tolerância de clique: nunca menor que um valor mínimo (facilita acertar planetas pequenos),
                // mas cresce com o zoom out, igual fizemos com a espessura da linha de órbita
                float clickTolerance = Mathf.Max(body.transform.localScale.x * 0.6f, cam.orthographicSize * 0.02f);

                if (distance < clickTolerance && distance < closestDistance)
                {
                    closest = body;
                    closestDistance = distance;
                }
            }

            if (closest != null)
            {
                followTarget = closest.transform;
            }
        }
    }

    void HandleFollow()
    {
        if (followTarget != null)
        {
            Vector3 targetPos = followTarget.position;
            transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        }
    }
}