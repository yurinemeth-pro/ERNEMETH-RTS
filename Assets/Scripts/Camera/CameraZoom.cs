using UnityEngine;
using System.Collections;

public class CameraZoom : MonoBehaviour
{
    [Header("Zoom (multiplicativo)")]
    public float zoomSpeed = 0.15f;
    public float minZoom = 2f;
    public float maxZoom = 4000f;

    [Header("Pan (arrastar com o botão do meio)")]
    public int panMouseButton = 2;

    [Header("Foco em planeta (clique)")]
    public Transform referencePlanet; // arraste o objeto Jupiter aqui
    public float zoomPadding = 1.6f;  // margem extra ao redor do diâmetro de referência
    public float focusTransitionDuration = 1f;

    private Camera cam;
    private Vector3 dragOrigin;
    private bool isDragging;
    private Transform followTarget;
    private OrbitalBody hoveredBody;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        HandleZoom();
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
            cam.orthographicSize *= zoomFactor;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
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
        float closestDistance = Mathf.Infinity;

        foreach (OrbitalBody body in allBodies)
        {
            float distance = Vector3.Distance(mouseWorldPos, body.transform.position);
            float tolerance = Mathf.Max(body.transform.localScale.x * 0.6f, cam.orthographicSize * 0.02f);

            if (distance < tolerance && distance < closestDistance)
            {
                closest = body;
                closestDistance = distance;
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

        float targetZoom = referencePlanet != null
            ? referencePlanet.localScale.x * zoomPadding
            : cam.orthographicSize;

        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        StopAllCoroutines();
        StartCoroutine(TransitionToPlanet(body.transform, targetZoom));
    }

    IEnumerator TransitionToPlanet(Transform target, float targetZoom)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        float startZoom = cam.orthographicSize;

        while (elapsed < focusTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float tNorm = elapsed / focusTransitionDuration;

            Vector3 targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(startPos, targetPos, tNorm);
            cam.orthographicSize = Mathf.Lerp(startZoom, targetZoom, tNorm);

            yield return null;
        }

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