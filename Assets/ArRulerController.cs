using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ArRulerController : MonoBehaviour
{
    public ARRaycastManager raycastManager;

    public LineRenderer ruler;

    private LineRenderer currentRuler;
        
    void Update()
    {
        Vector2 screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        List<Transform> myTrnasforms = new List<Transform>();

        raycastManager.Raycast(screenCenter, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon);
         
        if (Input.touchCount > 0) // Touching screen
        {
            Debug.Log("Touch held");

            if (hits.Count > 0)
            {
                if (currentRuler == null)
                {
                    currentRuler = Instantiate(ruler, hits[0].pose.position, Quaternion.identity);
                    currentRuler.SetPosition(0, hits[0].pose.position);
                    Debug.Log("Ruler placed");
                }

                currentRuler.SetPosition(1, hits[0].pose.position); // Make ruler extend to where you're pointing
                Debug.Log("Ruler updating");
            }
            
        }
        else // Stopped touching screen
        {
            currentRuler = null;
            Debug.Log("Ruler stopped");
        }
    }
}
