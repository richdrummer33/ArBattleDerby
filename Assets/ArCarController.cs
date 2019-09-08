using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.XR.ARFoundation;

public class ArCarController : MonoBehaviour
{
    public ARRaycastManager raycastManager;

    public GameObject carPrefab;

    public GameObject enemyPrefab;

    public GameObject debugMarksd;

    GameObject currentEnemy;

    private GameObject currentCar;

    public ARPlaneManager planeManager;

    Vector3 enemySpawn;

    ARPlane spawnPlane;

    CarMotorController motor;

    List<ARPlane> allPlanes = new List<ARPlane>();

    void OnEnable()
    {
        planeManager.planesChanged += OnPlaneAdded;
    }

    void OnDisable()
    {
        planeManager.planesChanged -= OnPlaneAdded;
    }

    void Update()
    {
        Vector2 screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));

        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        raycastManager.Raycast(screenCenter, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon);

        if (Input.touchCount > 0) // Touching screen
        {
            if (hits.Count > 0)
            {
                if (currentCar == null)
                {
                    currentCar = Instantiate(carPrefab, hits[0].pose.position, Quaternion.identity);
                    motor = currentCar.GetComponent<CarMotorController>();

                    planeManager.trackables.TryGetTrackable(hits[0].trackableId, out spawnPlane);
                    StartCoroutine(RandomSpawnGenerator());

                    Debug.Log("Car placed");
                }
            }

            if (motor != null)
            {
                var touch = Input.GetTouch(0);

                if (touch.position.x < Screen.width / 3f)
                {
                    motor.SteerLeft();
                }
                else if (touch.position.x > Screen.width * (2f / 3f))
                {
                    motor.SteerRight();
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
        }
        else if (motor)
        {
            motor.StopCar();
        }

        if (!currentEnemy && allPlanes.Count > 0 && currentCar)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Debug.Log("Attempt Spawn");

        Vector3 spawnPos = enemySpawn;

        Vector3 innerDirection = spawnPlane.center - spawnPos; // Vector that points from the extremity to the center of the plane

        Quaternion spawnRot = Quaternion.Euler(innerDirection);

        currentEnemy = Instantiate(enemyPrefab, spawnPos, spawnRot);

        currentEnemy.GetComponent<EnemyAiCarController>().Init(currentCar.transform);

        Debug.Log("Spawned!!!");
    }

    private IEnumerator RandomSpawnGenerator()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);

            Vector2 screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));

            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            raycastManager.Raycast(screenCenter, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon);

            if (hits.Count > 0)
            {
                enemySpawn = hits[0].pose.position;
            }
        }
    }

    void OnPlaneAdded(ARPlanesChangedEventArgs eventArgs)
    {
        foreach (ARPlane plane in eventArgs.added)
        {
            Debug.Log("Added plane");
            allPlanes.Add(plane);
        }
    }
}
