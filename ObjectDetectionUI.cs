using UnityEngine;
using TMPro;

public class ObjectDetectionUI : MonoBehaviour
{
    [Header("Scene refs")]
    public Camera mainCamera;
    public Canvas canvas;
    public RectTransform boxRect;
    public TextMeshProUGUI label;

    [Header("Tuning")]
    public float padding = 8f;
    public bool clampToScreen = true;

    [Header("Debug")]
    public bool debugLogs = true;

    Renderer targetRenderer;
    float debugTimer;

    void Start()
    {
        if (!debugLogs) return;

        Debug.Log(
            $"[ObjectDetectionUI] Start. " +
            $"mainCamera={(mainCamera ? mainCamera.name : "NULL")}, " +
            $"canvas={(canvas ? canvas.name : "NULL")}, " +
            $"boxRect={(boxRect ? boxRect.name : "NULL")}, " +
            $"label={(label ? label.name : "NULL")}"
        );

        if (boxRect) boxRect.gameObject.SetActive(false);
        if (label) label.gameObject.SetActive(false);
    }

    public void SetTarget(Renderer rend)
    {
        targetRenderer = rend;

        bool active = (targetRenderer != null);

        if (boxRect) boxRect.gameObject.SetActive(active);
        if (label) label.gameObject.SetActive(active);

        if (active && label)
            label.text = targetRenderer.gameObject.name;

        if (debugLogs)
            Debug.Log($"[ObjectDetectionUI] SetTarget: {(rend ? rend.gameObject.name : "NULL")}");
    }

    void Update()
    {
        if (debugLogs)
        {
            debugTimer += Time.deltaTime;
            if (debugTimer > 1f)
            {
                debugTimer = 0f;
                Debug.Log($"[ObjectDetectionUI] running. target={(targetRenderer ? targetRenderer.gameObject.name : "NULL")}, " +
                          $"boxActive={(boxRect ? boxRect.gameObject.activeSelf : false)}, " +
                          $"labelActive={(label ? label.gameObject.activeSelf : false)}");
            }
        }

        if (!targetRenderer) return;

        if (!mainCamera || !canvas || !boxRect)
        {
            if (debugLogs)
                Debug.LogError("[ObjectDetectionUI] Missing refs: mainCamera/canvas/boxRect. Check Inspector.");
            return;
        }

        if (!targetRenderer.gameObject.activeInHierarchy)
        {
            SetTarget(null);
            return;
        }

        if (!TryGetScreenRect(targetRenderer.bounds, out Rect screenRect))
        {
            if (boxRect) boxRect.gameObject.SetActive(false);
            if (label) label.gameObject.SetActive(false);
            return;
        }

        screenRect.xMin -= padding;
        screenRect.yMin -= padding;
        screenRect.xMax += padding;
        screenRect.yMax += padding;

        if (clampToScreen)
        {
            screenRect.xMin = Mathf.Clamp(screenRect.xMin, 0, Screen.width);
            screenRect.xMax = Mathf.Clamp(screenRect.xMax, 0, Screen.width);
            screenRect.yMin = Mathf.Clamp(screenRect.yMin, 0, Screen.height);
            screenRect.yMax = Mathf.Clamp(screenRect.yMax, 0, Screen.height);
        }

        RectTransform canvasRect = canvas.transform as RectTransform;

        Camera eventCam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

        Vector2 minLocal, maxLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenRect.min, eventCam, out minLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenRect.max, eventCam, out maxLocal);

        Vector2 size = maxLocal - minLocal;
        Vector2 center = (minLocal + maxLocal) * 0.5f;

        boxRect.anchoredPosition = center;
        boxRect.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));

        if (label)
        {
            RectTransform labelRect = label.rectTransform;

            labelRect.anchoredPosition = new Vector2(center.x, maxLocal.y + 18f);
            label.text = targetRenderer.gameObject.name;
        }

        boxRect.gameObject.SetActive(true);
        if (label) label.gameObject.SetActive(true);
    }

    bool TryGetScreenRect(Bounds b, out Rect rect)
    {
        Vector3 c = b.center;
        Vector3 e = b.extents;

        Vector3[] pts =
        {
            c + new Vector3( e.x,  e.y,  e.z),
            c + new Vector3( e.x,  e.y, -e.z),
            c + new Vector3( e.x, -e.y,  e.z),
            c + new Vector3( e.x, -e.y, -e.z),
            c + new Vector3(-e.x,  e.y,  e.z),
            c + new Vector3(-e.x,  e.y, -e.z),
            c + new Vector3(-e.x, -e.y,  e.z),
            c + new Vector3(-e.x, -e.y, -e.z),
        };

        float minX = float.PositiveInfinity, minY = float.PositiveInfinity;
        float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;

        bool anyInFront = false;

        for (int i = 0; i < pts.Length; i++)
        {
            Vector3 sp = mainCamera.WorldToScreenPoint(pts[i]);

            if (sp.z > 0) anyInFront = true;

            minX = Mathf.Min(minX, sp.x);
            minY = Mathf.Min(minY, sp.y);
            maxX = Mathf.Max(maxX, sp.x);
            maxY = Mathf.Max(maxY, sp.y);
        }

        if (!anyInFront)
        {
            rect = default;
            return false;
        }

        rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
        return true;
    }
}
