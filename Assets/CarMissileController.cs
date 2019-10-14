using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Launcher
public class CarMissileController : MonoBehaviour 
{
    [SerializeField]
    GameObject missilePrefab;

    [SerializeField]
    List<Transform> launchTubes;

    int currentTube = 0;

    [SerializeField]
    List<Transform> enemiesInRange = new List<Transform>();

    Rigidbody carRb;

    private void Start()
    {
        carRb = transform.root.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            LaunchMissile();
        }
    }

    public void LaunchMissile()
    {
        GameObject missile = Instantiate(missilePrefab, launchTubes[currentTube].position + transform.up * 0.1f, Quaternion.LookRotation(launchTubes[currentTube].up), null);

        int randIndex = Mathf.RoundToInt(Random.Range(0f, enemiesInRange.Count - 1f));

        Transform target = enemiesInRange[randIndex];

        Debug.Log("carRb.velocity " + carRb.velocity);
        missile.GetComponent<MissileController>().SetTarget(target, carRb.velocity);

        currentTube = (currentTube + 1) % launchTubes.Count;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            enemiesInRange.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            enemiesInRange.Remove(other.transform);
        }
    }
}
