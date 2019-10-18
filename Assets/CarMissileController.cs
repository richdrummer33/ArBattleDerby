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
        GameObject missile = Instantiate(missilePrefab, launchTubes[currentTube].position, Quaternion.LookRotation(launchTubes[currentTube].up), null);

        Transform target = null;

        if (enemiesInRange.Count > 0)
        {
            int randIndex = Mathf.RoundToInt(Random.Range(0f, enemiesInRange.Count - 1f));

            target = enemiesInRange[randIndex];
        }

        missile.GetComponent<MissileController>().SetTarget(target, carRb.velocity + (carRb.transform.forward + carRb.transform.up * 0.5f) * 0.25f);

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
