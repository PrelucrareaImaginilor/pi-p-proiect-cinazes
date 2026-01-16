using UnityEngine;

public class GazeDetector : MonoBehaviour
{
    public float maxDistance = 10f;    // maximum distance for raycast detection
    public LayerMask mask = ~0;        // layer mask to filter detectable objects default: all layers
    public Material highlightMat;      // material applied to detected objects
    public ObjectDetectionUI ui;       // reference to the UI component that displays detection info

    // variables to track highlighted object
    Renderer lastRend;                 // last object's renderer that was highlighted
    Material[] lastMats;               // original materials of the last highlighted object

    /*
         Called every frame to update gaze detection
         Performs raycast, manages object highlighting and coordinates with UI
    */
    void Update()
    {
        if (ui == null)    // ensure UI reference is set
        {
            Debug.LogError("UI reference is NULL in GazeDetector!");
            return;
        }
    
        // create a ray from current position looking forward
        Ray ray = new Ray(transform.position, transform.forward);

        // perform raycast to detect objects in gaze direction
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, mask))
        {
            // get the renderer component of the hit object
            var rend = hit.collider.GetComponent<Renderer>();

            // check if we've hit a new object (different from last frame)
            if (rend != null && rend != lastRend)
            {
                // clear previous highlight before applying the new one
                Clear();

                // store reference to new object and its original materials
                lastRend = rend;
                lastMats = rend.materials;

                // create new material array with highlight material for all material slots
                Material[] mats = new Material[lastMats.Length];
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = highlightMat;

                // apply highlight materials to the object
                rend.materials = mats;

                // notify UI about the detected object
                ui.SetTarget(rend);
            }
        }
        else
        {
            // no object detected - clear highlight and UI
            Clear();
            ui.SetTarget(null);
        }
    }

    /* 
        Restores the original materials of the previously highlighted objects.
        Called when looking away from an object or when a new object is detected.
    */
    void Clear()
    {
        // check if we have a previous object to restore
        if (lastRend && lastMats != null)
            lastRend.materials = lastMats;    // restore original materials

        // clear references
        lastRend = null;
        lastMats = null;
    }
}
