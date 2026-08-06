using UnityEngine;
using System.Collections;

public class CameraZoom : MonoBehaviour
{
    [Header("Zoom (multiplicativo, suave)")]
    public float zoomSpeed = 0.15f;
    public float zoomSmoothSpeed = 8f; // maior = alcança o alvo mais rápido
    public float minZoom = 2f;
    public float maxZoom = 4000f;

    [Header("Pan (arrastar com o botão do meio)")]
    public int panMouseButton = 2;

    [Header("Foco em planeta (clique)")]
    public Transform referencePlanet;
    public float zoomPadding = 1.6f;
    public float focusTransitionDuration = 1f;

    [Header("Tolerância de clique/hover (ajustável)")]
    public float bodyToleranceMultiplier = 1.5f;   // proximidade do PRÓPRIO planeta
    public float bodyMinTolerranceFactor = 0.09f;  // tolerância mínima, relativa ao zoom atual
    public float ringToleranceFactor = 0.09f;      // proximidade da LINHA de órbita
    public float ringMinTolerance = 0.6f;

    private Camera cam;
    private float targetZoom;
    private bool isTransitioning;

    private Vector3 dragOrigin;
    private bool isDragging;
    private Transform followTarget;
    private OrbitalBody hoveredBody;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;
    }

    void Update()
    {
        HandleZoom();
        ApplyZoomSmoothing();
        HandleHover();
        HandleClick();
        HandlePan();
        HandleFollow();
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            float zoomFactor = 1f - (scrollInput * zoomSpeed * 10f);
            targetZoom *= zoomFactor;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
    }

    void ApplyZoomSmoothing()
    {
        if (isTransitioning) return; // durante o foco automático, a coroutine controla o zoom
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSmoothSpeed);
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(transform.position.z);
        return cam.ScreenToWorldPoint(mouseScreenPos);
    }

    OrbitalBody FindClosestBodyToMouse()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        OrbitalBody[] allBodies = FindObjectsOfType<OrbitalBody>();

        OrbitalBody closest = null;
        float closestScore = Mathf.Infinity;
        float distToOrigin = Vector3.Distance(mouseWorldPos, Vector3.zero);

        foreach (OrbitalBody body in allBodies)
        {
            float bodyDistance = Vector3.Distance(mouseWorldPos, body.transform.position);
            float bodyTolerance = Mathf.Max(body.transform.localScale.x * bodyToleranceMultiplier, cam.orthographicSize * bodyMinTolerranceFactor);

            if (bodyDistance < bodyTolerance && bodyDistance < closestScore)
            {
                closest = body;
                closestScore = bodyDistance;
            }

            if (body.OrbitRadiusWorld > 0f)
            {
                float ringDistance = Mathf.Abs(distToOrigin - body.OrbitRadiusWorld);
                float ringTolerance = Mathf.Max(cam.orthographicSize * ringToleranceFactor, ringMinTolerance);

                if (ringDistance < ringTolerance && ringDistance < closestScore)
                {
                    closest = body;
                    closestScore = ringDistance;
                }
            }
        }

        return closest;
    }

    void HandleHover()
    {
        OrbitalBody closest = FindClosestBodyToMouse();

        if (closest != hoveredBody)
        {
            if (hoveredBody != null) hoveredBody.SetHighlighted(false);
            hoveredBody = closest;
            if (hoveredBody != null) hoveredBody.SetHighlighted(true);
        }
    }

    void HandleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OrbitalBody closest = FindClosestBodyToMouse();
            if (closest != null)
            {
                FocusOnPlanet(closest);
            }
        }
    }

    void FocusOnPlanet(OrbitalBody body)
    {
        followTarget = null;

        float focusZoom = referencePlanet != null
            ? referencePlanet.localScale.x * zoomPadding
            : cam.orthographicSize;

        focusZoom = Mathf.Clamp(focusZoom, minZoom, maxZoom);
        targetZoom = focusZoom;

        StopAllCoroutines();
        StartCoroutine(TransitionToPlanet(body.transform, focusZoom));
    }

    IEnumerator TransitionToPlanet(Transform target, float targetZoomValue)
    {
        isTransitioning = true;

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        float startZoom = cam.orthographicSize;

        while (elapsed < focusTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float tNorm = elapsed / focusTransitionDuration;

            Vector3 targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(startPos, targetPos, tNorm);
            cam.orthographicSize = Mathf.Lerp(startZoom, targetZoomValue, tNorm);

            yield return null;
        }

        isTransitioning = false;
        followTarget = target;
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(panMouseButton))
        {
            dragOrigin = GetMouseWorldPosition();
            isDragging = true;
            followTarget = null;
            StopAllCoroutines();
            isTransitioning = false;
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

    void HandleFollow()
    {
        if (followTarget != null)
        {
            Vector3 targetPos = followTarget.position;
            transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        }
    }
}