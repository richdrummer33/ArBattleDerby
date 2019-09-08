﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.XR.ARFoundation;

public class ArCarController : MonoBehaviour
{
    public ARRaycastManager raycastManager;

    public GameObject carPrefab;

    public GameObject enemyPrefab;

    GameObject currentEnemy;

    private GameObject currentCar;

    public ARPlaneManager planeManager;

    CarMotorController motor;

    List<ARPlane> allPlanes = new List<ARPlane>();

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

        if (!currentEnemy && allPlanes.Count > 0 && currentCar)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Debug.Log("Attempt Spawn");

        int spawnPlaneIndex = Mathf.RoundToInt(Random.Range(0, allPlanes.Count - 1));

        ARPlane spawnPlane = allPlanes[spawnPlaneIndex];

        List<Vector3> planeVerts = spawnPlane.GetComponent<MeshCollider>().sharedMesh.vertices.ToList<Vector3>();

        int vertIndex = Mathf.RoundToInt(Random.Range(0, planeVerts.Count - 1));

        Vector3 innerDirection = spawnPlane.center - planeVerts[vertIndex]; // Vector that points from the extremity to the center of the plane

        Vector3 spawnPos = planeVerts[vertIndex] + innerDirection.normalized * 0.05f; // Spawn 5cm inside of the plane

        Quaternion spawnRot = Quaternion.Euler(innerDirection);

        currentEnemy = Instantiate(enemyPrefab, spawnPos, spawnRot);

        currentEnemy.GetComponent<EnemyAiCarController>().Init(currentCar.transform);

        Debug.Log("Spawned!!!");
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
