using UnityEngine;

public class GazeDetector : MonoBehaviour
{
    public float maxDistance = 10f;
    public LayerMask mask = ~0;
    public Material highlightMat;
    public ObjectDetectionUI ui;

    Renderer lastRend;
    Material[] lastMats;

    void Update()
    {
        if (ui == null)
        {
            Debug.LogError("UI reference is NULL in GazeDetector!");
            return;
        }


        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, mask))
        {
            var rend = hit.collider.GetComponent<Renderer>();

            if (rend != null && rend != lastRend)
            {
                Clear();

                lastRend = rend;
                lastMats = rend.materials;

                Material[] mats = new Material[lastMats.Length];
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = highlightMat;

                rend.materials = mats;

                ui.SetTarget(rend);
            }
        }
        else
        {
            Clear();
            ui.SetTarget(null);
        }
    }

    void Clear()
    {
        if (lastRend && lastMats != null)
            lastRend.materials = lastMats;

        lastRend = null;
        lastMats = null;
    }
}
