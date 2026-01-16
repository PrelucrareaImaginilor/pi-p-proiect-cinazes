using UnityEngine;
using TMPro;

/*
    Manages UI elements that display detection information
    Creates a bounding box and label around detected objects in screen space
    Works in conjunction with GazeDetector.cs
*/
public class ObjectDetectionUI : MonoBehaviour
{
    [Header("Scene refs")]
    public Camera mainCamera;            // camera used for world-to-screen coordinate conversion
    public Canvas canvas;                // parent canvas for UI elements
    public RectTransform boxRect;        // UI rectangle far forms the bounding box
    public TextMeshProUGUI label;        // text label displaying object name

    [Header("Tuning")]
    public float padding = 8f;           // additional padding around the object in pixels
    public bool clampToScreen = true;    // whether to keep UI elements within screen bounds

    [Header("Debug")]
    public bool debugLogs = true;        // enable/disable debug logging for troubleshooting

    // Internal tracking variables
    Renderer targetRenderer;             // currently targeted object's renderer
    float debugTimer;                    // timer for periodic debug logging

    /*
        Initializes UI elements and performs initial validation
        Called when the script instance is being loaded
    */
    void Start()
    {
        // debug logging setup
        if (!debugLogs) return;

        Debug.Log(
            $"[ObjectDetectionUI] Start. " +
            $"mainCamera={(mainCamera ? mainCamera.name : "NULL")}, " +
            $"canvas={(canvas ? canvas.name : "NULL")}, " +
            $"boxRect={(boxRect ? boxRect.name : "NULL")}, " +
            $"label={(label ? label.name : "NULL")}"
        );

        // initially hide UI elements
        if (boxRect) boxRect.gameObject.SetActive(false);
        if (label) label.gameObject.SetActive(false);
    }

    /*
        sets the current target object for UI display
        called by GazeDetector when a new object is detected
        rend : renderer component of the target object (null to clear target)
    */
    public void SetTarget(Renderer rend)
    {
        targetRenderer = rend; // store reference to target object

        // determine if UI should be active
        bool active = (targetRenderer != null);

        // show/hide UI elements based on target presence
        if (boxRect) boxRect.gameObject.SetActive(active);
        if (label) label.gameObject.SetActive(active);

        // update label text if we have a target
        if (active && label)
            label.text = targetRenderer.gameObject.name;

        // debug logging
        if (debugLogs)
            Debug.Log($"[ObjectDetectionUI] SetTarget: {(rend ? rend.gameObject.name : "NULL")}");
    }

    /*
        updates UI element positions and visibility each frame
        calculates screen-space bounding box and positions UI elements accordingly
    */
    void Update()
    {
        // periodic debug logging 
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

        // early exit if no target
        if (!targetRenderer) return;

        // validate required references
        if (!mainCamera || !canvas || !boxRect)
        {
            if (debugLogs)
                Debug.LogError("[ObjectDetectionUI] Missing refs: mainCamera/canvas/boxRect. Check Inspector.");
            return;
        }

        // check if target object is still active in hierarchy
        if (!targetRenderer.gameObject.activeInHierarchy)
        {
            SetTarget(null);
            return;
        }

        // calculate screen-space rectangle for target object
        if (!TryGetScreenRect(targetRenderer.bounds, out Rect screenRect))
        {
            // object is behind camera or not visible - hide UI
            if (boxRect) boxRect.gameObject.SetActive(false);
            if (label) label.gameObject.SetActive(false);
            return;
        }

        // add padding to the calculated rectangle
        screenRect.xMin -= padding;
        screenRect.yMin -= padding;
        screenRect.xMax += padding;
        screenRect.yMax += padding;

        // optionally clamp rectangle to screen boundaries
        if (clampToScreen)
        {
            screenRect.xMin = Mathf.Clamp(screenRect.xMin, 0, Screen.width);
            screenRect.xMax = Mathf.Clamp(screenRect.xMax, 0, Screen.width);
            screenRect.yMin = Mathf.Clamp(screenRect.yMin, 0, Screen.height);
            screenRect.yMax = Mathf.Clamp(screenRect.yMax, 0, Screen.height);
        }

        // get canvas RectTransform for coordinate conversion
        RectTransform canvasRect = canvas.transform as RectTransform;

        // determine which camea to use for coordinate conversion
        // (null for ScreenSpaceOverlay, otherwise use canvas.worldCamera)
        Camera eventCam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

        // convert screen coordinates to canvas-relative coordinates
        Vector2 minLocal, maxLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenRect.min, eventCam, out minLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenRect.max, eventCam, out maxLocal);

        // calculate size and center position for the bounding box
        Vector2 size = maxLocal - minLocal;
        Vector2 center = (minLocal + maxLocal) * 0.5f;

        // apply position and size to bounding box
        boxRect.anchoredPosition = center;
        boxRect.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));

        // position label above the bounding box
        if (label)
        {
            RectTransform labelRect = label.rectTransform;

            labelRect.anchoredPosition = new Vector2(center.x, maxLocal.y + 18f);
            label.text = targetRenderer.gameObject.name;
        }

        // ensure UI elements are visible
        boxRect.gameObject.SetActive(true);
        if (label) label.gameObject.SetActive(true);
    }

    /*
        calculates a screen-space rectangle that encloses a 3D object's bounds
        projects all 8 corners of the bounding box to screen space
        b: 3D bounds of the target object
        rect: output screen-space rectangle
        return TRUE if object is visible (at least one point in front of camera)
    */
    bool TryGetScreenRect(Bounds b, out Rect rect)
    {
        // get center and extents of the bounding box
        Vector3 c = b.center;
        Vector3 e = b.extents;

        // calculate all 8 corners of the bounding box
        Vector3[] pts =
        {
            c + new Vector3( e.x,  e.y,  e.z),    // front-top-right
            c + new Vector3( e.x,  e.y, -e.z),    // back-top-right
            c + new Vector3( e.x, -e.y,  e.z),    // front-bottom-right
            c + new Vector3( e.x, -e.y, -e.z),    // back-bottom-right
            c + new Vector3(-e.x,  e.y,  e.z),    // front-top-left
            c + new Vector3(-e.x,  e.y, -e.z),    // back-top-left
            c + new Vector3(-e.x, -e.y,  e.z),    // front-bottom-left
            c + new Vector3(-e.x, -e.y, -e.z),    // back-bottom-left
        };

        // initialize min/max values for rectangle calculation
        float minX = float.PositiveInfinity, minY = float.PositiveInfinity;
        float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;

        bool anyInFront = false;    // track if any point is in front of camera

        // project each corner to screen space and find extremes
        for (int i = 0; i < pts.Length; i++)
        {
            Vector3 sp = mainCamera.WorldToScreenPoint(pts[i]);

            // check if point is in front of camera (z > 0)
            if (sp.z > 0) anyInFront = true;

            // update rectangle boundaries
            minX = Mathf.Min(minX, sp.x);
            minY = Mathf.Min(minY, sp.y);
            maxX = Mathf.Max(maxX, sp.x);
            maxY = Mathf.Max(maxY, sp.y);
        }

        // if all points are behind camera, object is not visible
        if (!anyInFront)
        {
            rect = default;
            return false;
        }

        // create rectangle from calculated boundaries
        rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
        return true;
    }
}
