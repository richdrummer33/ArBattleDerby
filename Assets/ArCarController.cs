using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ArCarController : MonoBehaviour
{
    public ARRaycastManager raycastManager;

    public GameObject carPrefab;

    private GameObject currentCar;

    CarMotorController motor;

    void Update()
    {
        Vector2 screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));

        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        raycastManager.Raycast(screenCenter, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon);
        
        if (Input.touchCount > 0) // Touching screen
        {
            Debug.Log("Touch held");

            if (hits.Count > 0)
            {
                if (currentCar == null)
                {
                    currentCar = Instantiate(carPrefab, hits[0].pose.position, Quaternion.identity);
                    motor = currentCar.GetComponent<CarMotorController>();
                    Debug.Log("Car placed");
                }
            }

            if (motor != null)
            {
                var touch = Input.GetTouch(0);

                if (touch.position.x < Screen.width / 3f)
                {
                    motor.SteerLeft();
                    Debug.Log("Car steer L");
                }
                else if (touch.position.x > Screen.width * (2f / 3f))
                {
                    motor.SteerRight();
                    Debug.Log("Car steer R");
                }
                else
                {
                    motor.SteerStraight();
                }
            }
        }
        else if (Input.touchCount > 1 && motor != null)
        {
            motor.SteerStraight();
            Debug.Log("Car steer Straint");
        }        
        else if (motor)
        {
            motor.StopCar();
        }
    }     
}
